using AutoMapper;
using ManejoPresupuesto.Models;
using ManejoPresupuesto.servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.IdentityModel.Abstractions;
using System.Reflection;

namespace ManejoPresupuesto.Controllers
{
    public class TransaccionesController : Controller
    {
        private readonly IServiciosUsuarios servicioUsuarios;
        private readonly IRepositorioCuentas repositorioCuentas;
        private readonly IRepositorioCategorias repositorioCategorias;
        private readonly IRepositorioTransacciones repositorioTransacciones;
        private readonly IMapper mapper;

        public TransaccionesController(IServiciosUsuarios servicioUsuarios, IRepositorioCuentas repositorioCuentas,
            IRepositorioCategorias repositorioCategorias,
            IRepositorioTransacciones repositorioTransacciones,
            IMapper mapper)
        {
            this.servicioUsuarios = servicioUsuarios;
            this.repositorioCuentas = repositorioCuentas;
            this.repositorioCategorias = repositorioCategorias;
            this.repositorioTransacciones = repositorioTransacciones;
            this.mapper = mapper;
        }
        public async Task<IActionResult> IndexAsync(int mes , int año)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            DateTime fechaInicio;
            DateTime fechaFin;
            if (mes <= 0 || mes > 12 || año <= 1900)
            {
                var hoy = DateTime.Today;
                fechaInicio = new DateTime(hoy.Year, hoy.Month, 1);
            }
            else
            {
                fechaInicio = new DateTime(año, mes, 1);
            }
            fechaFin = fechaInicio.AddMonths(1).AddDays(-1);
            var parametro = new ParametroObtenerTransaccionesPorUsuario()
            {
                UsuarioId = usuarioId,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            };
            var transacciones = await repositorioTransacciones.ObtenerPorUsuarioId(parametro);
            var modelo = new ReporteTransaccionesDetallada();
           
            var transaccionesPorFecha = transacciones.OrderByDescending(x => x.FechaTransaccion)
                .GroupBy(x => x.FechaTransaccion)
                .Select(grupo => new ReporteTransaccionesDetallada.TransaccionesPorFecha
                {
                    FechaTransaccion = grupo.Key,
                    Transacciones = grupo.AsEnumerable()
                });
            modelo.TransaccionesAgrupadas = transaccionesPorFecha;
            modelo.FechaInicio = fechaInicio;
            modelo.FechaFin = fechaFin;
            ViewBag.mesAnterior = fechaInicio.AddMonths(-1).Month;
            ViewBag.añoAnterior = fechaInicio.AddMonths(-1).Year;
            ViewBag.mesPosterior = fechaInicio.AddMonths(1).Month;
            ViewBag.añoPosterior = fechaInicio.AddMonths(1).Year;
            ViewBag.urlRetorno = HttpContext.Request.Path + HttpContext.Request.QueryString;
            return View(modelo);

        }
        public async Task<IActionResult> Crear()
        {
            var UsuarioId = servicioUsuarios.ObtenerUsuarioId();
            var modelo = new TransaccionCreacionViewModel();
            modelo.Cuentas = await ObtenerCuentas(UsuarioId);
            modelo.Categorias = await ObtenerCategorias(UsuarioId, modelo.TipoOperacionId);
            return View(modelo);
        }
        [HttpPost]
        public async Task<IActionResult> Crear(TransaccionCreacionViewModel modelo)
        {
            var UsuarioId = servicioUsuarios.ObtenerUsuarioId();
            if (!ModelState.IsValid)
            {
                modelo.Cuentas = await ObtenerCuentas(UsuarioId);
                modelo.Categorias = await ObtenerCategorias(UsuarioId, modelo.TipoOperacionId);
                return View(modelo);
            }
            var cuenta = await repositorioCuentas.ObtenerPorId(modelo.CuentaId, UsuarioId);
            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            var categoria = await repositorioCategorias.ObtenerPorId(modelo.CategoriaId, UsuarioId);
            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            modelo.UsuarioId = UsuarioId;
            if (modelo.TipoOperacionId == TipoOperacion.Gasto)
             {
                modelo.Monto *= -1;
            }
            await repositorioTransacciones.Crear(modelo);
            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> Editar(int id,string urlRetorno = null)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var transaccion = await repositorioTransacciones.ObtenerPorId(id, usuarioId);
            if (transaccion is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var modelo= mapper.Map<TransaccionActualizacionViewModel>(transaccion);
            modelo.MontoAnterior = modelo.Monto;
            if (modelo.TipoOperacionId == TipoOperacion.Gasto)
            {
                modelo.MontoAnterior = modelo.Monto * -1;
            }
            modelo.CuentaAnteriorId = modelo.CuentaId;
            modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
            modelo.Cuentas = await ObtenerCuentas(usuarioId);
            modelo.UrlRetorno = urlRetorno;
            return View(modelo);

            }
        [HttpPost]
        public async Task<IActionResult> Editar(TransaccionActualizacionViewModel modelo)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            if (!ModelState.IsValid)
            {
                modelo.Cuentas = await ObtenerCuentas(usuarioId);
                modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
                return View(modelo);
            }
            var cuenta = await repositorioCuentas.ObtenerPorId(modelo.CuentaId, usuarioId);
            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            var categoria = await repositorioCategorias.ObtenerPorId(modelo.CategoriaId, usuarioId);
            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            var transaccion = mapper.Map<Transaccion>(modelo);
            if (modelo.TipoOperacionId == TipoOperacion.Gasto)
            {
                transaccion.Monto *= -1;
            }
            await repositorioTransacciones.Actualizar(transaccion, modelo.MontoAnterior, modelo.CuentaAnteriorId);
            if (!string.IsNullOrEmpty(modelo.UrlRetorno) && Url.IsLocalUrl(modelo.UrlRetorno))
            {
                return LocalRedirect(modelo.UrlRetorno);
            }
            else
            {
                return RedirectToAction("Index");
            }


        }
        [HttpPost]
        public async Task <IActionResult> Borrar(int id, string urlRetorno = null)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var transaccion = await repositorioTransacciones.ObtenerPorId(id, usuarioId);
            if (transaccion is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            await repositorioTransacciones.Borrar(id);
            if (!string.IsNullOrEmpty(urlRetorno) && Url.IsLocalUrl(urlRetorno))
            {
                return LocalRedirect(urlRetorno);
            }
            else
            {
                return RedirectToAction("Index");
            }
           
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerCuentas(int usuarioId)
        {
            var cuentas = await repositorioCuentas.Buscar(usuarioId);
            return cuentas.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
        }
        private async Task<IEnumerable<SelectListItem>> ObtenerCategorias(int usuarioId,
           TipoOperacion tipoOperacion)
        {
            var categorias = await repositorioCategorias.Obtener(usuarioId, tipoOperacion);
            return categorias.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
            
        }

        [HttpPost]


        public async Task<IActionResult> ObtenerCategorias([FromBody] TipoOperacion tipoOperacion)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var categorias = await ObtenerCategorias(usuarioId, tipoOperacion);
            return Ok(categorias);
        }

    }

}
