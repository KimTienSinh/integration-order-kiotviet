namespace Kps.Integration.Api.Infra
{
    using Kps.Integration.Domain.Entities;
    using Microsoft.EntityFrameworkCore;

    public class KpsIntegrationContext : Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext
    {
        public virtual DbSet<CustomerMapping> CustomerMapping { get; set; }

        public virtual DbSet<Order> Order { get; set; }

        public virtual DbSet<OrderItem> OrderItem { get; set; }

        public virtual DbSet<ProductMapping> ProductMapping { get; set; }

        public virtual DbSet<ScheduleLogging> ScheduleLogging { get; set; }

        public virtual DbSet<WmsSyncLog> WmsSyncLog { get; set; }

        public KpsIntegrationContext(DbContextOptions<KpsIntegrationContext> options) : base(options)
        {
            
        }

    }
}
