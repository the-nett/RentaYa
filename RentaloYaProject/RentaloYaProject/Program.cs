
using Microsoft.EntityFrameworkCore;
using RentaloYa.Application.Common.Interfaces;
using RentaloYa.Application.Services;
using RentaloYa.Application.Services.InterfacesServices;
using RentaloYa.Infrastructure.Data;
using RentaloYa.Infrastructure.ExternalServices;
using RentaloYa.Infrastructure.Repository;
using Supabase;
using Microsoft.AspNetCore.Authentication.Cookies; // Necesario para acceder a las opciones de cookie
using System.Security.Claims; // Necesario para trabajar con Claims

// Importante: Necesitas este using para IHttpContextAccessor
using Microsoft.AspNetCore.Http; // Añade este using

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// ***** NUEVO: Necesario para acceder al HttpContext (usuario actual) desde la inyección de dependencias *****
builder.Services.AddHttpContextAccessor();

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


// --- INICIO: CAMBIO CLAVE EN LA CONFIGURACIÓN DE SUPABASE.CLIENT ---
// Mantenemos AddScoped, pero ahora la función que crea el cliente
// es inteligente sobre el usuario actual.
builder.Services.AddScoped<Supabase.Client>(sp =>
{
    // Obtenemos el HttpContextAccessor del proveedor de servicios.
    // Esto nos permite acceder al contexto HTTP de la solicitud actual.
    var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
    var user = httpContextAccessor.HttpContext?.User; // Obtenemos el ClaimsPrincipal del usuario logueado

    // Obtenemos la URL y la Clave Pública (AnonKey) de Supabase desde la configuración.
    var supabaseUrl = builder.Configuration["Supabase:Url"];
    var supabaseAnonKey = builder.Configuration["Supabase:PublicKey"]; // Asegúrate de que esta es tu ANON_KEY (public key)

    // Recuperamos el access_token y refresh_token de los Claims del usuario.
    // Estos claims deben haber sido guardados en el Paso 1 (TokenValidation Controller).
    var accessToken = user?.FindFirst("SupabaseAccessToken")?.Value;
    var refreshToken = user?.FindFirst("SupabaseRefreshToken")?.Value;

    // Configuración de opciones para el cliente Supabase.
    var options = new SupabaseOptions
    {
        AutoRefreshToken = true,       // Muy importante para mantener la sesión viva
        AutoConnectRealtime = true,    // Si lo necesitas, manténlo
        // Puedes añadir otras opciones aquí si las tenías antes.
    };

    Supabase.Client client;

    // Si hay un usuario autenticado (es decir, tenemos un access_token y refresh_token en los claims)
    if (!string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(refreshToken))
    {
        // Creamos el cliente de Supabase.
        client = new Supabase.Client(supabaseUrl, supabaseAnonKey, options);

        // ¡Este es el paso crítico! Le decimos al cliente de Supabase que establezca la sesión
        // con el token del usuario actual. Esto hace que las futuras llamadas a Supabase
        // desde este cliente (durante esta solicitud HTTP) se autentiquen como ese usuario.
        client.Auth.SetSession(accessToken, refreshToken);
    }
    else
    {
        // Si no hay un usuario autenticado en la solicitud actual (ej. para páginas públicas
        // o antes de que el usuario haya iniciado sesión), inicializamos el cliente
        // solo con la clave anónima. Esto es seguro y permite acceso público a Supabase
        // si tus políticas lo permiten (ej. lectura pública de ítems).
        client = new Supabase.Client(supabaseUrl, supabaseAnonKey, options);
    }

    return client; // Devolvemos la instancia del cliente Supabase configurada para esta solicitud.
});
// --- FIN: CAMBIO CLAVE EN LA CONFIGURACIÓN DE SUPABASE.CLIENT ---


// Configurar Autenticación por Cookies (sin cambios aquí)
builder.Services.AddAuthentication("Cookies")
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Habilitar la autenticación y la autorización (el orden es importante)
app.UseAuthentication(); // Debe ir antes de UseAuthorization
app.UseAuthorization();

// Rutas de tu aplicación (sin cambios aquí)
app.MapControllerRoute(
    name: "supabase_callback",
    pattern: "auth/callback/supabase",
    defaults: new { controller = "Auth", action = "HandleSupabaseCallback" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
