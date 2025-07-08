using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;


namespace DataManager
{

    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Pet> Pets { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Vet> Vets { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Service> Services { get; set; }

        public DbSet<AppointmentService> AppointmentsService { get; set; }

        public DbSet<PetService> PetsService { get; set; }

        public DbSet<PostComment> PostComments { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Composite Key for PostComment
            modelBuilder.Entity<PostComment>()
                .HasKey(pc => new { pc.PostId, pc.CommentId });

            


            // Composite Keys for Other Junction Tables
            modelBuilder.Entity<Like>()
                .HasKey(pl => new { pl.PostId, pl.UserId });


            modelBuilder.Entity<PetService>()
                .HasKey(ps => new { ps.PetId, ps.ServiceId });

            modelBuilder.Entity<AppointmentService>()
                .HasKey(aps => new { aps.AppointmentId, aps.ServiceId });

            // User relationships
            modelBuilder.Entity<User>()
                .HasMany(u => u.Pets)
                .WithOne(p => p.User)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Appointments)
                .WithOne(a => a.User)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Orders)
                .WithOne(o => o.User)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Posts)
                .WithOne(p => p.User)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Comments)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Likes)
                .WithOne(pl => pl.User)
                .HasForeignKey(pl => pl.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            

            

           

            // Appointment relationships
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Pet)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PetId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Vet)
                .WithMany(v => v.Appointments)
                .HasForeignKey(a => a.VetId)
                .OnDelete(DeleteBehavior.NoAction);

            // OrderItem relationships
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.NoAction);

            // Post relationships
            modelBuilder.Entity<Post>()
                .HasMany(p => p.Comments)
                .WithOne(c => c.Post)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Post>()
                .HasMany(p => p.Likes)
                .WithOne(pl => pl.Post)
                .HasForeignKey(pl => pl.PostId)
                .OnDelete(DeleteBehavior.NoAction);

            // Comment relationships
            

            // PetService relationships
            modelBuilder.Entity<PetService>()
                .HasOne(ps => ps.Pet)
                .WithMany(p => p.PetServices)
                .HasForeignKey(ps => ps.PetId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PetService>()
                .HasOne(ps => ps.Service)
                .WithMany(s => s.PetServices)
                .HasForeignKey(ps => ps.ServiceId)
                .OnDelete(DeleteBehavior.NoAction);

            // AppointmentService relationships
            modelBuilder.Entity<AppointmentService>()
                .HasOne(aps => aps.Appointment)
                .WithMany(a => a.AppointmentServices)
                .HasForeignKey(aps => aps.AppointmentId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<AppointmentService>()
                .HasOne(aps => aps.Service)
                .WithMany(s => s.AppointmentServices)
                .HasForeignKey(aps => aps.ServiceId)
                .OnDelete(DeleteBehavior.NoAction);

            // Vet relationship
            modelBuilder.Entity<Vet>()
                .HasOne(v => v.User)
                .WithOne(u => u.Vet)
                .HasForeignKey<Vet>(v => v.VetId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
