using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Security.Cryptography;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BCrypt.Net;
using EStoreManagementAPI;
using System.IO;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// ===================== DATABASE =====================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=shop.db"));

// ===================== JWT =====================
var key = builder.Configuration["Jwt:Key"] ?? Environment.GetEnvironmentVariable("JWT_KEY") ?? "SUPER_SECRET_KEY_12345";

// Ensure signing key is at least 256 bits. If configured key is shorter, use its SHA256 hash.
var keyBytes = Encoding.UTF8.GetBytes(key);
if (keyBytes.Length < 32)
{
    keyBytes = SHA256.HashData(Encoding.UTF8.GetBytes(key));
}

// JWT authentication handled manually for endpoints that require it

// Data Protection: persist keys to disk and protect with DPAPI on Windows
var keysFolder = Path.Combine(builder.Environment.ContentRootPath, "DataProtection-Keys");
Directory.CreateDirectory(keysFolder);
var dpBuilder = builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysFolder))
    .SetApplicationName("EStoreManagementAPI");

// Call ProtectKeysWithDpapi only on Windows. Use reflection to avoid CA1416 analyzer.
if (OperatingSystem.IsWindows())
{
    var extType = Type.GetType("Microsoft.AspNetCore.DataProtection.DataProtectionBuilderExtensions, Microsoft.AspNetCore.DataProtection");
    var protectMethod = extType?.GetMethod("ProtectKeysWithDpapi", new[] { typeof(Microsoft.AspNetCore.DataProtection.IDataProtectionBuilder) });
    if (protectMethod != null)
    {
        protectMethod.Invoke(null, new object[] { dpBuilder });
    }
}

// ===================== CORS =====================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ===================== LOGGING =====================
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// ===================== SWAGGER =====================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Shop API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// Ensure database is created and seed minimal data for local testing
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    if (!db.Categories.Any())
    {
        var defaultCat = new Category { Name = "Default" };
        db.Categories.Add(defaultCat);
        db.Products.Add(new Product { Name = "Sample Product", Price = 9.99M, Category = defaultCat });
        db.Users.Add(new User { Email = "user@example.com" });
        db.SaveChanges();
    }
}

// ===================== MIDDLEWARE =====================
app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");

// Authentication/Authorization middleware not used because JWT is validated manually

// ===================== API ENDPOINTS =====================

// -------- PRODUCTS --------
app.MapGet("/api/products", GetProducts);

app.MapGet("/api/products/search/{query}", async (string query, AppDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(query))
        return Results.BadRequest(new { message = "Search query is required." });

    var results = await db.Products
        .Include(p => p.Category)
        .Where(p => p.Name.Contains(query) || (p.Category != null && p.Category!.Name.Contains(query)))
        .Take(20)
        .ToListAsync();

    return Results.Ok(results);
});

app.MapGet("/api/products/{id}", async (int id, AppDbContext db) =>
{
    var product = await db.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
    return product == null ? Results.NotFound() : Results.Ok(product);
});

app.MapPost("/api/products", async (Product product, AppDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(product.Name) || product.Price <= 0 || product.CategoryId <= 0)
        return Results.BadRequest(new { message = "Invalid product data: Name, Price > 0, and valid CategoryId required." });
    
    var category = await db.Categories.FindAsync(product.CategoryId);
    if (category == null)
        return Results.NotFound(new { message = "Category not found." });
    
    db.Products.Add(product);
    await db.SaveChangesAsync();
    return Results.Created($"/api/products/{product.Id}", product);
});

app.MapPut("/api/products/{id}", async (int id, Product product, AppDbContext db) =>
{
    var existing = await db.Products.FindAsync(id);
    if (existing == null)
        return Results.NotFound(new { message = "Product not found." });
    
    if (product.CategoryId > 0 && !await db.Categories.AnyAsync(c => c.Id == product.CategoryId))
        return Results.NotFound(new { message = "Category not found." });
    
    if (!string.IsNullOrWhiteSpace(product.Name)) existing.Name = product.Name;
    if (product.Price > 0) existing.Price = product.Price;
    if (product.CategoryId > 0) existing.CategoryId = product.CategoryId;
    
    await db.SaveChangesAsync();
    return Results.Ok(existing);
});

app.MapDelete("/api/products/{id}", async (int id, AppDbContext db) =>
{
    var product = await db.Products.FindAsync(id);
    if (product == null)
        return Results.NotFound(new { message = "Product not found." });
    
    db.Products.Remove(product);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// -------- CATEGORIES --------
app.MapGet("/api/categories", GetCategories);

app.MapGet("/api/categories/{id}", async (int id, AppDbContext db) =>
{
    var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == id);
    return category == null ? Results.NotFound() : Results.Ok(category);
});

app.MapPost("/api/categories", async (Category category, AppDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(category.Name))
        return Results.BadRequest(new { message = "Category name is required." });
    
    db.Categories.Add(category);
    await db.SaveChangesAsync();
    return Results.Created($"/api/categories/{category.Id}", category);
});

app.MapPut("/api/categories/{id}", async (int id, Category category, AppDbContext db) =>
{
    var existing = await db.Categories.FindAsync(id);
    if (existing == null)
        return Results.NotFound(new { message = "Category not found." });
    
    if (!string.IsNullOrWhiteSpace(category.Name))
        existing.Name = category.Name;
    
    await db.SaveChangesAsync();
    return Results.Ok(existing);
});

app.MapDelete("/api/categories/{id}", async (int id, AppDbContext db) =>
{
    var category = await db.Categories.FindAsync(id);
    if (category == null)
        return Results.NotFound(new { message = "Category not found." });
    
    if (await db.Products.AnyAsync(p => p.CategoryId == id))
        return Results.BadRequest(new { message = "Cannot delete category with products." });
    
    db.Categories.Remove(category);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// -------- ORDERS (JWT) --------
app.MapGet("/api/orders", async (HttpContext http, AppDbContext db) =>
{
    var authHeader = http.Request.Headers["Authorization"].FirstOrDefault();
    if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
        return Results.Unauthorized();

    var token = authHeader.Substring("Bearer ".Length).Trim();
    if (!JwtUtils.ValidateJwt(token, keyBytes)) return Results.Unauthorized();

    return Results.Ok(await db.Orders.Include(o => o.User).ToListAsync());
});

app.MapPost("/api/orders", async (HttpContext http, Order order, AppDbContext db) =>
{
    var authHeader = http.Request.Headers["Authorization"].FirstOrDefault();
    if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
        return Results.Unauthorized();

    var token = authHeader.Substring("Bearer ".Length).Trim();
    if (!JwtUtils.ValidateJwt(token, keyBytes)) return Results.Unauthorized();

    if (order.UserId <= 0)
        return Results.BadRequest(new { message = "Valid UserId required." });

    var user = await db.Users.FindAsync(order.UserId);
    if (user == null)
        return Results.NotFound(new { message = "User not found." });

    order.OrderDate = DateTime.UtcNow;
    db.Orders.Add(order);
    await db.SaveChangesAsync();
    return Results.Created($"/api/orders/{order.Id}", order);
});

app.MapPost("/api/auth/register", async (AuthRequest req, AppDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
        return Results.BadRequest(new { message = "Email and Password are required." });

    if (await db.Users.AnyAsync(u => u.Email == req.Email))
        return Results.Conflict(new { message = "User already exists." });

    var user = new User { Email = req.Email, PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password) };
    db.Users.Add(user);
    await db.SaveChangesAsync();
    return Results.Created($"/api/users/{user.Id}", new { user.Id, user.Email });
});

app.MapPost("/api/auth/login", async (AuthRequest req, AppDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
        return Results.BadRequest(new { message = "Email and Password are required." });

    var user = await db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
    if (user == null || string.IsNullOrWhiteSpace(user.PasswordHash) || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
        return Results.Unauthorized();

    var tokenHandler = new JwtSecurityTokenHandler();
    var claims = new[] {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.Email)
    };

    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(claims),
        Expires = DateTime.UtcNow.AddHours(1),
        SigningCredentials = new SigningCredentials(
            new SymmetricSecurityKey(keyBytes),
            SecurityAlgorithms.HmacSha256Signature)
    };

    var token = tokenHandler.CreateToken(tokenDescriptor);
    return Results.Ok(new { token = tokenHandler.WriteToken(token), expiresIn = "1 hour" });
});

// Public user endpoints (no password hash returned)
app.MapGet("/api/users", async (AppDbContext db) =>
    await db.Users.Select(u => new { u.Id, u.Email }).ToListAsync());

app.MapGet("/api/users/{id}", async (int id, AppDbContext db) =>
{
    var u = await db.Users.FindAsync(id);
    return u == null ? Results.NotFound() : Results.Ok(new { u.Id, u.Email });
});

// -------- HEALTH --------
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

// -------- ANALYTICS --------
app.MapGet("/api/stats", async (AppDbContext db) =>
{
    var products = await db.Products.ToListAsync();
    var avgPrice = products.Count > 0 ? products.Average(p => (double)p.Price) : 0;
    
    var topCatGroup = products
        .GroupBy(p => p.CategoryId)
        .OrderByDescending(g => g.Count())
        .FirstOrDefault();

    var stats = new
    {
        totalProducts = await db.Products.CountAsync(),
        totalCategories = await db.Categories.CountAsync(),
        totalOrders = await db.Orders.CountAsync(),
        totalUsers = await db.Users.CountAsync(),
        averageProductPrice = Math.Round(avgPrice, 2),
        topCategoryId = topCatGroup?.Key
    };

    return Results.Ok(stats);
});

app.MapGet("/api/products/by-category/{categoryId}", async (int categoryId, AppDbContext db) =>
{
    var products = await db.Products
        .Where(p => p.CategoryId == categoryId)
        .ToListAsync();

    if (!products.Any())
        return Results.NotFound(new { message = "No products found in this category." });

    return Results.Ok(products);
});

// DEBUG: validate JWT using the same simple logic
app.MapGet("/debug/validate", (string token) =>
{
    bool ValidateJwtSimpleLocal(string t)
    {
        try
        {
            var parts = t.Split('.');
            if (parts.Length != 3) return false;
            var signingInput = parts[0] + "." + parts[1];
            var sig = parts[2];

            byte[] hash;
            using (var hmac = new HMACSHA256(keyBytes))
            {
                hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signingInput));
            }

            string computed = Convert.ToBase64String(hash).TrimEnd('=').Replace('+', '-').Replace('/', '_');
            if (!string.Equals(computed, sig, StringComparison.Ordinal)) return false;

            var payloadJson = Encoding.UTF8.GetString(Convert.FromBase64String(parts[1].PadRight(parts[1].Length + (4 - parts[1].Length % 4) % 4, '=').Replace('-', '+').Replace('_', '/')));
            using var doc = System.Text.Json.JsonDocument.Parse(payloadJson);
            if (doc.RootElement.TryGetProperty("exp", out var expEl) && expEl.ValueKind == System.Text.Json.JsonValueKind.Number)
            {
                var exp = expEl.GetInt64();
                var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                if (now >= exp) return false;
            }

            return true;
        }
        catch { return false; }
    }

    return Results.Ok(new { valid = ValidateJwtSimpleLocal(token) });
});

app.Run();

// ===================== ENDPOINT HANDLERS =====================

async Task<IResult> GetProducts(int page, int pageSize, string? search, int? categoryId, AppDbContext db)
{
    page = page > 0 ? page : 1;
    pageSize = pageSize > 0 ? pageSize : 10;

    var query = db.Products.Include(p => p.Category).AsQueryable();

    // Filter by category
    if (categoryId.HasValue && categoryId > 0)
        query = query.Where(p => p.CategoryId == categoryId.Value);

    // Search by name
    if (!string.IsNullOrWhiteSpace(search))
        query = query.Where(p => p.Name.Contains(search));

    var totalCount = await query.CountAsync();
    var items = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    var result = new PagedResult<Product>
    {
        Items = items,
        TotalCount = totalCount,
        Page = page,
        PageSize = pageSize
    };

    return Results.Ok(result);
}

async Task<IResult> GetCategories(int page, int pageSize, AppDbContext db)
{
    page = page > 0 ? page : 1;
    pageSize = pageSize > 0 ? pageSize : 10;

    var totalCount = await db.Categories.CountAsync();
    var items = await db.Categories
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    var result = new PagedResult<Category>
    {
        Items = items,
        TotalCount = totalCount,
        Page = page,
        PageSize = pageSize
    };

    return Results.Ok(result);
}

// ===================== DB CONTEXT =====================
// AppDbContext moved to AppDbContext.cs
