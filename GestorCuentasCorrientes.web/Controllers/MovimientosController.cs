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
        public async Task<IActionResult> Create(int clienteId)
        {
            var cliente = await _context.Clientes.FindAsync(clienteId);
            if (cliente == null)
                return NotFound();

            // Usar MovimientoRecibosCreateVm para soportar tanto movimientos simples como Recibos
            var viewModel = new MovimientoRecibosCreateVm
            {
                Fecha = DateTime.Now,
                ClienteId = cliente.Id,
                ClienteNombre = cliente.RazonSocial
            };

            // Cargar todos los tipos de movimiento (incluyendo REC ahora)
            ViewBag.TiposMovimiento = new SelectList(
                await _context.TiposMovimiento
                    .OrderBy(tm => tm.Nombre)
                    .ToListAsync(),
                "Id",
                "Nombre");

            // Cargar medios de pago para los recibos
            ViewBag.MediosPago = new SelectList(
                await _context.MediosPago
                    .OrderBy(mp => mp.Nombre)
                    .ToListAsync(),
                "Id",
                "Nombre");

            return View(viewModel);
        }

        // POST: Movimientos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MovimientoRecibosCreateVm viewModel)
        {
            System.Diagnostics.Debug.WriteLine($"🔵 POST Create iniciado. ModelState.IsValid = {ModelState.IsValid}");

            // Obtener el tipo de movimiento para saber si es Recibo o no
            var tipoMovimiento = await _context.TiposMovimiento.FindAsync(viewModel.TipoMovimientoId);

            // Si es un Recibo, limpiar la lista de pagos si está vacía y asignar Importe dummy
            if (tipoMovimiento != null && tipoMovimiento.Codigo == "REC")
            {
                ModelState.Remove(nameof(viewModel.Importe));

                System.Diagnostics.Debug.WriteLine("🟡 Es RECIBO, limpiando Pagos vacíos y asignando Importe dummy");
                // Filtrar líneas de pago válidas
                viewModel.Pagos = viewModel.Pagos?.Where(p => p.MedioPagoId > 0 && p.Importe > 0).ToList() ?? new List<PagoLineaVm>();
                // Asignar un valor dummy para pasar la validación (el real se calcula en CrearRecibo)
                viewModel.Importe = viewModel.Pagos.Sum(p => p.Importe);

            }
            // Si NO es un Recibo, limpiar la lista de pagos para evitar validaciones innecesarias
            else if (tipoMovimiento != null && tipoMovimiento.Codigo != "REC")
            {
                System.Diagnostics.Debug.WriteLine("🟡 Es movimiento SIMPLE, limpiando lista de Pagos");
                viewModel.Pagos = new List<PagoLineaVm>();
            }

            if (!ModelState.IsValid)
            {
                System.Diagnostics.Debug.WriteLine("🔴 ModelState no válido. Errores:");
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine($"   - {error.ErrorMessage}");
                    }
                }

                // Reconstruir el nombre del cliente (es de solo lectura, no viaja en el POST)
                var clientePreseleccionado = await _context.Clientes.FindAsync(viewModel.ClienteId);
                viewModel.ClienteNombre = clientePreseleccionado?.RazonSocial ?? string.Empty;

                ViewBag.TiposMovimiento = new SelectList(
                    await _context.TiposMovimiento
                        .OrderBy(tm => tm.Nombre)
                        .ToListAsync(),
                    "Id",
                    "Nombre",
                    viewModel.TipoMovimientoId);

                ViewBag.MediosPago = new SelectList(
                    await _context.MediosPago
                        .OrderBy(mp => mp.Nombre)
                        .ToListAsync(),
                    "Id",
                    "Nombre");

                return View(viewModel);
            }

            System.Diagnostics.Debug.WriteLine($"✅ ModelState válido. ClienteId={viewModel.ClienteId}, TipoMovimientoId={viewModel.TipoMovimientoId}, Importe={viewModel.Importe}");

            // Verificar que el cliente existe y está activo
            var cliente = await _context.Clientes.FindAsync(viewModel.ClienteId);
            if (cliente == null || !cliente.Activo)
            {
                ModelState.AddModelError("", "El cliente no existe o está inactivo.");
                ViewBag.TiposMovimiento = new SelectList(
                    await _context.TiposMovimiento
                        .OrderBy(tm => tm.Nombre)
                        .ToListAsync(),
                    "Id",
                    "Nombre");
                ViewBag.MediosPago = new SelectList(
                    await _context.MediosPago
                        .OrderBy(mp => mp.Nombre)
                        .ToListAsync(),
                    "Id",
                    "Nombre");
                return View(viewModel);
            }

            // Verificar que el tipo de movimiento existe
            if (tipoMovimiento == null)
            {
                ModelState.AddModelError("", "Tipo de movimiento inválido.");
                return View(viewModel);
            }

            // Evitar cargar dos veces el mismo comprobante para el mismo tipo de movimiento
            if (!string.IsNullOrWhiteSpace(viewModel.NumeroComprobante))
            {
                var yaExiste = await _context.Movimientos.AnyAsync(m =>
                    m.TipoMovimientoId == viewModel.TipoMovimientoId &&
                    m.NumeroComprobante == viewModel.NumeroComprobante);

                if (yaExiste)
                {
                    ModelState.AddModelError("", $"Ya existe un movimiento de este tipo con el número de comprobante '{viewModel.NumeroComprobante}'.");

                    var clientePreseleccionado = await _context.Clientes.FindAsync(viewModel.ClienteId);
                    viewModel.ClienteNombre = clientePreseleccionado?.RazonSocial ?? string.Empty;

                    ViewBag.TiposMovimiento = new SelectList(
                        await _context.TiposMovimiento
                            .OrderBy(tm => tm.Nombre)
                            .ToListAsync(),
                        "Id",
                        "Nombre",
                        viewModel.TipoMovimientoId);

                    ViewBag.MediosPago = new SelectList(
                        await _context.MediosPago
                            .OrderBy(mp => mp.Nombre)
                            .ToListAsync(),
                        "Id",
                        "Nombre");

                    return View(viewModel);
                }
            }

            var usuarioId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(usuarioId))
            {
                System.Diagnostics.Debug.WriteLine("🔴 No se pudo obtener el usuarioId");
                ModelState.AddModelError("", "No se pudo identificar el usuario.");
                return View(viewModel);
            }

            System.Diagnostics.Debug.WriteLine($"✅ UsuarioId obtenido: {usuarioId}");

            // ============================================
            // RAMA 1: Tipo de movimiento "Recibo" (REC)
            // ============================================
            if (tipoMovimiento.Codigo == "REC")
            {
                System.Diagnostics.Debug.WriteLine("🟡 Es un RECIBO. Llamando a CrearRecibo()");
                return await CrearRecibo(viewModel, tipoMovimiento, usuarioId);
            }

            System.Diagnostics.Debug.WriteLine("🟡 Es un movimiento SIMPLE. Continuando con la rama normal");

            // ============================================
            // RAMA 2: Movimientos simples (todos los demás)
            // ============================================
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

            if (viewModel.ArchivoComprobante != null && viewModel.ArchivoComprobante.Length > 0)
            {
                var appDataPath = Path.Combine(_webHostEnvironment.ContentRootPath, "App_Data", "Comprobantes", movimiento.Id.ToString());
                Directory.CreateDirectory(appDataPath);

                var fileName = Path.GetFileName(viewModel.ArchivoComprobante.FileName);
                var filePath = Path.Combine(appDataPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await viewModel.ArchivoComprobante.CopyToAsync(stream);
                }

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

            System.Diagnostics.Debug.WriteLine($"✅✅✅ MOVIMIENTO GUARDADO EXITOSAMENTE. ID={movimiento.Id}, ClienteId={viewModel.ClienteId}");
            System.Diagnostics.Debug.WriteLine($"🟢 Redirigiendo a Details del cliente...");

            return RedirectToAction("Details", "Clientes", new { id = viewModel.ClienteId });
        }

        /// <summary>
        /// Maneja la creación de un Recibo (tipo movimiento "REC") con múltiples líneas de pago.
        /// Valida que haya al menos una línea de pago válida, calcula el importe total como suma de líneas,
        /// crea el Movimiento, y luego los registros de Pagos y Cheques (si aplica).
        /// </summary>
        private async Task<IActionResult> CrearRecibo(MovimientoRecibosCreateVm viewModel, TipoMovimiento tipoMovimiento, string usuarioId)
        {
            // Validar que haya al menos una línea de pago con medio + importe válidos
            viewModel.Pagos = viewModel.Pagos?.Where(p => p.MedioPagoId > 0 && p.Importe > 0).ToList() ?? new List<PagoLineaVm>();

            if (!viewModel.Pagos.Any())
            {
                ModelState.AddModelError("", "Debe cargar al menos una línea de pago con medio y importe válidos.");
                viewModel.ClienteNombre = (await _context.Clientes.FindAsync(viewModel.ClienteId))?.RazonSocial ?? string.Empty;
                ViewBag.TiposMovimiento = new SelectList(
                    await _context.TiposMovimiento.OrderBy(tm => tm.Nombre).ToListAsync(),
                    "Id", "Nombre", viewModel.TipoMovimientoId);
                ViewBag.MediosPago = new SelectList(
                    await _context.MediosPago.OrderBy(mp => mp.Nombre).ToListAsync(),
                    "Id", "Nombre");
                return View("Create", viewModel);
            }

            // Calcular importe total como suma de líneas
            var importeTotal = viewModel.Pagos.Sum(p => p.Importe);

            // Validar que el comprobante sea único para tipo Recibo (igual que en movimientos simples)
            if (!string.IsNullOrWhiteSpace(viewModel.NumeroComprobante))
            {
                var yaExiste = await _context.Movimientos.AnyAsync(m =>
                    m.TipoMovimientoId == viewModel.TipoMovimientoId &&
                    m.NumeroComprobante == viewModel.NumeroComprobante);

                if (yaExiste)
                {
                    ModelState.AddModelError("", $"Ya existe un recibo con el número de comprobante '{viewModel.NumeroComprobante}'.");
                    viewModel.ClienteNombre = (await _context.Clientes.FindAsync(viewModel.ClienteId))?.RazonSocial ?? string.Empty;
                    ViewBag.TiposMovimiento = new SelectList(
                        await _context.TiposMovimiento.OrderBy(tm => tm.Nombre).ToListAsync(),
                        "Id", "Nombre", viewModel.TipoMovimientoId);
                    ViewBag.MediosPago = new SelectList(
                        await _context.MediosPago.OrderBy(mp => mp.Nombre).ToListAsync(),
                        "Id", "Nombre");
                    return View("Create", viewModel);
                }
            }

            // Obtener diccionario de medios de pago para validación de cheques
            var mediosPago = await _context.MediosPago.ToDictionaryAsync(m => m.Id, m => m.Nombre);

            // Validar campos de cheque para líneas que lo requieran
            for (int i = 0; i < viewModel.Pagos.Count; i++)
            {
                var linea = viewModel.Pagos[i];
                var nombreMedio = mediosPago.GetValueOrDefault(linea.MedioPagoId);
                bool esCheque = nombreMedio is "Cheque" or "E-cheque";

                if (esCheque)
                {
                    if (string.IsNullOrWhiteSpace(linea.ChequeNumero))
                        ModelState.AddModelError($"Pagos[{i}].ChequeNumero", "El número de cheque es obligatorio.");
                    if (linea.ChequeFechaEmision is null)
                        ModelState.AddModelError($"Pagos[{i}].ChequeFechaEmision", "La fecha de emisión es obligatoria.");
                    if (linea.ChequeFechaCobro is null)
                        ModelState.AddModelError($"Pagos[{i}].ChequeFechaCobro", "La fecha de cobro es obligatoria.");
                }
            }

            if (!ModelState.IsValid)
            {
                viewModel.ClienteNombre = (await _context.Clientes.FindAsync(viewModel.ClienteId))?.RazonSocial ?? string.Empty;
                ViewBag.TiposMovimiento = new SelectList(
                    await _context.TiposMovimiento.OrderBy(tm => tm.Nombre).ToListAsync(),
                    "Id", "Nombre", viewModel.TipoMovimientoId);
                ViewBag.MediosPago = new SelectList(
                    await _context.MediosPago.OrderBy(mp => mp.Nombre).ToListAsync(),
                    "Id", "Nombre");
                return View("Create", viewModel);
            }

            // Crear el Movimiento de tipo Recibo
            var movimiento = new Movimiento
            {
                ClienteId = viewModel.ClienteId,
                TipoMovimientoId = viewModel.TipoMovimientoId,
                Fecha = viewModel.Fecha,
                NumeroComprobante = viewModel.NumeroComprobante,
                Importe = importeTotal,  // Suma de todas las líneas
                Observaciones = viewModel.Observaciones,
                UsuarioId = usuarioId,
                FechaRegistro = DateTime.Now,
                Anulado = false
            };

            _context.Movimientos.Add(movimiento);
            await _context.SaveChangesAsync();

            // Crear registros de Pagos y Cheques
            foreach (var linea in viewModel.Pagos)
            {
                var pago = new Pago
                {
                    MovimientoId = movimiento.Id,
                    MedioPagoId = linea.MedioPagoId,
                    Importe = linea.Importe
                };

                _context.Pagos.Add(pago);
                await _context.SaveChangesAsync();

                // Si el medio de pago es Cheque o E-cheque, crear registro de Cheque
                var nombreMedio = mediosPago.GetValueOrDefault(linea.MedioPagoId);
                if (nombreMedio is "Cheque" or "E-cheque")
                {
                    var cheque = new Cheque
                    {
                        PagoId = pago.Id,
                        Numero = linea.ChequeNumero,
                        Banco = linea.ChequeBanco,
                        Titular = linea.ChequeTitular,
                        FechaEmision = linea.ChequeFechaEmision.Value,
                        FechaCobro = linea.ChequeFechaCobro.Value,
                        Estado = "EnCartera"
                    };

                    _context.Cheques.Add(cheque);
                    await _context.SaveChangesAsync();
                }
            }

            // Guardar archivo comprobante si lo hay
            if (viewModel.ArchivoComprobante != null && viewModel.ArchivoComprobante.Length > 0)
            {
                var appDataPath = Path.Combine(_webHostEnvironment.ContentRootPath, "App_Data", "Comprobantes", movimiento.Id.ToString());
                Directory.CreateDirectory(appDataPath);

                var fileName = Path.GetFileName(viewModel.ArchivoComprobante.FileName);
                var filePath = Path.Combine(appDataPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await viewModel.ArchivoComprobante.CopyToAsync(stream);
                }

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
