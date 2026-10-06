using GestorCuentasCorrientes.web.Data;
using GestorCuentasCorrientes.web.Models;
using GestorCuentasCorrientes.web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;


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

        // GET: Presupuestos
        public async Task<IActionResult> Index()
        {
            var presupuestos = await _context.Presupuestos
                .Include(p => p.Cliente)
                .Include(p => p.Detalles)
                .AsNoTracking()
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            return View(presupuestos);
        }

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

        // GET: Presupuestos/Pdf/5
        public async Task<IActionResult> Pdf(int? id)
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

            decimal total = presupuesto.Detalles
                .Sum(d => d.Cantidad * d.PrecioUnitario);

            // Paleta de colores (solo visual)
            const string colorPrincipal = "#1F3A5F";
            const string colorAcento = "#0D6EFD";
            const string colorTextoSuave = "#6C757D";
            const string colorBorde = "#DEE2E6";
            const string colorFondoSuave = "#F5F7FA";

            var documento = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);

                    page.DefaultTextStyle(x =>
                        x.FontSize(10).FontColor("#212529"));

                    // ENCABEZADO
                    page.Header()
                        .Column(column =>
                        {
                            column.Item()
                                .Row(row =>
                                {
                                    row.RelativeItem()
                                        .Column(col =>
                                        {
                                            col.Item()
                                                .Text("Gestor de Cuentas Corrientes")
                                                .FontSize(16)
                                                .Bold()
                                                .FontColor(colorPrincipal);

                                            col.Item()
                                                .PaddingTop(2)
                                                .Text("Administración de cuentas corrientes y gestión comercial")
                                                .FontSize(9)
                                                .FontColor(colorTextoSuave);
                                        });

                                    row.ConstantItem(190)
                                        .Background(colorPrincipal)
                                        .Padding(10)
                                        .Column(col =>
                                        {
                                            col.Item()
                                                .AlignRight()
                                                .Text("PRESUPUESTO")
                                                .FontSize(10)
                                                .Bold()
                                                .FontColor("#BFD4F2");

                                            col.Item()
                                                .AlignRight()
                                                .Text($"N.º {presupuesto.Id}")
                                                .FontSize(20)
                                                .Bold()
                                                .FontColor(Colors.White);
                                        });
                                });

                            column.Item()
                                .PaddingTop(12)
                                .LineHorizontal(2)
                                .LineColor(colorAcento);
                        });

                    // CONTENIDO
                    page.Content()
                        .PaddingTop(20)
                        .Column(column =>
                        {
                            column.Spacing(12);

                            // Cliente y fecha
                            column.Item()
                                .Row(row =>
                                {
                                    row.Spacing(12);

                                    row.RelativeItem()
                                        .Background(colorFondoSuave)
                                        .Border(1)
                                        .BorderColor(colorBorde)
                                        .Padding(10)
                                        .Column(col =>
                                        {
                                            col.Item()
                                                .Text("CLIENTE")
                                                .FontSize(8)
                                                .Bold()
                                                .FontColor(colorTextoSuave);

                                            col.Item()
                                                .PaddingTop(3)
                                                .Text(
                                                    presupuesto.Cliente?.RazonSocial
                                                    ?? "Sin cliente")
                                                .FontSize(12)
                                                .Bold();
                                        });

                                    row.RelativeItem()
                                        .Background(colorFondoSuave)
                                        .Border(1)
                                        .BorderColor(colorBorde)
                                        .Padding(10)
                                        .Column(col =>
                                        {
                                            col.Item()
                                                .Text("FECHA")
                                                .FontSize(8)
                                                .Bold()
                                                .FontColor(colorTextoSuave);

                                            col.Item()
                                                .PaddingTop(3)
                                                .Text(
                                                    presupuesto.Fecha
                                                        .ToString("dd/MM/yyyy"))
                                                .FontSize(12)
                                                .Bold();
                                        });
                                });

                            // Observaciones
                            if (!string.IsNullOrWhiteSpace(
                                presupuesto.Observaciones))
                            {
                                column.Item()
                                    .BorderLeft(3)
                                    .BorderColor(colorAcento)
                                    .Background(colorFondoSuave)
                                    .Padding(10)
                                    .Column(col =>
                                    {
                                        col.Item()
                                            .Text("OBSERVACIONES")
                                            .FontSize(8)
                                            .Bold()
                                            .FontColor(colorTextoSuave);

                                        col.Item()
                                            .PaddingTop(3)
                                            .Text(
                                                presupuesto.Observaciones);
                                    });
                            }

                            // Tabla de conceptos
                            column.Item()
                                .PaddingTop(8)
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(4);
                                        columns.RelativeColumn(1.2f);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell()
                                            .Background(colorPrincipal)
                                            .Padding(7)
                                            .Text("Producto")
                                            .Bold()
                                            .FontColor(Colors.White);

                                        header.Cell()
                                            .Background(colorPrincipal)
                                            .Padding(7)
                                            .AlignRight()
                                            .Text("Cantidad")
                                            .Bold()
                                            .FontColor(Colors.White);

                                        header.Cell()
                                            .Background(colorPrincipal)
                                            .Padding(7)
                                            .AlignRight()
                                            .Text("Precio Unitario")
                                            .Bold()
                                            .FontColor(Colors.White);

                                        header.Cell()
                                            .Background(colorPrincipal)
                                            .Padding(7)
                                            .AlignRight()
                                            .Text("Subtotal")
                                            .Bold()
                                            .FontColor(Colors.White);
                                    });

                                    int indiceFila = 0;

                                    foreach (var detalle in presupuesto.Detalles)
                                    {
                                        var subtotal =
                                            detalle.Cantidad *
                                            detalle.PrecioUnitario;

                                        // Solo para alternar el color de fondo
                                        var fondoFila =
                                            indiceFila % 2 == 0
                                                ? "#FFFFFF"
                                                : colorFondoSuave;

                                        indiceFila++;

                                        table.Cell()
                                            .Background(fondoFila)
                                            .BorderBottom(1)
                                            .BorderColor(colorBorde)
                                            .Padding(7)
                                            .Text(detalle.Descripcion);

                                        table.Cell()
                                            .Background(fondoFila)
                                            .BorderBottom(1)
                                            .BorderColor(colorBorde)
                                            .Padding(7)
                                            .AlignRight()
                                            .Text(
                                                detalle.Cantidad
                                                    .ToString("N2"));

                                        table.Cell()
                                            .Background(fondoFila)
                                            .BorderBottom(1)
                                            .BorderColor(colorBorde)
                                            .Padding(7)
                                            .AlignRight()
                                            .Text(
                                                detalle.PrecioUnitario
                                                    .ToString("C2"));

                                        table.Cell()
                                            .Background(fondoFila)
                                            .BorderBottom(1)
                                            .BorderColor(colorBorde)
                                            .Padding(7)
                                            .AlignRight()
                                            .Text(
                                                subtotal
                                                    .ToString("C2"))
                                            .Bold();
                                    }

                                    // Total
                                    table.Cell()
                                        .ColumnSpan(3)
                                        .Background(colorFondoSuave)
                                        .BorderTop(2)
                                        .BorderColor(colorPrincipal)
                                        .Padding(9)
                                        .AlignRight()
                                        .Text("TOTAL")
                                        .Bold()
                                        .FontSize(11)
                                        .FontColor(colorPrincipal);

                                    table.Cell()
                                        .Background(colorFondoSuave)
                                        .BorderTop(2)
                                        .BorderColor(colorPrincipal)
                                        .Padding(9)
                                        .AlignRight()
                                        .Text(total.ToString("C2"))
                                        .Bold()
                                        .FontSize(13)
                                        .FontColor(colorAcento);
                                });
                        });

                    // PIE DE PÁGINA
                    page.Footer()
                        .Column(column =>
                        {
                            column.Item()
                                .LineHorizontal(1)
                                .LineColor(colorBorde);

                            column.Item()
                                .PaddingTop(6)
                                .Row(row =>
                                {
                                    row.RelativeItem()
                                        .Text(text =>
                                        {
                                            text.DefaultTextStyle(x =>
                                                x.FontSize(8)
                                                 .FontColor(colorTextoSuave));

                                            text.Span("Presupuesto generado el ");
                                            text.Span(
                                                DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                                        });

                                    row.ConstantItem(100)
                                        .AlignRight()
                                        .Text(text =>
                                        {
                                            text.DefaultTextStyle(x =>
                                                x.FontSize(8)
                                                 .FontColor(colorTextoSuave));

                                            text.Span("Página ");
                                            text.CurrentPageNumber();
                                            text.Span(" de ");
                                            text.TotalPages();
                                        });
                                });
                        });
                });
            });

            byte[] pdf = documento.GeneratePdf();

            return File(
                pdf,
                "application/pdf",
                $"Presupuesto-{presupuesto.Id}.pdf");
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

        //Gestionar presupuestos, Aprobado, rechazado.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Aceptar(int id)
        {
            var presupuesto = await _context.Presupuestos
                .FirstOrDefaultAsync(p => p.Id == id);

            if (presupuesto == null)
                return NotFound();

            if (presupuesto.Estado != "Pendiente")
            {
                return RedirectToAction(
                    nameof(Details),
                    new { id = presupuesto.Id });
            }

            presupuesto.Estado = "Aprobado";

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(Details),
                new { id = presupuesto.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rechazar(int id)
        {
            var presupuesto = await _context.Presupuestos
                .FirstOrDefaultAsync(p => p.Id == id);

            if (presupuesto == null)
                return NotFound();

            if (presupuesto.Estado != "Pendiente")
            {
                return RedirectToAction(
                    nameof(Details),
                    new { id = presupuesto.Id });
            }

            presupuesto.Estado = "Rechazado";

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(Details),
                new { id = presupuesto.Id });
        }
    }
}