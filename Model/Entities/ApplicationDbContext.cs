using SwiftDrop.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SwiftDrop.Models.Entities
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int,
        IdentityUserClaim<int>, ApplicationUserRole, IdentityUserLogin<int>,
        IdentityRoleClaim<int>, IdentityUserToken<int>>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // Users / Roles / UserRoles بتيجي جاهزة من IdentityDbContext
        // (Users, Roles, UserRoles, UserClaims, UserLogins, UserTokens, RoleClaims)

        // Profiles
        public DbSet<Admin> Admins => Set<Admin>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Delivery> Deliveries => Set<Delivery>();

        // Catalog
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
        public DbSet<SecondaryImage> SecondaryImages => Set<SecondaryImage>();
        public DbSet<ProductSecondaryImage> ProductSecondaryImages => Set<ProductSecondaryImage>();
        public DbSet<ProductReview> ProductReviews => Set<ProductReview>();

        // Cart
        public DbSet<Cart> Carts => Set<Cart>();
        public DbSet<CartItem> CartItems => Set<CartItem>();

        // Orders
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();

        // Delivery
        public DbSet<DeliveryAssignment> DeliveryAssignments => Set<DeliveryAssignment>();
        public DbSet<DeliveryReview> DeliveryReviews => Set<DeliveryReview>();

        // Chat
        public DbSet<Conversation> Conversations => Set<Conversation>();
        public DbSet<Message> Messages => Set<Message>();

        // System
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // بتحمّل كل ملفات Configurations تلقائي
            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}