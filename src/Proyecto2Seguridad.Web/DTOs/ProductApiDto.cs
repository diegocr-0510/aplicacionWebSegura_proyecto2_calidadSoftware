using System.ComponentModel.DataAnnotations;

namespace Proyecto2Seguridad.Web.DTOs
{
    public class ProductApiDto
    {
        // Id del producto
        public int Id { get; set; }

        // Código alfanumérico del producto
        [Required(ErrorMessage = "El código es obligatorio.")]
        [RegularExpression("^[a-zA-Z0-9]+$", ErrorMessage = "El código debe ser alfanumérico.")]
        [StringLength(50, ErrorMessage = "El código no puede superar los 50 caracteres.")]
        public string Code { get; set; } = string.Empty;

        // Nombre del producto
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
        public string Name { get; set; } = string.Empty;

        // Descripción del producto
        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres.")]
        public string Description { get; set; } = string.Empty;

        // Cantidad disponible
        [Range(0, int.MaxValue, ErrorMessage = "La cantidad no puede ser negativa.")]
        public int Quantity { get; set; }

        // Precio del producto
        [Range(typeof(decimal), "0.01", "9999999999", ErrorMessage = "El precio debe ser mayor a 0.")]
        public decimal Price { get; set; }
    }
}