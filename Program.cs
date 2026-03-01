using BookHub.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using BookHub.Services;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<EmailService>();

var app = builder.Build();

// Seed roles
using (var scope = app.Services.CreateScope())
{

    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // Automatically apply migrations
    db.Database.Migrate();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    // Wrapped in async to make sure seeding completes before app starts
    Task.Run(async () =>
    {
        var roles = new[] { "Admin", "User" };
        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // Default admin user for testing, will not be in final product.
        // In production, admin user should be created through a secure process and not hardcoded.
        string adminEmail = "admin@bookhub.com";
        string adminPassword = "Admin123!";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                DisplayName = "Admin",
                DateJoined = DateTime.Now
            };
            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Seeding admin user failed: {errors}");
            }

            await userManager.AddToRoleAsync(adminUser, "Admin");
        }

        // Default normal user for testing
        string testEmail = "testuser@bookhub.com";
        string testPassword = "User123!";
        var testUser = await userManager.FindByEmailAsync(testEmail);
        if (testUser == null)
        {
            testUser = new ApplicationUser
            {
                UserName = testEmail,
                Email = testEmail,
                DisplayName = "Test User",
                DateJoined = DateTime.Now
            };
            var result = await userManager.CreateAsync(testUser, testPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Seeding normal user failed: {errors}");
            }

            await userManager.AddToRoleAsync(testUser, "User");
        }
    }).GetAwaiter().GetResult();
}


// Seeding test Database entries
using (var scope = app.Services.CreateScope())
{
    // This seeding is no longer needed as it is handed by EF Core Migrations.
    // However, I will keep it here for reference and in case we want to add more complex seeding logic in the future.
    /*
    var dbPath = builder.Configuration.GetConnectionString("DefaultConnection")?.Replace("Data Source=", "");
    if (!string.IsNullOrEmpty(dbPath))
    {
        var dir = Path.GetDirectoryName(dbPath);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
    };
    */
    
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // Avoid duplicate seeding
    if (!context.Authors.Any() && !context.Books.Any() && !context.Genres.Any())
    {
        // --- Seed Authors ---
        var authors = new List<Author>
        {
            new Author { Name = "J.K. Rowling", Bio = "Author of the Harry Potter series" },
            new Author { Name = "George R.R. Martin", Bio = "Author of A Song of Ice and Fire" },
            new Author { Name = "J.R.R. Tolkien", Bio = "Author of The Lord of the Rings" },
            new Author { Name = "Name", Bio = "Bio"}
        };
        context.Authors.AddRange(authors);
        await context.SaveChangesAsync();

        // --- Seed Genres ---
        var genres = new List<Genre>
        {
            new Genre { Name = "Fantasy" },
            new Genre { Name = "Adventure" },
            new Genre { Name = "Science Fiction" },
            new Genre { Name = "Mystery" }
        };
        context.Genres.AddRange(genres);
        await context.SaveChangesAsync();

        // --- Seed Books ---
        var books = new List<Book>
        {
            new Book { Title = "Harry Potter and the Philosopher's Stone", Description = "A young wizard discovers his powers.", AuthorId = authors[0].Id },
            new Book { Title = "A Game of Thrones", Description = "Noble families fight for control of the Iron Throne.", AuthorId = authors[1].Id },
            new Book { Title = "The Fellowship of the Ring", Description = "A hobbit embarks on a quest to destroy a powerful ring.", AuthorId = authors[2].Id },
            new Book { Title = "Title", Description = "Description", AuthorId = authors[3].Id}
        };
        context.Books.AddRange(books);
        await context.SaveChangesAsync();

        // --- Seed BookGenres (many-to-many link table) ---
        var bookGenres = new List<BookGenre>
        {
            new BookGenre { BookId = books[0].Id, GenreId = genres.First(g => g.Name == "Fantasy").Id },
            new BookGenre { BookId = books[0].Id, GenreId = genres.First(g => g.Name == "Adventure").Id },
            new BookGenre { BookId = books[1].Id, GenreId = genres.First(g => g.Name == "Fantasy").Id },
            new BookGenre { BookId = books[1].Id, GenreId = genres.First(g => g.Name == "Adventure").Id },
            new BookGenre { BookId = books[2].Id, GenreId = genres.First(g => g.Name == "Fantasy").Id },
            new BookGenre { BookId = books[2].Id, GenreId = genres.First(g => g.Name == "Adventure").Id },
            new BookGenre { BookId = books[3].Id, GenreId = genres.First(g => g.Name == "Mystery").Id }
        };
        context.BookGenres.AddRange(bookGenres);

        await context.SaveChangesAsync();

        Console.WriteLine("Seeded sample authors, books, and genres.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();