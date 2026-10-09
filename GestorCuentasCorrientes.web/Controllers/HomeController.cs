using GestorCuentasCorrientes.web.Data;
using GestorCuentasCorrientes.web.Models;
using GestorCuentasCorrientes.web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace GestorCuentasCorrientes.web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var vm = new DashboardVm();

            // La página de inicio es pública: los datos del negocio
            // solo se cargan cuando hay una sesión iniciada
            if (User.Identity?.IsAuthenticated != true)
                return View(vm);

            var hoy = DateTime.Today;
            var limitePorVencer = hoy.AddDays(7);
            var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
            var inicioMesSiguiente = inicioMes.AddMonths(1);

            var presupuestos = _context.Presupuestos.AsNoTracking();

            // Pendientes cuya validez todavía no pasó (o que no tienen vencimiento)
            var pendientesVigentes = presupuestos.Where(p =>
                p.Estado == "Pendiente" &&
                (p.FechaVencimiento == null || p.FechaVencimiento >= hoy));

            // "Vencido" no es un estado guardado: es un Pendiente con la validez pasada
            var vencidos = presupuestos.Where(p =>
                p.Estado == "Pendiente" &&
                p.FechaVencimiento != null &&
                p.FechaVencimiento < hoy);

            var aceptadosMes = presupuestos.Where(p =>
                p.Estado == "Aprobado" &&
                p.Fecha >= inicioMes &&
                p.Fecha < inicioMesSiguiente);

            // Aceptados a los que todavía no se les generó el movimiento
            var sinMovimiento = presupuestos.Where(p =>
                p.Estado == "Aprobado" &&
                !_context.Movimientos.Any(m => m.PresupuestoId == p.Id));

            vm.PendientesCantidad = await pendientesVigentes.CountAsync();
            vm.PendientesMonto = await MontoAsync(pendientesVigentes);
            vm.PorVencerCantidad = await pendientesVigentes.CountAsync(p =>
                p.FechaVencimiento != null && p.FechaVencimiento <= limitePorVencer);

            vm.VencidosCantidad = await vencidos.CountAsync();
            vm.VencidosMonto = await MontoAsync(vencidos);

            vm.AceptadosMesCantidad = await aceptadosMes.CountAsync();
            vm.AceptadosMesMonto = await MontoAsync(aceptadosMes);

            vm.SinMovimientoCantidad = await sinMovimiento.CountAsync();
            vm.SinMovimientoMonto = await MontoAsync(sinMovimiento);

            vm.Ultimos = await presupuestos
                .OrderByDescending(p => p.Id)
                .Take(5)
                .Select(p => new PresupuestoListaItemVm
                {
                    Id = p.Id,
                    Fecha = p.Fecha,
                    FechaVencimiento = p.FechaVencimiento,
                    Cliente = p.Cliente != null ? p.Cliente.RazonSocial : "",
                    Estado = p.Estado,
                    Total = p.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario)
                })
                .ToListAsync();

            vm.MostrarResumen = true;

            return View(vm);
        }

        // Suma cantidad x precio de todas las líneas de los presupuestos indicados
        private Task<decimal> MontoAsync(IQueryable<Presupuesto> presupuestos) =>
            _context.PresupuestoDetalles
                .Where(d => presupuestos.Select(p => p.Id).Contains(d.PresupuestoId))
                .SumAsync(d => d.Cantidad * d.PrecioUnitario);

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}