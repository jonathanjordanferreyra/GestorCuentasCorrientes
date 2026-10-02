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
        public async Task<IActionResult> Create(int? clienteId)
        {
            var viewModel = new PresupuestoCreateVm
            {
                Fecha = DateTime.Now
            };

            // Si venimos desde un cliente, dejarlo preseleccionado
            if (clienteId.HasValue)
            {
                var cliente = await _context.Clientes
                    .FirstOrDefaultAsync(c => c.Id == clienteId.Value && c.Activo);

                if (cliente != null)
                {
                    viewModel.ClienteId = cliente.Id;
                }
            }

            await CargarClientes(viewModel.ClienteId);

            // Empezamos con una línea de detalle
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
            // Eliminar líneas completamente vacías
            viewModel.Detalles = viewModel.Detalles?
                .Where(d =>
                    !string.IsNullOrWhiteSpace(d.Descripcion) ||
                    d.Cantidad > 0 ||
                    d.PrecioUnitario > 0)
                .ToList()
                ?? new List<PresupuestoDetalleVm>();

            // Debe existir al menos un detalle
            if (!viewModel.Detalles.Any())
            {
                ModelState.AddModelError(
                    "Detalles",
                    "Debe agregar al menos un concepto al presupuesto.");
            }

            // Verificar que el cliente exista y esté activo
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

            if (!ModelState.IsValid)
            {
                await CargarClientes(viewModel.ClienteId);

                if (!viewModel.Detalles.Any())
                {
                    viewModel.Detalles.Add(new PresupuestoDetalleVm());
                }

                return View(viewModel);
            }

            // Obtener usuario actual
            var usuarioId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(usuarioId))
            {
                ModelState.AddModelError(
                    "",
                    "No se pudo identificar al usuario.");

                await CargarClientes(viewModel.ClienteId);

                return View(viewModel);
            }

            // Crear presupuesto
            var presupuesto = new Presupuesto
            {
                Fecha = viewModel.Fecha,
                Observaciones = viewModel.Observaciones,
                Estado = "Pendiente",
                FechaRegistro = DateTime.Now,
                ClienteId = viewModel.ClienteId,
                UsuarioId = usuarioId
            };

            // Crear detalles
            foreach (var detalleVm in viewModel.Detalles)
            {
                var detalle = new PresupuestoDetalle
                {
                    Descripcion = detalleVm.Descripcion,
                    Cantidad = detalleVm.Cantidad,
                    PrecioUnitario = detalleVm.PrecioUnitario
                };

                presupuesto.Detalles.Add(detalle);
            }

            // Guardar presupuesto y detalles
            _context.Presupuestos.Add(presupuesto);

            await _context.SaveChangesAsync();

            // El Id ya fue generado por SQL Server
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
    }
}