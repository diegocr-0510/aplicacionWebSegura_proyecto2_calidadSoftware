using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Proyecto2Seguridad.Web.Data;
using Proyecto2Seguridad.Web.Models;
using Proyecto2Seguridad.Web.Seed;
using Proyecto2Seguridad.Web.Services;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

/// <summary>
/// Configuración de conexión a PostgreSQL usando la misma base de datos ya existente.
/// </summary>

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

/// <summary>
/// Configuración de Identity para usuarios y roles.
/// Aquí se definen reglas básicas de contraseña.
/// </summary>
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireDigit = true;
    options.Password.RequireNonAlphanumeric = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

/// <summary>
/// Configuración de la cookie de autenticación.
/// Se deja con un tiempo de expiración corto para alinearse con la seguridad del proyecto.
/// </summary>
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
    options.SlidingExpiration = true;
    options.LoginPath = "/Auth/Login";
    options.AccessDeniedPath = "/Auth/AccessDenied";
});

builder.Services.AddScoped<AuditService>();
builder.Services.AddControllersWithViews();
builder.Services.AddControllers();
builder.Services.AddScoped<JwtTokenService>();

var app = builder.Build();

// Configuración de autenticación JWT para la API
var jwtKey = builder.Configuration["JwtSettings:Key"]!;
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"]!;
var jwtAudience = builder.Configuration["JwtSettings:Audience"]!;

builder.Services.AddAuthentication()
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // Validar emisor
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,

            // Validar audiencia
            ValidateAudience = true,
            ValidAudience = jwtAudience,

            // Validar expiración
            ValidateLifetime = true,

            // Validar firma
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),

            // Reducir margen de tolerancia de tiempo
            ClockSkew = TimeSpan.Zero
        };
    });

/// <summary>
/// Configuración del pipeline HTTP.
/// </summary>
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Mapear controladores API
app.MapControllers();



/// <summary>
/// Crear roles y usuario administrador inicial al arrancar la aplicación.
/// </summary>
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

    await DbInitializer.SeedRolesAndAdminAsync(userManager, roleManager);
}

app.Run();