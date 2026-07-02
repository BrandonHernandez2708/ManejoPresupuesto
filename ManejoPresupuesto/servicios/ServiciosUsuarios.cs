namespace ManejoPresupuesto.servicios
{
    public interface IServiciosUsuarios
    {
        int ObtenerUsuarioId();
    }
public class serviciosUsuarios : IServiciosUsuarios
    {
        public int ObtenerUsuarioId()
        {
            // Lógica para obtener el ID del usuario actual
            return 1; // Ejemplo: devolver un ID fijo para pruebas
        }
    } 
}
