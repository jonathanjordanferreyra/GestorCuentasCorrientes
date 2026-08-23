using GestorCuentasCorrientes.web.Data;
using GestorCuentasCorrientes.web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GestorCuentasCorrientes.web.Controllers
{
    [Authorize]
    public class ClientesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClientesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Clientes
        public async Task<IActionResult> Index(string? searchTerm, int? localidadId, int? estado)
        {
            IQueryable<Cliente> query = _context.Clientes
                .Include(c => c.Localidad)
                .AsNoTracking();

            // Filtro por búsqueda: RazonSocial o CuitDni
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(c => c.RazonSocial.Contains(searchTerm) || 
                                        c.CuitDni.Contains(searchTerm));
            }

            // Filtro por Localidad
            if (localidadId.HasValue && localidadId > 0)
            {
                query = query.Where(c => c.LocalidadId == localidadId);
            }

            // Filtro por estado (1 = Activos, 0 = Inactivos, null/otro = Todos)
            if (estado.HasValue)
            {
                if (estado == 1)
                    query = query.Where(c => c.Activo);
                else if (estado == 0)
                    query = query.Where(c => !c.Activo);
            }

            // Ordenar por RazonSocial
            var clientes = await query.OrderBy(c => c.RazonSocial).ToListAsync();

            // Datos para los dropdowns
            ViewBag.Localidades = new SelectList(
                await _context.Localidades.OrderBy(l => l.Nombre).ToListAsync(),
                "Id",
                "Nombre",
                localidadId);

            ViewBag.SearchTerm = searchTerm;
            ViewBag.LocalidadId = localidadId;
            ViewBag.Estado = estado;

            return View(clientes);
        }

        // GET: Clientes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var cliente = await _context.Clientes
                .Include(c => c.Localidad)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (cliente == null)
                return NotFound();

            return View(cliente);
        }

        // GET: Clientes/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.LocalidadId = new SelectList(
                await _context.Localidades.OrderBy(l => l.Nombre).ToListAsync(),
                "Id",
                "Nombre");

            return View();
        }

        // POST: Clientes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RazonSocial,CuitDni,Direccion,Telefono,Email,LocalidadId")] Cliente cliente)
        {
            if (ModelState.IsValid)
            {
                // Asignar valores por defecto
                cliente.Activo = true;
                cliente.FechaAlta = DateTime.Now;

                _context.Add(cliente);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewBag.LocalidadId = new SelectList(
                await _context.Localidades.OrderBy(l => l.Nombre).ToListAsync(),
                "Id",
                "Nombre",
                cliente.LocalidadId);

            return View(cliente);
        }

        // GET: Clientes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente == null)
                return NotFound();

            ViewBag.LocalidadId = new SelectList(
                await _context.Localidades.OrderBy(l => l.Nombre).ToListAsync(),
                "Id",
                "Nombre",
                cliente.LocalidadId);

            return View(cliente);
        }

        // POST: Clientes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,RazonSocial,CuitDni,Direccion,Telefono,Email,Activo,FechaAlta,LocalidadId")] Cliente cliente)
        {
            if (id != cliente.Id)
                return NotFound();

            // Capturar FechaAlta del cliente existente (no permitir edición)
            var clienteOriginal = await _context.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            if (clienteOriginal != null)
            {
                cliente.FechaAlta = clienteOriginal.FechaAlta;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cliente);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClienteExists(cliente.Id))
                        return NotFound();
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewBag.LocalidadId = new SelectList(
                await _context.Localidades.OrderBy(l => l.Nombre).ToListAsync(),
                "Id",
                "Nombre",
                cliente.LocalidadId);

            return View(cliente);
        }

        // POST: Clientes/CambiarEstado/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente == null)
                return NotFound();

            cliente.Activo = !cliente.Activo;
            _context.Update(cliente);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool ClienteExists(int id)
        {
            return _context.Clientes.Any(e => e.Id == id);
        }
    }
}
