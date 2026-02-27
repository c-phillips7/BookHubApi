using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;


namespace BookHub.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Book> Books { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<BookGenre> BookGenres { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ReadingList> ReadingLists { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<ReadingListItem> ReadingListItems { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Many-to-Many relations
                
            // Link Table between Book and Genre
            builder.Entity<BookGenre>().HasKey(bg => new { bg.BookId, bg.GenreId });
                // Composite primary key

            // BookGenre --- Book
            builder.Entity<BookGenre>()
                .HasOne(bg => bg.Book)
                .WithMany(b => b.BookGenres)
                .HasForeignKey(bg => bg.BookId);

            // BookGenre --- Genre
            builder.Entity<BookGenre>()
                .HasOne(bg => bg.Genre)
                .WithMany(g => g.BookGenres)
                .HasForeignKey(bg => bg.GenreId);

            // Many-to-One relations
            // ReadingListItem --- ReadingList
            builder.Entity<ReadingListItem>()
                .HasOne(rli => rli.ReadingList)
                .WithMany(rl => rl.Items)
                .HasForeignKey(rli => rli.ReadingListId);

            // ReadingListItem --- Book
            builder.Entity<ReadingListItem>()
                .HasOne(rli => rli.Book)
                .WithMany() // Don't think back reference is needed?
                .HasForeignKey(rli => rli.BookId);

            // Book --- Author

            // Review --- Book
            
            // Review --- User

            
        }
    }
}