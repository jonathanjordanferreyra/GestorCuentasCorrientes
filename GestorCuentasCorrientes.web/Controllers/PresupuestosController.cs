using GestorCuentasCorrientes.web.Data;
using GestorCuentasCorrientes.web.Models;
using GestorCuentasCorrientes.web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GestorCuentasCorrientes.web.Controllers
{
    [Authorize]
    public class PresupuestosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Usuario> _userManager;

        public PresupuestosController(
            ApplicationDbContext context,
            UserManager<Usuario> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Presupuestos/Create

        // GET: Presupuestos/Create
        public async Task<IActionResult> Create(int? clienteId)
        {
            var viewModel = new PresupuestoCreateVm
            {
                Fecha = DateTime.Now
            };

            if (clienteId.HasValue)
            {
                var cliente = await _context.Clientes
                    .FirstOrDefaultAsync(c =>
                        c.Id == clienteId.Value &&
                        c.Activo);

                if (cliente != null)
                {
                    viewModel.ClienteId = cliente.Id;
                }
            }

            await CargarClientes(viewModel.ClienteId);
            await CargarProductos();

            if (!viewModel.Detalles.Any())
            {
                viewModel.Detalles.Add(new PresupuestoDetalleVm());
            }

            return View(viewModel);
        }
        // POST: Presupuestos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PresupuestoCreateVm viewModel)
        {
            // Eliminar líneas sin producto o con cantidad inválida
            viewModel.Detalles = viewModel.Detalles?
                .Where(d => d.ProductoId > 0 && d.Cantidad > 0)
                .ToList()
                ?? new List<PresupuestoDetalleVm>();

            if (!viewModel.Detalles.Any())
            {
                ModelState.AddModelError(
                    "Detalles",
                    "Debe agregar al menos un producto al presupuesto.");
            }

            // Validar cliente
            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c =>
                    c.Id == viewModel.ClienteId &&
                    c.Activo);

            if (cliente == null)
            {
                ModelState.AddModelError(
                    nameof(viewModel.ClienteId),
                    "El cliente no existe o está inactivo.");
            }

            // Buscar todos los productos seleccionados
            var productoIds = viewModel.Detalles
                .Select(d => d.ProductoId)
                .Distinct()
                .ToList();

            var productos = await _context.Productos
                .Where(p =>
                    productoIds.Contains(p.Id) &&
                    p.Activo)
                .ToDictionaryAsync(p => p.Id);

            // Validar que todos los productos existan y estén activos
            foreach (var detalleVm in viewModel.Detalles)
            {
                if (!productos.ContainsKey(detalleVm.ProductoId))
                {
                    ModelState.AddModelError(
                        "Detalles",
                        "Uno de los productos seleccionados no existe o está inactivo.");
                }
            }

            if (!ModelState.IsValid)
            {
                await CargarClientes(viewModel.ClienteId);
                await CargarProductos();

                if (!viewModel.Detalles.Any())
                {
                    viewModel.Detalles.Add(new PresupuestoDetalleVm());
                }

                return View(viewModel);
            }

            var usuarioId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(usuarioId))
            {
                ModelState.AddModelError(
                    "",
                    "No se pudo identificar al usuario.");

                await CargarClientes(viewModel.ClienteId);
                await CargarProductos();

                return View(viewModel);
            }

            var presupuesto = new Presupuesto
            {
                Fecha = viewModel.Fecha,
                Observaciones = viewModel.Observaciones,
                Estado = "Pendiente",
                FechaRegistro = DateTime.Now,
                ClienteId = viewModel.ClienteId,
                UsuarioId = usuarioId
            };

            foreach (var detalleVm in viewModel.Detalles)
            {
                var producto = productos[detalleVm.ProductoId];

                var detalle = new PresupuestoDetalle
                {
                    Descripcion = producto.Nombre,
                    Cantidad = detalleVm.Cantidad,
                    PrecioUnitario = producto.PrecioUnitario
                };

                presupuesto.Detalles.Add(detalle);
            }

            _context.Presupuestos.Add(presupuesto);

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(Details),
                new { id = presupuesto.Id });
        }

        // GET: Presupuestos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var presupuesto = await _context.Presupuestos
                .Include(p => p.Cliente)
                .Include(p => p.Usuario)
                .Include(p => p.Detalles)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (presupuesto == null)
                return NotFound();

            return View(presupuesto);
        }

        // Cargar clientes para el Select
        private async Task CargarClientes(int? clienteId = null)
        {
            var clientes = await _context.Clientes
                .Where(c => c.Activo)
                .OrderBy(c => c.RazonSocial)
                .ToListAsync();

            ViewBag.ClienteId = new SelectList(
                clientes,
                "Id",
                "RazonSocial",
                clienteId);
        }
        private async Task CargarProductos()
        {
            var productos = await _context.Productos
                .Where(p => p.Activo)
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            ViewBag.Productos = productos;
        }
    }
}