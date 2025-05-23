using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using RentaloYa.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentaloYa.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Gender> Genders { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRol> UsersRoles { get; set; }
        public DbSet<RolePermission> RolesPermissions { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<ItemStatus> ItemStatuses { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<RentalType> RentalTypes { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Tabla intermedia UserRoles
            modelBuilder.Entity<UserRol>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<UserRol>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRol>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UsersRoles)
                .HasForeignKey(ur => ur.RoleId);

            // Tabla intermedia RolePermissions
            modelBuilder.Entity<RolePermission>()
                .HasKey(rp => new { rp.RoleId, rp.PermissionId });

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolesPermissions)
                .HasForeignKey(rp => rp.RoleId);

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolesPermissions)
                .HasForeignKey(rp => rp.PermissionId);

            // Role
            modelBuilder.Entity<Role>()
                .Property(r => r.Rol)
                .IsRequired();

            // Permission
            modelBuilder.Entity<Permission>()
                .Property(p => p.Permissionn)
                .IsRequired();

            //Status for items
            modelBuilder.Entity<Item>()
                .HasOne(i => i.ItemStatus)
                .WithMany(s => s.Items)
                .HasForeignKey(i => i.ItemStatusId);

            //category for items
            modelBuilder.Entity<Item>()
                .HasOne(i => i.Category)
                .WithMany(c => c.Items)
                .HasForeignKey(i => i.CategoryId);

            // RentalType for items
            modelBuilder.Entity<Item>()
                .HasOne(i => i.RentalType)
                .WithMany(rt => rt.Items)
                .HasForeignKey(i => i.RentalTypeId);


            // ---------- Seed Data ----------
            modelBuilder.Entity<Gender>().HasData(
                new Gender { IdGender = 1, GenderName = "Masculino" },
                new Gender { IdGender = 2, GenderName = "Femenino" },
                new Gender { IdGender = 3, GenderName = "Otro" }
                );

            // Seed Roles
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, Rol = "Administrador"},
                new Role { RoleId = 2, Rol = "Invitado"},
                new Role { RoleId = 3, Rol = "Cliente"}
            );

            // Seed Permissions
            modelBuilder.Entity<Permission>().HasData(
                new Permission { PermissionId = 1, Permissionn = "Crear Usuario" },
                new Permission { PermissionId = 2, Permissionn = "Eliminar Farmacia"},
                new Permission { PermissionId = 3, Permissionn = "Ver Reportes"}
            );

            // Seed RolePermissions
            modelBuilder.Entity<RolePermission>().HasData(
                new RolePermission { RoleId = 1, PermissionId = 1 }, // Admin - Crear Usuario
                new RolePermission { RoleId = 1, PermissionId = 2 }, // Admin - Eliminar Farmacia
                new RolePermission { RoleId = 1, PermissionId = 3 }, // Admin - Ver Reportes

                new RolePermission { RoleId = 2, PermissionId = 3 }  // Invitado - Ver Reportes
            );

            // ---------- Seed Categories ----------
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Herramientas" },
                new Category { Id = 2, Name = "Electrónicos" },
                new Category { Id = 3, Name = "Deportes" },
                new Category { Id = 4, Name = "Hogar" },
                new Category { Id = 5, Name = "Jardín" },
                new Category { Id = 6, Name = "Eventos" }
            );

            // ---------- Seed RentalTypes ----------
            modelBuilder.Entity<RentalType>().HasData(
                new RentalType { Id = 1, TypeName = "Por hora" },
                new RentalType { Id = 2, TypeName = "Por día" },
                new RentalType { Id = 3, TypeName = "Por semana" },
                new RentalType { Id = 4, TypeName = "Por mes" }
            );

            // ---------- Seed ItemStatuses ----------
            modelBuilder.Entity<ItemStatus>().HasData(
                new ItemStatus { Id = 1, StatusName = "Disponible", Description = "El artículo está visible y se puede alquilar" },
                new ItemStatus { Id = 2, StatusName = "Rentado", Description = "Todos los ejemplares están ocupados en este momento" },
                new ItemStatus { Id = 3, StatusName = "Pausado", Description = "El dueño pausó la publicación (invisible)" },
                new ItemStatus { Id = 4, StatusName = "Eliminado", Description = "Eliminado lógicamente del sistema" }
            );
            modelBuilder.Entity<User>().HasData(
              new User
              {
                  Id = 1,
                  IdSupa = new Guid("d290f1ee-6c54-4b01-90e6-d701748f0851"), // <- estático
                  UsernameProsody = "testuser2@localhost",
                  Username = "FelipSeg",
                  FullName = "Felipe Segura",
                  Email = "felipe@gmail.com",
                  Birthdate = new DateOnly(2002, 7, 7),
                  Gender_Id = 1,
                  LastLogin = null,
                  CreatedAt = new DateTime(2025, 4, 5, 0, 0, 0, DateTimeKind.Utc),
                  IsActive = true
              }
            );

            modelBuilder.Entity<User>()
                .Property(u => u.CreatedAt)
                .HasDefaultValueSql("GETDATE()");
            
            modelBuilder.Entity<Item>()
                .Property(i => i.CreatedAt)
                .HasDefaultValueSql("GETDATE()");
        }
    }
}
