using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMEsar.Models.ViewModels
{
    public class VerificarAutenticadorVM
    {
        [Required]
        [Display(Name = "Codigo numerico")]
        public string code { get; set; }

        public string? returnURL{ get; set; }

        [Required]
        [Display(Name = "Recordar datos")]
        public bool RecordarDatos { get; set; }


    }
}
