using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMEsar.AccesoDatos.Data.Repository.IRepository
{
    public interface IContenedorTrabajo : IDisposable
    {
        //Aca se deben ir agregando los diferentes repositorios
        IEstadoRepository Estado { get; }

        void Save();
    }
}
