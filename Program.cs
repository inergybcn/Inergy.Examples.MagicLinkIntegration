using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Base URL del servicio de autenticación
var authBaseUrl = builder.Configuration["Auth:BaseUrl"]?.ToString() ?? "";
if (string.IsNullOrEmpty(authBaseUrl))
    throw new ArgumentException("AuthBaseUrl is not configured.");

using var httpClient = new HttpClient { BaseAddress = new Uri(authBaseUrl) };

// Obtener Token del usuario Administrador del servicio de autenticación
var token = await GetTokenAsync(httpClient, builder.Configuration);

app.MapGet("/", async () =>
{
    try
    {
        // Obtener el usuario configurado para el acceso sin contraseña
        // Asegúrate de que la configuración tenga la sección PasswordlessUser con el Email
        var passwordlessUser = builder.Configuration["PasswordlessUser:Email"];
        if (string.IsNullOrEmpty(passwordlessUser))
        {
            return Results.BadRequest("Passwordless user is not configured.");
        }
        // Llamar al método para obtener la URL del magic link
        var url = await GetUrlAsync(token, userName: passwordlessUser, httpClient, builder.Configuration);
        if (string.IsNullOrEmpty(url))
        {
            return Results.StatusCode(403);
        }

        // Redirigir al usuario a la URL del magic link
        if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
        {
            return Results.BadRequest("The generated URL is not valid.");
        }

        // Añadir parámetros adicionales a la URL sólo si es necesario: código
        var code = builder.Configuration["PasswordlessUser:Code"];
        if (!string.IsNullOrEmpty(code))
        {
            url += $"&code={Uri.EscapeDataString(code)}";
        }

        // Añadir parámetros adicionales a la URL sólo si es necesario: idioma
        var lang = builder.Configuration["PasswordlessUser:Lang"];
        if (!string.IsNullOrEmpty(lang))
        {
            url += $"&lang={lang}";
        }  

        return Results.Redirect(url);
    }
    catch (Exception ex)
    {
        // Manejo de errores, puedes registrar el error o retornar un mensaje específico
        return Results.Problem("An error occurred while processing your request: " + ex.Message);
    }
});

app.Run();

/// Método para obtener la URL del magic link del usuario configurado para el acceso sin contraseña
static async Task<string?> GetUrlAsync(string? token, string userName, HttpClient httpClient, IConfiguration config)
{
    var url = string.Empty;

    // Para llamadas recurrentes a login_magic_link se recomienda guardar de manera persistente el token y comporbar su validez
    if (!IsValidToken(token))
        token = await GetTokenAsync(httpClient, config);

    // Agregar el token en el header para la siguiente llamada    
    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    // Hacer POST /sie/login_magic_link/{email}
    var magicLinkResponse = await httpClient.PostAsync($"/sie/login_magic_link/{Uri.EscapeDataString(userName)}", null);
    if (!magicLinkResponse.IsSuccessStatusCode)
        throw new HttpRequestException($"Magic link request failed with status code: {magicLinkResponse.StatusCode}");

    // Leer el contenido de la respuesta del magic link
    var magicLinkContent = await magicLinkResponse.Content.ReadFromJsonAsync<JsonDocument>()
                            ?? throw new InvalidOperationException("Magic link response content is null.");

    // Extraer la URL del contenido de la respuesta
    url = magicLinkContent.RootElement.GetProperty("url").GetString();
    if (string.IsNullOrEmpty(url))
        throw new InvalidOperationException("URL is missing in the magic link response.");

    return url;
}

/// Método para obtener el token JWT del usuario Administrador del servicio de autenticación
static async Task<string?> GetTokenAsync(HttpClient httpClient, IConfiguration config)
{
    // Construir el cuerpo del login
    var loginData = new
    {
        email = config["MasterUser:Email"],
        password = config["MasterUser:Password"]
    };

    // Hacer POST /account/login
    var loginResponse = await httpClient.PostAsJsonAsync("/account/login", loginData);
    if (!loginResponse.IsSuccessStatusCode)
        throw new HttpRequestException($"Master login failed with status code: {loginResponse.StatusCode}");

    // Leer el contenido de la respuesta del login
    var loginContent = await loginResponse.Content.ReadFromJsonAsync<JsonDocument>()
                        ?? throw new InvalidOperationException("Login response content is null.");

    // Extraer el token del contenido de la respuesta
    var token = loginContent.RootElement.GetProperty("token").GetString();
    if (string.IsNullOrEmpty(token))
        throw new InvalidOperationException("Token is missing in the login response.");

    return token;
}

/// Método para validar el token JWT
static bool IsValidToken(string? token)
{
    var handler = new JwtSecurityTokenHandler();
    var jwtToken = handler.ReadJwtToken(token);
    return jwtToken != null && jwtToken.ValidTo >= DateTime.UtcNow;
}