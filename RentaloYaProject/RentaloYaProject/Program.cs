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
using Microsoft.AspNetCore.Http; // A�ade este using

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// ***** NUEVO: Necesario para acceder al HttpContext (usuario actual) desde la inyecci�n de dependencias *****
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient(); // �til para servicios que hagan llamadas HTTP salientes (como Gemini o SupabaseAuthClient)

// DbContext
// Aseg�rate de que "LocalConnectionDev" est� definido en appsettings.json
builder.Services.AddDbContext<ApplicationDbContext>(option =>
    option.UseSqlServer(builder.Configuration.GetConnectionString("LocalConnectionDev")));

// Dependency Injection - Repositorios
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IGenderRepository, GenderRepository>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>)); // Si tienes una implementaci�n gen�rica
builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<IPostRepository, PostRepository>(); // A�adido

// Dependency Injection - Servicios de Aplicaci�n
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISupabaseAuthProvider, SupabaseAuthClient>(); // Proveedor de autenticaci�n Supabase
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IItemService, ItemService>();
builder.Services.AddScoped<IPostService, PostService>(); // A�adido

// ***** NUEVAS INYECCIONES A�ADIDAS/REVISADAS *****
builder.Services.AddScoped<IImageTaggingService, GeminiImageTaggingService>(); // Servicio de etiquetado de im�genes
builder.Services.AddScoped<ISearchService, SearchService>(); // �El servicio de b�squeda que usa el controlador!

// --- INICIO: CONFIGURACI�N AVANZADA DE SUPABASE.CLIENT ---
// Mantenemos AddScoped para que cada solicitud obtenga una instancia de Supabase.Client
// configurada con el usuario actual, si est� logueado.
builder.Services.AddScoped<Supabase.Client>(sp =>
{
    var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
    var user = httpContextAccessor.HttpContext?.User; // Obtenemos el ClaimsPrincipal del usuario logueado

    var supabaseUrl = builder.Configuration["Supabase:Url"];
    var supabaseAnonKey = builder.Configuration["Supabase:PublicKey"]; // Aseg�rate de que esta es tu ANON_KEY (public key)

    // Validar que las configuraciones de Supabase no sean nulas o vac�as
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
        // Puedes a�adir o quitar m�s opciones aqu� seg�n tus necesidades
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
        // Establecemos la sesi�n con el token del usuario actual.
        // Esto hace que las futuras llamadas a Supabase desde este cliente se autentiquen como ese usuario.
        client.Auth.SetSession(accessToken, refreshToken);
    }
    else
    {
        // Si no hay un usuario autenticado en la solicitud actual (ej. para p�ginas p�blicas
        // o antes de que el usuario haya iniciado sesi�n), inicializamos el cliente
        // solo con la clave an�nima. Esto es seguro y permite acceso p�blico a Supabase
        // si tus pol�ticas de RLS lo permiten (ej. lectura p�blica de �tems).
        client = new Supabase.Client(supabaseUrl, supabaseAnonKey, options);
    }

    return client; // Devolvemos la instancia del cliente Supabase configurada para esta solicitud.
});
// --- FIN: CONFIGURACI�N AVANZADA DE SUPABASE.CLIENT ---


// Configurar Autenticaci�n por Cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme) // Mejor usar la constante
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
        // Si necesitas persistir el token de Supabase en la cookie, podr�as hacer algo as�
        // options.Events.OnValidatePrincipal = async context => { /* L�gica para revalidar/refrescar token de Supabase */ };
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

// Habilitar la autenticaci�n y la autorizaci�n (el orden es importante: Authentication antes de Authorization)
app.UseAuthentication();
app.UseAuthorization();

// Rutas de tu aplicaci�n
app.MapControllerRoute(
    name: "supabase_callback",
    pattern: "auth/callback/supabase",
    defaults: new { controller = "Auth", action = "HandleSupabaseCallback" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();