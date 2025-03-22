using CRMEsar.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CRMEsar.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        //Aqui Se deben agregar los modelos que se vayan creando
        public DbSet<AspNetUser> AspNetUsers { get; set; }
    }
}
