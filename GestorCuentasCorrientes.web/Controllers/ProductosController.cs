using GestorCuentasCorrientes.web.Data;
using GestorCuentasCorrientes.web.Models;
using GestorCuentasCorrientes.web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GestorCuentasCorrientes.web.Controllers
{
    [Authorize]
    public class ProductosController : Controller
    {
        private const int TamanoPagina = 10;

        private readonly ApplicationDbContext _context;

        public ProductosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Productos
        public async Task<IActionResult> Index(string? searchTerm, int page = 1)
        {
            IQueryable<Producto> query = _context.Productos.AsNoTracking();

            // Filtro por búsqueda: Nombre o Código
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p =>
                    p.Nombre.Contains(searchTerm) ||
                    (p.Codigo != null && p.Codigo.Contains(searchTerm)));
            }

            // Paginación
            var totalItems = await query.CountAsync();
            var totalPaginas = Math.Max(1, (int)Math.Ceiling(totalItems / (double)TamanoPagina));
            page = Math.Clamp(page, 1, totalPaginas);

            var productos = await query
                .OrderBy(p => p.Nombre)
                .ThenBy(p => p.Id)
                .Skip((page - 1) * TamanoPagina)
                .Take(TamanoPagina)
                .ToListAsync();

            // En cuántos presupuestos distintos se usó cada producto de esta página
            var ids = productos.Select(p => p.Id).ToList();

            var uso = await _context.PresupuestoDetalles
                .Where(d => d.ProductoId.HasValue && ids.Contains(d.ProductoId.Value))
                .GroupBy(d => d.ProductoId!.Value)
                .Select(g => new
                {
                    ProductoId = g.Key,
                    Cantidad = g.Select(d => d.PresupuestoId).Distinct().Count()
                })
                .ToDictionaryAsync(x => x.ProductoId, x => x.Cantidad);

            ViewBag.SearchTerm = searchTerm;
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPaginas;
            ViewBag.TotalItems = totalItems;
            ViewBag.Uso = uso;

            return View(productos);
        }

        // GET: Productos/Create
        public IActionResult Create()
        {
            CargarUnidades();
            return View(new Producto());
        }

        // POST: Productos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Codigo,Nombre,UnidadMedida,PrecioUnitario")] Producto producto)
        {
            producto.Codigo = LimpiarCodigo(producto.Codigo);
            await ValidarProductoAsync(producto);

            if (ModelState.IsValid)
            {
                producto.Activo = true;

                _context.Add(producto);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            CargarUnidades(producto.UnidadMedida);
            return View(producto);
        }

        // GET: Productos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var producto = await _context.Productos.FindAsync(id);

            if (producto == null)
                return NotFound();

            CargarUnidades(producto.UnidadMedida);
            return View(producto);
        }

        // POST: Productos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,Codigo,Nombre,UnidadMedida,PrecioUnitario")] Producto producto)
        {
            if (id != producto.Id)
                return NotFound();

            producto.Codigo = LimpiarCodigo(producto.Codigo);
            await ValidarProductoAsync(producto);

            if (ModelState.IsValid)
            {
                var productoExistente = await _context.Productos.FindAsync(id);

                if (productoExistente == null)
                    return NotFound();

                productoExistente.Codigo = producto.Codigo;
                productoExistente.Nombre = producto.Nombre;
                productoExistente.UnidadMedida = producto.UnidadMedida;
                productoExistente.PrecioUnitario = producto.PrecioUnitario;

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            CargarUnidades(producto.UnidadMedida);
            return View(producto);
        }

        // POST: Productos/CambiarEstado/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            var producto = await _context.Productos.FindAsync(id);

            if (producto == null)
                return NotFound();

            producto.Activo = !producto.Activo;
            _context.Update(producto);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Productos/ActualizarPrecios
        [Authorize(Roles = "Admin")]
        public IActionResult ActualizarPrecios()
        {
            return View(new ActualizarPreciosVm());
        }

        // POST: Productos/ActualizarPrecios
        // accion = "vista" (solo muestra el resultado) o "aplicar" (guarda los cambios)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ActualizarPrecios(ActualizarPreciosVm vm, string? accion)
        {
            if (vm.Porcentaje == 0)
            {
                ModelState.AddModelError(nameof(vm.Porcentaje), "El porcentaje no puede ser 0.");
            }

            if (!ModelState.IsValid)
                return View(vm);

            var query = _context.Productos.AsQueryable();

            if (vm.SoloActivos)
                query = query.Where(p => p.Activo);

            var productos = await query
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            if (productos.Count == 0)
            {
                ModelState.AddModelError("", "No hay productos para actualizar con ese criterio.");
                return View(vm);
            }

            var factor = 1 + (vm.Porcentaje / 100m);

            // APLICAR: se recalcula acá, en el servidor, con los precios de este momento
            if (accion == "aplicar")
            {
                foreach (var p in productos)
                {
                    p.PrecioUnitario = CalcularNuevoPrecio(p.PrecioUnitario, factor);
                }

                await _context.SaveChangesAsync();

                TempData["Mensaje"] =
                    $"Se actualizó el precio de {productos.Count} producto(s) ({vm.Porcentaje:+#;-#}%).";

                return RedirectToAction(nameof(Index));
            }

            // VISTA PREVIA: no se guarda nada
            vm.Vista = productos
                .Select(p => new PrecioPreviewVm
                {
                    Codigo = p.Codigo,
                    Nombre = p.Nombre,
                    PrecioActual = p.PrecioUnitario,
                    PrecioNuevo = CalcularNuevoPrecio(p.PrecioUnitario, factor)
                })
                .ToList();

            return View(vm);
        }

        private static decimal CalcularNuevoPrecio(decimal actual, decimal factor)
        {
            var nuevo = Math.Round(actual * factor, 2, MidpointRounding.AwayFromZero);

            // Respeta los límites del modelo (mínimo 0,01 y máximo de la columna)
            return Math.Clamp(nuevo, 0.01m, 999999999.99m);
        }

        private void CargarUnidades(string? seleccionada = null)
        {
            ViewBag.Unidades = new SelectList(Producto.Unidades, seleccionada);
        }

        // Código vacío → null (así no choca con el índice único)
        private static string? LimpiarCodigo(string? codigo) =>
            string.IsNullOrWhiteSpace(codigo) ? null : codigo.Trim();

        private async Task ValidarProductoAsync(Producto producto)
        {
            // Evita que alguien manipule el formulario con una unidad inventada
            if (!Producto.Unidades.Contains(producto.UnidadMedida))
            {
                ModelState.AddModelError(
                    nameof(producto.UnidadMedida),
                    "Seleccioná una unidad de medida válida.");
            }

            // Mensaje claro antes de que lo frene el índice único de la base
            if (producto.Codigo != null)
            {
                var codigoEnUso = await _context.Productos
                    .AnyAsync(p => p.Codigo == producto.Codigo && p.Id != producto.Id);

                if (codigoEnUso)
                {
                    ModelState.AddModelError(
                        nameof(producto.Codigo),
                        "Ya existe un producto con ese código.");
                }
            }
        }

        private bool ProductoExists(int id)
        {
            return _context.Productos.Any(e => e.Id == id);
        }
    }
}