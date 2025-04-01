using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CRMEsar.AccesoDatos.Data.Repository.IRepository;
using CRMEsar.Data;
using CRMEsar.Models;

namespace CRMEsar.AccesoDatos.Data.Repository
{
    public class ContenedorTrabajo :IContenedorTrabajo
    {
        private readonly ApplicationDbContext _db;

        public ContenedorTrabajo(ApplicationDbContext db)
        {
            _db = db;
            //Aca debemos pasar los modelos puestos en el IContenedorTrabajo
            Estado = new EstadoRepository(_db);
            Entidad = new EntidadRepository(_db);
            Modulo = new ModuloRepository(_db);
        }

        public IEstadoRepository Estado { get; private set; }
        public IEntidadRepository Entidad { get; private set; }
        public IModuloRepository Modulo { get; private set; }
        
        public void Dispose()
        {
            _db.Dispose();
        }

        public void Save()
        {
            _db.SaveChanges();
        }

    }
}
