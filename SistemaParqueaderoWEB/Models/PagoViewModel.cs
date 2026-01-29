using System.ComponentModel.DataAnnotations;

namespace SistemaParqueaderoWEB.Models
{
    public class PagoViewModel
    {
        public int RegistroParqueoId { get; set; }
        
        public RegistroParqueo? RegistroParqueo { get; set; }
        
        [Display(Name = "Monto a pagar")]
        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Monto { get; set; }
        
        [Display(Name = "Método de pago")]
        [Required(ErrorMessage = "El método de pago es requerido")]
        public string MetodoPago { get; set; } = string.Empty;
        
        [Display(Name = "Referencia de pago")]
        [StringLength(100)]
        public string? ReferenciaPago { get; set; }
        
        [Display(Name = "Observaciones")]
        [StringLength(500)]
        public string? Observaciones { get; set; }
        
        // Opciones de métodos de pago
        public List<string> MetodosPagoDisponibles { get; set; } = new()
        {
            "Efectivo",
            "Tarjeta Débito",
            "Tarjeta Crédito",
            "Transferencia Bancaria",
            "Nequi",
            "Daviplata",
            "Otro"
        };
    }
    
    public class PagoHistorialViewModel
    {
        public List<Pago> Pagos { get; set; } = new();
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public string? MetodoPagoFiltro { get; set; }
        public string? EstadoPagoFiltro { get; set; }
        public decimal TotalRecaudado { get; set; }
        public int TotalPagos { get; set; }
        public List<string> MetodosPagoDisponibles { get; set; } = new()
        {
            "Efectivo",
            "Tarjeta Débito",
            "Tarjeta Crédito",
            "Transferencia Bancaria",
            "Nequi",
            "Daviplata",
            "Otro"
        };
    }
}
