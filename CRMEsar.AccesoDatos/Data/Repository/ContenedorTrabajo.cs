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
            ModulosSecciones = new ModulosSeccionesRepository(_db);
            PermisosModulosSecciones = new PermisosModulosSeccionesRepository(_db);
            Log = new LogRepository(_db);
            TipoNotificaciones = new TipoNotificacionesRepository(_db);
            Notificaciones = new NotificacionesRepository(_db);
            Integraciones = new IntegracionesRepository(_db);
        }

        public IEstadoRepository Estado { get; private set; }
        public IEntidadRepository Entidad { get; private set; }
        public IModuloRepository Modulo { get; private set; }
        public IModulosSeccionesRepository ModulosSecciones { get; private set; }
        public IPermisosModulosSeccionesRepository PermisosModulosSecciones { get; private set; }
        public ILogRepository Log { get; private set; }
        public ITipoNotificacionesRepository TipoNotificaciones { get; private set; }
        public INotificacionesRepository Notificaciones { get; private set; }
        public IIntegracionesRepository Integraciones { get; private set; }

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
