using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", async () =>
{
    try
    {
        // Obtener el usuario configurado para el acceso sin contraseña
        // Asegúrate de que la configuración tenga la sección PasswordlessUser con el Email
        string passwordlessUser = builder.Configuration["PasswordlessUser:Email"];
        if (string.IsNullOrEmpty(passwordlessUser))
        {
            return Results.BadRequest("Passwordless user is not configured.");
        }
        // Llamar al método para obtener la URL del magic link
        var url = await GetUrlAsync(userName: passwordlessUser, builder.Configuration);
        if (string.IsNullOrEmpty(url))
        {
            return Results.StatusCode(403);
        }
        // Redirigir al usuario a la URL del magic link
        if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
        {
            return Results.BadRequest("The generated URL is not valid.");
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

static async Task<string?> GetUrlAsync(string userName, IConfiguration config)
{
    string url = string.Empty;

    // Base URL del servicio de autenticación
    var authBaseUrl = config["Auth:BaseUrl"].ToString() ?? "";
    if (string.IsNullOrEmpty(authBaseUrl))
        throw new ArgumentException("AuthBaseUrl is not configured.");

    using var httpClient = new HttpClient { BaseAddress = new Uri(authBaseUrl) };

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
    var loginContent = await loginResponse.Content.ReadFromJsonAsync<JsonDocument>();
    if (loginContent == null)
        throw new InvalidOperationException("Login response content is null.");

    // Extraer el token del contenido de la respuesta
    var token = loginContent.RootElement.GetProperty("token").GetString();
    if (string.IsNullOrEmpty(token))
        throw new InvalidOperationException("Token is missing in the login response.");

    // Agregar el token en el header para la siguiente llamada
    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    // Hacer POST /sie/login_magic_link/{email}
    var magicLinkResponse = await httpClient.PostAsync($"/sie/login_magic_link/{Uri.EscapeDataString(userName)}", null);
    if (!magicLinkResponse.IsSuccessStatusCode)
        throw new HttpRequestException($"Magic link request failed with status code: {magicLinkResponse.StatusCode}");

    // Leer el contenido de la respuesta del magic link
    var magicLinkContent = await magicLinkResponse.Content.ReadFromJsonAsync<JsonDocument>();
    if (magicLinkContent == null)
        throw new InvalidOperationException("Magic link response content is null.");

    // Extraer la URL del contenido de la respuesta
    url = magicLinkContent.RootElement.GetProperty("url").GetString();
    if (string.IsNullOrEmpty(url))
        throw new InvalidOperationException("URL is missing in the magic link response.");

    return url;
}
