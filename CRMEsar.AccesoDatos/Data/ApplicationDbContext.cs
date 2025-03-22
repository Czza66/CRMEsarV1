using CRMEsar.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CRMEsar.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser,IdentityRole<Guid>, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        //Aqui Se deben agregar los modelos que se vayan creando
        public DbSet<ApplicationUser> AspNetUsers { get; set; }
        public DbSet<Estado> Estado { get; set; }
    }
}
