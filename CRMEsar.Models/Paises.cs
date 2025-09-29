using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMEsar.Models
{
    public class Paises
    {
        [Key]
        public Guid PaisId { get; set; } = Guid.NewGuid();
        public string Nombre{ get; set; }
        public string NormalizedName{ get; set; }
        public string  codigoInternacional { get; set; }
        public DateTime FechaCreacion {  get; set; }
    }
}
