using AutoMapper;
using ManejoPresupuesto.Models;

namespace ManejoPresupuesto.servicios
{
    public class AutoMapperProfile: Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Cuenta, CuentaCreacionViewModel>();
        }

    }
}
