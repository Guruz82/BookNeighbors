
using BookNeighbors.Api.Models;
using BookNeighbors.Api.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BookNeighbors.Api.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Importante: configura le tabelle standard di Identity
        base.OnModelCreating(modelBuilder);

        // Personalizzazioni AspNetUsers
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(u => u.FirstName)
                .HasMaxLength(100);

            entity.Property(u => u.LastName)
                .HasMaxLength(100);

            entity.Property(u => u.DisplayName)
                .HasMaxLength(100)
                .IsRequired();
        });


         modelBuilder.Entity<Library>(entity =>
         {
             entity.HasKey(e => e.Id);
                entity.HasOne(e => e.ApplicationUser)
                    .WithMany()
                    .HasForeignKey(e => e.UserId);
                entity.HasIndex(e => e.UserId);
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
                .HasMaxLength(255);
                 entity.Property(e => e.ISBN)
                .HasMaxLength(17);
                 entity.Property(u => u.Author)
                .HasMaxLength(200);
                 entity.Property(u => u.ImageUrl    )
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
                .HasForeignKey(e => e.BookId);

            entity.HasOne(e => e.Requester)
                .WithMany()
                .HasForeignKey(e => e.RequesterId);

            entity.HasOne(e => e.Status)
                .WithMany()
                .HasForeignKey(e => e.StatusId);
        });
    }
}
