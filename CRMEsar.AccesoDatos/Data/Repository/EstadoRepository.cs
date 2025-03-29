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
    public class EstadoRepository : Repository<Estados>, IEstadoRepository
    {
        private readonly ApplicationDbContext _db;
        public EstadoRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Estados estado)
        {
            var objDesdeDB = _db.Estado.FirstOrDefault(e => e.EstadoId == estado.EstadoId);
            if (objDesdeDB != null) 
            {
                objDesdeDB.Nombre = estado.Nombre;
                objDesdeDB.Descripcion = estado.Descripcion;
                objDesdeDB.NormalizedName = estado.NormalizedName;
                objDesdeDB.Activo = estado.Activo;
            }
        }
    }
}
