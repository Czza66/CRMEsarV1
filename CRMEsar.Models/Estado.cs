using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMEsar.Models
{
    public class Estado
    {
        [Key]
        public Guid EstadoId { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage ="El Nombre del estado es requerido")]
        [MaxLength(50)]
        public string Nombre { get; set; }

        [Required(ErrorMessage ="La Normalizacion del nombre es requerida")]
        [MaxLength(50)]
        public string NormalizedName { get; set; }

        [MaxLength(150)]
        public string Descripcion { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public bool Activo { get; set; } = true;

        // Relación con AspNetUser (uno a muchos)
        public ICollection<ApplicationUser> ApplicationUser { get; set; }

    }
}
