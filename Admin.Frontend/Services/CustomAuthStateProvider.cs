using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Net.Http.Headers;

namespace Admin.Frontend.Services;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly IJSRuntime _jsRuntime;
    private readonly HttpClient _httpClient;
    private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

    public CustomAuthStateProvider(IJSRuntime jsRuntime, HttpClient httpClient)
    {
        _jsRuntime = jsRuntime;
        _httpClient = httpClient;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
            if (string.IsNullOrEmpty(token))
            {
                return new AuthenticationState(_anonymous);
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            if (jwtToken.ValidTo < DateTime.UtcNow)
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
                return new AuthenticationState(_anonymous);
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var identity = new ClaimsIdentity(jwtToken.Claims, "jwtAuthType");
            var user = new ClaimsPrincipal(identity);

            return new AuthenticationState(user);
        }
        catch
        {
            return new AuthenticationState(_anonymous);
        }
    }

    public void NotifyUserAuthentication(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);
        var identity = new ClaimsIdentity(jwtToken.Claims, "jwtAuthType");
        var user = new ClaimsPrincipal(identity);
        var authState = Task.FromResult(new AuthenticationState(user));
        NotifyAuthenticationStateChanged(authState);
    }

    // ✅ MÉTODO DE LOGOUT COMPLETO Y ROBUSTO
    public async Task LogoutAsync()
    {
        // 1. Borra el token del almacenamiento del navegador
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");

        // 2. Limpia la cabecera de autorización del HttpClient
        _httpClient.DefaultRequestHeaders.Authorization = null;

        // 3. Notifica a Blazor que el usuario ahora es anónimo
        var authState = Task.FromResult(new AuthenticationState(_anonymous));
        NotifyAuthenticationStateChanged(authState);
    }
}