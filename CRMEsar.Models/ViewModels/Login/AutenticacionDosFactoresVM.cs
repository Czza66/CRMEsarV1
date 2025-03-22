using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMEsar.Models.ViewModels.Login
{
    public class AutenticacionDosFactoresVM
    {
        //Acceso (Login)
        [Required]
        [Display(Name = "Codigo del Autenticador")]
        public string code { get; set; }

        //Para el registro

        public string Token { get; set; }

        //Para codigoQR
        public string? urlCodigoQR { get; set; }
    }
}
