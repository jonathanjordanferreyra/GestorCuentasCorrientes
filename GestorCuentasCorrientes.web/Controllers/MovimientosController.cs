using GestorCuentasCorrientes.web.Data;
using GestorCuentasCorrientes.web.Models;
using GestorCuentasCorrientes.web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GestorCuentasCorrientes.web.Controllers
{
    [Authorize]
    public class MovimientosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Usuario> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public MovimientosController(
            ApplicationDbContext context,
            UserManager<Usuario> userManager,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Movimientos/Create
        public async Task<IActionResult> Create(int? clienteId)
        {
            var viewModel = new MovimientoSimpleCreateVm
            {
                Fecha = DateTime.Now
            };

            // Si vino clienteId, precargá y bloqueá el cliente
            if (clienteId.HasValue)
            {
                var cliente = await _context.Clientes.FindAsync(clienteId);
                if (cliente == null)
                    return NotFound();

                viewModel.ClienteId = cliente.Id;
                viewModel.ClienteNombre = cliente.RazonSocial;
            }
            else
            {
                // Mostrar dropdown con clientes activos
                ViewBag.Clientes = new SelectList(
                    await _context.Clientes
                        .Where(c => c.Activo)
                        .OrderBy(c => c.RazonSocial)
                        .ToListAsync(),
                    "Id",
                    "RazonSocial");
            }

            // Dropdown de TipoMovimiento, excluyendo "REC" (Recibo)
            ViewBag.TiposMovimiento = new SelectList(
                await _context.TiposMovimiento
                    .Where(tm => tm.Codigo != "REC")
                    .OrderBy(tm => tm.Nombre)
                    .ToListAsync(),
                "Id",
                "Nombre");

            return View(viewModel);
        }

        // POST: Movimientos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MovimientoSimpleCreateVm viewModel)
        {
            if (!ModelState.IsValid)
            {
                // Repoblar los dropdowns en caso de error
                if (viewModel.ClienteId == 0)
                {
                    ViewBag.Clientes = new SelectList(
                        await _context.Clientes
                            .Where(c => c.Activo)
                            .OrderBy(c => c.RazonSocial)
                            .ToListAsync(),
                        "Id",
                        "RazonSocial",
                        viewModel.ClienteId);
                }

                ViewBag.TiposMovimiento = new SelectList(
                    await _context.TiposMovimiento
                        .Where(tm => tm.Codigo != "REC")
                        .OrderBy(tm => tm.Nombre)
                        .ToListAsync(),
                    "Id",
                    "Nombre",
                    viewModel.TipoMovimientoId);

                return View(viewModel);
            }

            // Verificar que el cliente existe y está activo
            var cliente = await _context.Clientes.FindAsync(viewModel.ClienteId);
            if (cliente == null || !cliente.Activo)
            {
                ModelState.AddModelError("", "El cliente no existe o está inactivo.");
                ViewBag.TiposMovimiento = new SelectList(
                    await _context.TiposMovimiento
                        .Where(tm => tm.Codigo != "REC")
                        .OrderBy(tm => tm.Nombre)
                        .ToListAsync(),
                    "Id",
                    "Nombre");
                return View(viewModel);
            }

            // Verificar que el tipo de movimiento existe y no es "REC"
            var tipoMovimiento = await _context.TiposMovimiento.FindAsync(viewModel.TipoMovimientoId);
            if (tipoMovimiento == null || tipoMovimiento.Codigo == "REC")
            {
                ModelState.AddModelError("", "Tipo de movimiento inválido.");
                return View(viewModel);
            }

            // Obtener UsuarioId del usuario logueado
            var usuarioId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(usuarioId))
            {
                ModelState.AddModelError("", "No se pudo identificar el usuario.");
                return View(viewModel);
            }

            // Crear el Movimiento
            var movimiento = new Movimiento
            {
                ClienteId = viewModel.ClienteId,
                TipoMovimientoId = viewModel.TipoMovimientoId,
                Fecha = viewModel.Fecha,
                NumeroComprobante = viewModel.NumeroComprobante,
                Importe = viewModel.Importe,
                Observaciones = viewModel.Observaciones,
                UsuarioId = usuarioId,
                FechaRegistro = DateTime.Now,
                Anulado = false
            };

            _context.Movimientos.Add(movimiento);
            await _context.SaveChangesAsync();

            // Procesar archivo comprobante si viene
            if (viewModel.ArchivoComprobante != null && viewModel.ArchivoComprobante.Length > 0)
            {
                // Crear carpeta App_Data/Comprobantes/{movimientoId} si no existe
                var appDataPath = Path.Combine(_webHostEnvironment.ContentRootPath, "App_Data", "Comprobantes", movimiento.Id.ToString());
                Directory.CreateDirectory(appDataPath);

                // Generar nombre único para el archivo
                var fileName = Path.GetFileName(viewModel.ArchivoComprobante.FileName);
                var filePath = Path.Combine(appDataPath, fileName);

                // Guardar archivo
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await viewModel.ArchivoComprobante.CopyToAsync(stream);
                }

                // Crear registro en Comprobantes
                // Guardar solo la ruta relativa: App_Data/Comprobantes/{movimientoId}/{fileName}
                var rutaRelativa = Path.Combine("App_Data", "Comprobantes", movimiento.Id.ToString(), fileName);

                var comprobante = new Comprobante
                {
                    MovimientoId = movimiento.Id,
                    NombreArchivo = fileName,
                    RutaArchivo = rutaRelativa,
                    TipoArchivo = Path.GetExtension(fileName).TrimStart('.'),
                    FechaCarga = DateTime.Now
                };

                _context.Comprobantes.Add(comprobante);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Details", "Clientes", new { id = viewModel.ClienteId });
        }

        // GET: Movimientos/VerComprobante/5
        public async Task<IActionResult> VerComprobante(int? id)
        {
            if (id == null)
                return NotFound();

            var comprobante = await _context.Comprobantes.FindAsync(id);
            if (comprobante == null)
                return NotFound();

            // Reconstruir la ruta completa combinando ContentRootPath + ruta relativa
            var rutaCompleta = Path.Combine(_webHostEnvironment.ContentRootPath, comprobante.RutaArchivo);

            // Verificar que el archivo existe
            if (!System.IO.File.Exists(rutaCompleta))
                return NotFound();

            // Determinar el content-type basado en la extensión
            var contentType = ObtenerContentType(comprobante.TipoArchivo);

            // Devolver el archivo
            var fileStream = System.IO.File.OpenRead(rutaCompleta);
            return File(fileStream, contentType, comprobante.NombreArchivo);
        }

        // Método helper para obtener el content-type
        private string ObtenerContentType(string? extension)
        {
            return extension?.ToLower() switch
            {
                "pdf" => "application/pdf",
                "jpg" or "jpeg" => "image/jpeg",
                "png" => "image/png",
                "gif" => "image/gif",
                "doc" => "application/msword",
                "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "xls" => "application/vnd.ms-excel",
                "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "txt" => "text/plain",
                _ => "application/octet-stream"
            };
            
        }
    }
}
