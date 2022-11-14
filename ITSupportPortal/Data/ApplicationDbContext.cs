using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ITSupportPortal.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ITSupportPortal.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }
     
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ChatHistory>()
                .HasKey(ch => new {ch.CaseID,ch.Username,ch.CreatedAt });


            modelBuilder.Entity<BlogComment>()
                .HasKey(blc => new { blc.Id, blc.CommentTime, blc.Username });

            base.OnModelCreating(modelBuilder);
            
            SeedAdminUser(modelBuilder);
        }
        public  DbSet<Case> Case { get; set; }
        public  DbSet<ChatHistory> ChatHistory { get; set; }
        public  DbSet<BlogDetail> BlogDetail { get; set; }
        public  DbSet<BlogComment> BlogComment { get; set; }
        public  DbSet<CaseMetric> CaseMetric { get; set; }

        private void SeedAdminUser(ModelBuilder modelBuilder)
        {

            PasswordHasher<IdentityUser> hasher = new PasswordHasher<IdentityUser>();
            
            IdentityRole administrator = new IdentityRole("Admin");
            administrator.NormalizedName = "ADMIN";
            modelBuilder.Entity<IdentityRole>().HasData(administrator);

            var adminUser = new IdentityUser
            {
                UserName = "admin",
                NormalizedUserName = "ADMIN",
                Email = "admin@ITSupport.com",
                NormalizedEmail = "ADMIN@ITSUPPORT.COM",
                EmailConfirmed = false,
                LockoutEnabled = true,
                SecurityStamp = Guid.NewGuid().ToString(),
            };
            adminUser.PasswordHash = hasher.HashPassword(adminUser, "Securepassword@123");
            modelBuilder.Entity<IdentityUser>().HasData(adminUser);

            IdentityUserRole<string> administratorRole = new IdentityUserRole<string>();
            administratorRole.UserId = adminUser.Id;
            administratorRole.RoleId = administrator.Id;
            modelBuilder.Entity<IdentityUserRole<string>>().HasData(administratorRole);
        }
    }

}