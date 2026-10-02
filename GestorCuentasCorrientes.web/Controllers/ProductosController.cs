using GestorCuentasCorrientes.web.Data;
using GestorCuentasCorrientes.web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

            // Filtro por búsqueda: Nombre
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p => p.Nombre.Contains(searchTerm));
            }

            // Ordenar por Nombre
            var productos = await query.OrderBy(p => p.Nombre).ToListAsync();

            ViewBag.SearchTerm = searchTerm;

            return View(productos);
        }

        // GET: Productos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Productos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre,PrecioUnitario")] Producto producto)
        {
            if (ModelState.IsValid)
            {
                // Asignar valor por defecto
                producto.Activo = true;

                _context.Add(producto);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

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

            return View(producto);
        }

        // POST: Productos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,PrecioUnitario")] Producto producto)
        {
            if (id != producto.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                var productoExistente = await _context.Productos.FindAsync(id);

                if (productoExistente == null)
                    return NotFound();

                productoExistente.Nombre = producto.Nombre;
                productoExistente.PrecioUnitario = producto.PrecioUnitario;

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

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

        private bool ProductoExists(int id)
        {
            return _context.Productos.Any(e => e.Id == id);
        }
    }
}
