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
    public class RecibosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Usuario> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public RecibosController(
            ApplicationDbContext context,
            UserManager<Usuario> userManager,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Recibos/Create
        public async Task<IActionResult> Create(int clienteId)
        {
            var cliente = await _context.Clientes.FindAsync(clienteId);
            if (cliente == null)
                return NotFound();

            var viewModel = new ReciboCreateVm
            {
                Fecha = DateTime.Now,
                ClienteId = cliente.Id,
                ClienteNombre = cliente.RazonSocial
            };

            await CargarMediosPagoAsync();

            return View(viewModel);
        }

        private async Task CargarMediosPagoAsync()
        {
            ViewBag.MediosPago = new SelectList(
                await _context.MediosPago
                    .OrderBy(mp => mp.Nombre)
                    .ToListAsync(),
                "Id",
                "Nombre");
        }
    }
}