using Microsoft.EntityFrameworkCore;
using RentaloYa.Application.Common.Interfaces; // Para IItemRepository, ISearchService, IImageTaggingService
using RentaloYa.Application.Services; // Para SearchService, ItemService, UserService, AuthService
using RentaloYa.Application.Services.InterfacesServices; // Para IAuthService, IUserService, IItemService, IPostService
using RentaloYa.Infrastructure.Data; // Para ApplicationDbContext
using RentaloYa.Infrastructure.ExternalServices; // Para GeminiImageTaggingService, SupabaseAuthClient
using RentaloYa.Infrastructure.Repository; // Para ItemRepository, UserRepository, GenderRepository, PostRepository, Repository<T>
using Supabase;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Http; // Añade este using

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// ***** NUEVO: Necesario para acceder al HttpContext (usuario actual) desde la inyección de dependencias *****
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient(); // Útil para servicios que hagan llamadas HTTP salientes (como Gemini o SupabaseAuthClient)

// DbContext
// Asegúrate de que "LocalConnectionDev" esté definido en appsettings.json
builder.Services.AddDbContext<ApplicationDbContext>(option =>
    option.UseSqlServer(builder.Configuration.GetConnectionString("LocalConnectionDev")));

// Dependency Injection - Repositorios
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IGenderRepository, GenderRepository>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>)); // Si tienes una implementación genérica
builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<IPostRepository, PostRepository>(); // Añadido

// Dependency Injection - Servicios de Aplicación
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISupabaseAuthProvider, SupabaseAuthClient>(); // Proveedor de autenticación Supabase
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IItemService, ItemService>();
builder.Services.AddScoped<IPostService, PostService>(); // Añadido

// ***** NUEVAS INYECCIONES AÑADIDAS/REVISADAS *****
builder.Services.AddScoped<IImageTaggingService, GeminiImageTaggingService>(); // Servicio de etiquetado de imágenes
builder.Services.AddScoped<ISearchService, SearchService>(); // ¡El servicio de búsqueda que usa el controlador!

// --- INICIO: CONFIGURACIÓN AVANZADA DE SUPABASE.CLIENT ---
// Mantenemos AddScoped para que cada solicitud obtenga una instancia de Supabase.Client
// configurada con el usuario actual, si está logueado.
builder.Services.AddScoped<Supabase.Client>(sp =>
{
    var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
    var user = httpContextAccessor.HttpContext?.User; // Obtenemos el ClaimsPrincipal del usuario logueado

    var supabaseUrl = builder.Configuration["Supabase:Url"];
    var supabaseAnonKey = builder.Configuration["Supabase:PublicKey"]; // Asegúrate de que esta es tu ANON_KEY (public key)

    // Validar que las configuraciones de Supabase no sean nulas o vacías
    if (string.IsNullOrEmpty(supabaseUrl))
    {
        throw new InvalidOperationException("Supabase:Url is not configured in appsettings.json.");
    }
    if (string.IsNullOrEmpty(supabaseAnonKey))
    {
        throw new InvalidOperationException("Supabase:PublicKey is not configured in appsettings.json.");
    }

    var options = new SupabaseOptions
    {
        AutoRefreshToken = true,
        AutoConnectRealtime = true,
        // Puedes añadir o quitar más opciones aquí según tus necesidades
        // por ejemplo, Debug = true para ver logs internos de Supabase-csharp
    };

    Supabase.Client client;

    // Recuperamos el access_token y refresh_token de los Claims del usuario.
    // Estos claims deben haber sido guardados en el proceso de login/registro.
    var accessToken = user?.FindFirst("SupabaseAccessToken")?.Value;
    var refreshToken = user?.FindFirst("SupabaseRefreshToken")?.Value;

    if (!string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(refreshToken))
    {
        // Creamos el cliente de Supabase con los tokens del usuario.
        client = new Supabase.Client(supabaseUrl, supabaseAnonKey, options);
        // Establecemos la sesión con el token del usuario actual.
        // Esto hace que las futuras llamadas a Supabase desde este cliente se autentiquen como ese usuario.
        client.Auth.SetSession(accessToken, refreshToken);
    }
    else
    {
        // Si no hay un usuario autenticado en la solicitud actual (ej. para páginas públicas
        // o antes de que el usuario haya iniciado sesión), inicializamos el cliente
        // solo con la clave anónima. Esto es seguro y permite acceso público a Supabase
        // si tus políticas de RLS lo permiten (ej. lectura pública de ítems).
        client = new Supabase.Client(supabaseUrl, supabaseAnonKey, options);
    }

    return client; // Devolvemos la instancia del cliente Supabase configurada para esta solicitud.
});
// --- FIN: CONFIGURACIÓN AVANZADA DE SUPABASE.CLIENT ---


// Configurar Autenticación por Cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme) // Mejor usar la constante
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
        // Si necesitas persistir el token de Supabase en la cookie, podrías hacer algo así
        // options.Events.OnValidatePrincipal = async context => { /* Lógica para revalidar/refrescar token de Supabase */ };
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

// Habilitar la autenticación y la autorización (el orden es importante: Authentication antes de Authorization)
app.UseAuthentication();
app.UseAuthorization();

// Rutas de tu aplicación
app.MapControllerRoute(
    name: "supabase_callback",
    pattern: "auth/callback/supabase",
    defaults: new { controller = "Auth", action = "HandleSupabaseCallback" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();