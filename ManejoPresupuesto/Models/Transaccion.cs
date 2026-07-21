using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuesto.Models
{
    public class Transaccion
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        [Display(Name = "Fecha de la transacción")]
        [DataType(DataType.Date)]
        public DateTime FechaTransaccion { get; set; } = DateTime.Today;
        public decimal Monto { get; set; }
        [Range (0,maximum: int.MaxValue, ErrorMessage = "Debe seleccionar una categoría.")]
        [Display(Name = "Categoría")]
        public int CategoriaId { get; set; }
        [StringLength(100, ErrorMessage = "La nota no puede tener más de {1} caracteres.")]
        public string Nota { get; set; }
        [Display (Name = "Cuenta")]
        public int CuentaId { get; set; }
    }
}
