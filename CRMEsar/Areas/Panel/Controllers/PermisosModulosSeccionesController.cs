using System.Threading.Tasks;
using CRMEsar.AccesoDatos.Data.Repository.IRepository;
using CRMEsar.Models;
using CRMEsar.Models.ViewModels.CrudEntidades.PermisosModulosSecciones;
using CRMEsar.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CRMEsar.Areas.Panel.Controllers
{
    [Area("Panel")]
    [Authorize(Roles = "Admin,Prestador")]
    public class PermisosModulosSeccionesController : Controller
    {
        private readonly IContenedorTrabajo _contenedorTrabajo;
        private readonly ProtectorUtils _protectorUtils;
        private const string NombreEntidadNormalizado = "PERMISOSMODULOSSECCIONES";
        private readonly RoleManager<ApplicationRole> _roleManager;

        public PermisosModulosSeccionesController(IContenedorTrabajo contenedorTrabajo,
            ProtectorUtils protectorUtils,
            RoleManager<ApplicationRole> roleManager)
        {
            _contenedorTrabajo = contenedorTrabajo;
            _protectorUtils = protectorUtils;
            _roleManager = roleManager;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var permisoModuloSecciones = _contenedorTrabajo.PermisosModulosSecciones.GetAll(
                includeProperties: "Modulo,Seccion,applicationRole").Select(e => new PermisosModulosSeccionesTablaVM
                {
                     Modulo = e.Modulo.nombre,
                     Seccion = e.Seccion.nombre,
                     Temporal = e.Temporal,
                     Rol = e.applicationRole.Name,
                     PermisomoduloSeccionIdEncriptado = _protectorUtils.EncriptarGuid(e.PermisoId,"Permiso")
                });
            
            return View(permisoModuloSecciones);
        }

        [HttpGet]
        public async Task<IActionResult> Create() 
        {
            var listaModulos = _contenedorTrabajo.Modulo.GetListaModulos().ToList();
            var listaSecciones = _contenedorTrabajo.ModulosSecciones.GetListaSecciones().ToList();
            var roles = await _roleManager.Roles.ToListAsync();
            var listaRoles = roles.Select(r => new SelectListItem 
            {
                Text = r.Name,
                Value = r.Id.ToString()
            }).ToList();
            var viewModelPermisos = new PermisosModulosSeccionesCrearEditarVM
            {
                ListaModulos = listaModulos,
                ListaSecciones = listaSecciones,
                ListaRoles = listaRoles
            };
            return View(viewModelPermisos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(PermisosModulosSeccionesCrearEditarVM permisosVM) 
        {
            if (ModelState.IsValid) 
            {
                var permiso = new PermisosModulosSecciones
                {
                    PermisoId = Guid.NewGuid(),
                    Temporal = (bool)permisosVM.Temporal,
                    FechaInicio = permisosVM.FechaInicio,
                    FechaFin = permisosVM.FechaFin,
                    RolId = permisosVM.RolId,
                    ModuloId = permisosVM.ModuloId,
                    SeccionId = permisosVM.SeccionId
                };
                _contenedorTrabajo.PermisosModulosSecciones.Add(permiso);
                _contenedorTrabajo.Save();
                return RedirectToAction(nameof(Index));
            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var idReal = _protectorUtils.DesencriptarGuid(id, "Permiso");
            var permiso = _contenedorTrabajo.PermisosModulosSecciones.GetFirstOrDefault(
                e => e.PermisoId == idReal,
                includeproperties: "Modulo,Seccion,applicationRole"
                );
            if (permiso == null)
            {
                return NotFound();
            }
            var listaModulos = _contenedorTrabajo.Modulo.GetListaModulos().ToList();
            var listaSecciones = _contenedorTrabajo.ModulosSecciones.GetListaSecciones().ToList();
            var roles = await _roleManager.Roles.ToListAsync();
            var listaRoles = roles.Select(r => new SelectListItem
            {
                Text = r.Name,
                Value = r.Id.ToString()
            }).ToList();
            var viewModel = new PermisosModulosSeccionesCrearEditarVM
            {
                permisoId = _protectorUtils.EncriptarGuid(idReal, "Permiso"),
                Temporal = permiso.Temporal,
                FechaInicio = permiso.FechaInicio,
                FechaFin = permiso.FechaFin,
                ListaRoles = listaRoles,
                RolId = permiso.RolId,
                ListaModulos = listaModulos,
                ModuloId = permiso.ModuloId,
                ListaSecciones = listaSecciones,
                SeccionId = permiso.SeccionId,
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAsync(PermisosModulosSeccionesCrearEditarVM permisosVM)
        {
            if (ModelState.IsValid)
            {
                Guid idReal = _protectorUtils.DesencriptarGuid(permisosVM.permisoId, "Permiso");
                var permiso = new PermisosModulosSecciones
                {
                    PermisoId = idReal,
                    RolId = permisosVM.RolId,
                    ModuloId = permisosVM.ModuloId,
                    SeccionId = permisosVM.SeccionId,
                    Temporal = (bool)permisosVM.Temporal,
                    FechaFin = permisosVM.FechaFin,
                    FechaInicio = permisosVM.FechaInicio
                };
                _contenedorTrabajo.PermisosModulosSecciones.update(permiso);
                _contenedorTrabajo.Save();
                TempData["RespuestaOperacion"] = "Permiso actualizado correctamente";
                return RedirectToAction(nameof(Index));
            }

            var roles = await _roleManager.Roles.ToListAsync();
            permisosVM.ListaSecciones = _contenedorTrabajo.ModulosSecciones.GetListaSecciones().ToList();
            permisosVM.ListaModulos = _contenedorTrabajo.Modulo.GetListaModulos().ToList();
            permisosVM.ListaRoles = roles.Select(r => new SelectListItem
            {
                Text = r.Name,
                Value = r.Id.ToString()
            }).ToList();
            return View(permisosVM);
        }
    }
}
