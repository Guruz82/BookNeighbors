
using BookNeighbors.Api.Models;
using BookNeighbors.Api.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BookNeighbors.Api.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    // Definizione delle tabelle del database
    public DbSet<Library> Libraries => Set<Library>();
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<BookCategory> BookCategories => Set<BookCategory>();
    public DbSet<BookView> BookViews => Set<BookView>();
    public DbSet<LoanRequest> LoanRequests => Set<LoanRequest>();
    public DbSet<LoanRequestStatus> LoanRequestStatuses => Set<LoanRequestStatus>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<LoanMessage> LoanMessages => Set<LoanMessage>();

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // Configurazione delle entità e delle relazioni
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Importante: configura le tabelle standard di Identity
        base.OnModelCreating(modelBuilder);

        // Personalizzazioni AspNetUsers
        modelBuilder.Entity<ApplicationUser>(entity =>
        {


            entity.Property(u => u.DisplayName)
                .HasMaxLength(100)
                .IsRequired();
        });

        // Configurazioni per le entità Library, Book, BookView, BookCategory, LoanRequest, LoanRequestStatus e Category
        modelBuilder.Entity<Library>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.ApplicationUser)
                 .WithMany()
                 .HasForeignKey(e => e.UserId);
            entity.HasIndex(e => e.UserId)
                 .IsUnique();
            entity.Property(e => e.UserId)
                 .IsRequired()
                 .HasColumnName("user_id");
            entity.Property(u => u.Name)
                 .HasMaxLength(150)
                 .IsRequired();
            entity.Property(u => u.City)
                 .HasMaxLength(100);
            entity.Property(u => u.AreaName)
                 .HasMaxLength(150);
            entity.Property(e => e.Location)
                 .HasColumnType("geography(point, 4326)")
                 .IsRequired();
            entity.HasIndex(e => e.Location)
                .HasMethod("gist");
            entity.Property(e => e.LocationConsent)
                 .HasDefaultValue(false);
            entity.Property(e => e.IsPublished)
                 .HasDefaultValue(false);
        });


        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Library)
               .WithMany()
               .HasForeignKey(e => e.LibraryId);
            entity.HasIndex(e => e.LibraryId);
            entity.Property(e => e.LibraryId)
            .HasColumnName("library_id");
            entity.Property(u => u.Title)
               .HasMaxLength(255)
               .IsRequired();
            entity.Property(e => e.ISBN)
           .HasMaxLength(17);
            entity.Property(u => u.Author)
           .HasMaxLength(200)
           .IsRequired();
            entity.Property(u => u.ImageUrl)
           .HasMaxLength(1024);
            entity.Property(u => u.ThumbnailUrl)
           .HasMaxLength(1024);
            entity.Property(e => e.IsPublished)
            .HasDefaultValue(false);
        });

        modelBuilder.Entity<BookView>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.Book)
                .WithMany()
                .HasForeignKey(e => e.BookId);

            entity.HasIndex(e => e.BookId);
            entity.Property(e => e.BookId)
           .HasColumnName("book_id");
        });

        modelBuilder.Entity<BookCategory>(entity =>
        {
            entity.HasKey(x => new { x.BookId, x.CategoryId });

            entity.HasOne(x => x.Book)
                .WithMany(b => b.BookCategories)
                .HasForeignKey(x => x.BookId);

            entity.HasOne(x => x.Category)
                .WithMany()
                .HasForeignKey(x => x.CategoryId);
        });

        modelBuilder.Entity<LoanRequest>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.Book)
                .WithMany()
                .HasForeignKey(e => e.BookId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Requester)
                .WithMany()
                .HasForeignKey(e => e.RequesterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Status)
                .WithMany()
                .HasForeignKey(e => e.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.BookId)
             .HasColumnName("book_id");
            entity.Property(e => e.RequesterId)
            .HasColumnName("requester_id");
            entity.Property(e => e.StatusId)
            .HasColumnName("status_id");
        });
        modelBuilder.Entity<LoanRequestStatus>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.HasIndex(e => e.Name)
                    .IsUnique();
            });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.HasIndex(e => e.Name)
                .IsUnique();
        });


        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.Title).HasMaxLength(150).IsRequired();
            entity.Property(e => e.Message).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.Url).HasMaxLength(500);
            entity.HasIndex(e => new { e.UserId, e.IsRead, e.CreatedAt });
        });


        modelBuilder.Entity<LoanMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.LoanRequest).WithMany().HasForeignKey(e => e.LoanRequestId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Sender).WithMany().HasForeignKey(e => e.SenderId).OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.Text).HasMaxLength(2000).IsRequired();
            entity.HasIndex(e => new { e.LoanRequestId, e.CreatedAt });
        });

    }
}
