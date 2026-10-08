using GestorCuentasCorrientes.web.Data;
using GestorCuentasCorrientes.web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GestorCuentasCorrientes.web.Controllers
{
    [Authorize]
    public class ProductosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Productos
        public async Task<IActionResult> Index(string? searchTerm)
        {
            IQueryable<Producto> query = _context.Productos.AsNoTracking();

            // Filtro por búsqueda: Nombre o Código
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p =>
                    p.Nombre.Contains(searchTerm) ||
                    (p.Codigo != null && p.Codigo.Contains(searchTerm)));
            }

            var productos = await query.OrderBy(p => p.Nombre).ToListAsync();

            ViewBag.SearchTerm = searchTerm;

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