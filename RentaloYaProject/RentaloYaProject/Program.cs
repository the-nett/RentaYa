using Microsoft.EntityFrameworkCore;
using RentaloYa.Application.Common.Interfaces;
using RentaloYa.Application.Services;
using RentaloYa.Application.Services.InterfacesServices;
using RentaloYa.Infrastructure.Data;
using RentaloYa.Infrastructure.ExternalServices;
using RentaloYa.Infrastructure.Repository;
using Supabase;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Net.Mime.MediaTypeNames;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

//DbContext
builder.Services.AddDbContext<ApplicationDbContext>(option => option.UseSqlServer(builder.Configuration.GetConnectionString("LocalConnectionDev")));
//Dependency injection
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IGenderRepository, GenderRepository>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISupabaseAuthProvider, SupabaseAuthClient>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<IItemService, ItemService>();
//Supabase
builder.Services.AddScoped<Supabase.Client>(_ =>
    new Supabase.Client(
        builder.Configuration["Supabase:Url"],
        builder.Configuration["Supabase:PublicKey"],
        new SupabaseOptions
        {
            AutoRefreshToken = true,
            AutoConnectRealtime = true,
        }));

// Configurar Autenticación por Cookies
builder.Services.AddAuthentication("Cookies") // "Cookies" es el esquema de autenticación por defecto
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login"; // Redireccionar aquí si el usuario no está autenticado
        options.AccessDeniedPath = "/Auth/AccessDenied"; // Redireccionar aquí si no tiene permisos
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Duración de la cookie
        options.SlidingExpiration = true; // Renovar la cookie en cada solicitud si está a punto de expirar
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Habilitar la autenticación y la autorización
app.UseAuthentication(); // Debe ir antes de UseAuthorization
app.UseAuthorization();
app.MapControllerRoute(
    name: "supabase_callback",
    pattern: "auth/callback/supabase",
    defaults: new { controller = "Auth", action = "HandleSupabaseCallback" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
