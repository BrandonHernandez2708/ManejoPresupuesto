using ManejoPresupuesto.Models;
using ManejoPresupuesto.servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ManejoPresupuesto.Controllers
{
    public class TransaccionesController: Controller
    {
        private readonly IServiciosUsuarios servicioUsuarios;
        private readonly IRepositorioCuentas repositorioCuentas;


        public TransaccionesController(IServiciosUsuarios servicioUsuarios, IRepositorioCuentas repositorioCuentas)
        {
            this.servicioUsuarios = servicioUsuarios;
            this.repositorioCuentas = repositorioCuentas;
        }
        public async Task<IActionResult> Crear()
        {
            var UsuarioId = servicioUsuarios.ObtenerUsuarioId();
            var modelo = new TransaccionCreacionViewModel();
            modelo.Cuentas = await ObtenerCuentas(UsuarioId);
            return View(modelo);
        }

        
        private async Task<IEnumerable<SelectListItem>> ObtenerCuentas(int usuarioId)
        {
            var cuentas = await repositorioCuentas.Buscar(usuarioId);
            return cuentas.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
        }
    }
}
