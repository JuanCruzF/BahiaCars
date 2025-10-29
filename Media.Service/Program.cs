using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// --- SERVICIOS ---
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policyBuilder =>
    {
        policyBuilder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

// --- PIPELINE ---
app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthorization();

//CONFIGURACIÓN: Servir archivos desde /uploads
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "uploads")),
    RequestPath = "/uploads",
    ServeUnknownFileTypes = true,
    DefaultContentType = "application/octet-stream"
});

app.MapControllers();
app.Run();