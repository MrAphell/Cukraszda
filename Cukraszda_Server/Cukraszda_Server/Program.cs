using Cukraszda_Server.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Az adatbázis kapcsolat nincs konfigurálva. Kérjük, ellenõrizze az appsettings.json fájlt!");
}

var dataService = new DataService(connectionString);

builder.Services.AddSingleton(dataService);
builder.Services.AddGrpc();

var app = builder.Build();

app.MapGrpcService<AuthServiceImpl>();
app.MapGrpcService<CukraszdaServiceImpl>();
app.MapGet("/", context =>
{
    context.Response.ContentType = "text/plain; charset=utf-8";
    return context.Response.WriteAsync("gRPC szerver fut. Használjon gRPC klienst a kommunikációhoz.");
});

app.Run();