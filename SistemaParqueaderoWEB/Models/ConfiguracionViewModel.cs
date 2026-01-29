using System.ComponentModel.DataAnnotations;

namespace SistemaParqueaderoWEB.Models
{
    public class ConfiguracionViewModel
    {
        public int? Id { get; set; }

        [Display(Name = "Clave")]
        [Required(ErrorMessage = "La clave es requerida")]
        [StringLength(100, ErrorMessage = "La clave no puede exceder 100 caracteres")]
        public string Clave { get; set; } = string.Empty;

        [Display(Name = "Valor")]
        [Required(ErrorMessage = "El valor es requerido")]
        [StringLength(500, ErrorMessage = "El valor no puede exceder 500 caracteres")]
        public string Valor { get; set; } = string.Empty;

        [Display(Name = "Tipo")]
        [StringLength(50)]
        public string? Tipo { get; set; }

        [Display(Name = "Descripción")]
        [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        public string? Descripcion { get; set; }

        public List<string> TiposDisponibles { get; set; } = new()
        {
            "String",
            "Number",
            "Boolean",
            "Date",
            "Decimal"
        };
    }

    public class TarifasViewModel
    {
        [Display(Name = "Carro por hora")]
        [Required(ErrorMessage = "La tarifa es requerida")]
        [Range(0, double.MaxValue, ErrorMessage = "La tarifa debe ser mayor o igual a 0")]
        public decimal CarroPorHora { get; set; }

        [Display(Name = "Moto por hora")]
        [Required(ErrorMessage = "La tarifa es requerida")]
        [Range(0, double.MaxValue, ErrorMessage = "La tarifa debe ser mayor o igual a 0")]
        public decimal MotoPorHora { get; set; }

        [Display(Name = "Carro por minuto")]
        [Required(ErrorMessage = "La tarifa es requerida")]
        [Range(0, double.MaxValue, ErrorMessage = "La tarifa debe ser mayor o igual a 0")]
        public decimal CarroPorMinuto { get; set; }

        [Display(Name = "Moto por minuto")]
        [Required(ErrorMessage = "La tarifa es requerida")]
        [Range(0, double.MaxValue, ErrorMessage = "La tarifa debe ser mayor o igual a 0")]
        public decimal MotoPorMinuto { get; set; }
    }
}
