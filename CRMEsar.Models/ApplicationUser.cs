using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace CRMEsar.Models
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string Nombre1 { get; set; }
        public string? Nombre2 { get; set; }
        public string Apellido1 { get; set; }
        public string? Apellido2 { get; set;}
        public string NombreCompleto { get; set; }
        public string NumeroDocumento { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime FechaModificacion { get; set; }
        public int CuentaID { get; set; }
        public string? fotoUser { get; set; }

        //Campos Foraneos
        [ForeignKey("EstadoId")]
        public Guid? EstadoId { get; set; }
        public Estados? Estado { get; set; }

        //Campos Foraneos
        public Guid? PaisId { get; set; }
        [ForeignKey("PaisId")]
        public Paises? Pais { get; set; }
    }
}
