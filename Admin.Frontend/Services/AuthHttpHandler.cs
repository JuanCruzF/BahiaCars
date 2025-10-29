using System.Net.Http.Headers;
using Microsoft.JSInterop;

namespace Admin.Frontend.Services;

public class AuthHttpHandler : DelegatingHandler
{
    private readonly IJSRuntime _jsRuntime;

    public AuthHttpHandler(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Obtenemos el token del Local Storage
        var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");

        if (!string.IsNullOrEmpty(token))
        {
            // Lo añadimos a la cabecera de la petición
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}