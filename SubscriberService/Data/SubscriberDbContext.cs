using Microsoft.EntityFrameworkCore;
using SubscriberService.Domain;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace SubscriberService.Data
{
    public class SubscriberDbContext : DbContext
    {
        public SubscriberDbContext(DbContextOptions<SubscriberDbContext> options)
            : base(options)
        {
        }

        public DbSet<Subscriber> Subscribers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Subscriber>()
                .HasIndex(s => s.Username)
                .IsUnique();
        }
    }
}