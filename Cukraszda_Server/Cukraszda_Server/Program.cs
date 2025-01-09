using Cukraszda_Server.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Az adatb�zis kapcsolat nincs konfigur�lva. K�rj�k, ellen�rizze az appsettings.json f�jlt!");
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
    return context.Response.WriteAsync("gRPC szerver fut. Haszn�ljon gRPC klienst a kommunik�ci�hoz.");
});

app.Run();