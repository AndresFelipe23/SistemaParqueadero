using System.ComponentModel.DataAnnotations;

namespace SistemaParqueaderoWEB.Models
{
    public class CierreCajaViewModel
    {
        // Información del día actual
        public DateTime FechaActual { get; set; } = DateTime.Now;
        
        // Estado de la caja
        public bool CajaAbierta { get; set; }
        public CierreCaja? CierreActual { get; set; }
        
        // Resumen del día
        public int TotalRegistros { get; set; }
        public int TotalCarros { get; set; }
        public int TotalMotos { get; set; }
        public decimal MontoEsperado { get; set; }
        
        // Montos por método de pago
        public decimal MontoEfectivo { get; set; }
        public decimal MontoTarjeta { get; set; }
        public decimal MontoTransferencia { get; set; }
        public decimal MontoTotal { get; set; }
        
        // Para el formulario de cierre
        [Display(Name = "Monto en efectivo")]
        [Required(ErrorMessage = "El monto en efectivo es requerido")]
        [Range(0, double.MaxValue, ErrorMessage = "El monto debe ser mayor o igual a 0")]
        public decimal MontoEfectivoReal { get; set; }
        
        [Display(Name = "Monto en tarjeta")]
        [Required(ErrorMessage = "El monto en tarjeta es requerido")]
        [Range(0, double.MaxValue, ErrorMessage = "El monto debe ser mayor o igual a 0")]
        public decimal MontoTarjetaReal { get; set; }
        
        [Display(Name = "Monto en transferencia")]
        [Required(ErrorMessage = "El monto en transferencia es requerido")]
        [Range(0, double.MaxValue, ErrorMessage = "El monto debe ser mayor o igual a 0")]
        public decimal MontoTransferenciaReal { get; set; }
        
        [Display(Name = "Observaciones")]
        [StringLength(500)]
        public string? Observaciones { get; set; }
        
        // Diferencia calculada
        public decimal Diferencia { get; set; }
        
        // Lista de registros del día
        public List<RegistroParqueo> RegistrosDelDia { get; set; } = new();
        
        // Historial de cierres
        public List<CierreCaja> HistorialCierres { get; set; } = new();
    }
}
