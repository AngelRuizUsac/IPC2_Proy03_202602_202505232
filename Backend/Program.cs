using proyecto3.Backend.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddControllers();
string? carpeta = builder.Configuration["Datos:Carpeta"];
if (string.IsNullOrWhiteSpace(carpeta))
{
    carpeta = Path.Combine(builder.Environment.ContentRootPath, "datos");
}
Repositorio repositorio = new Repositorio(carpeta);
ServicioTributario servicio = new ServicioTributario(repositorio);
builder.Services.AddSingleton(typeof(ServicioTributario), servicio);
WebApplication app = builder.Build();
app.Lifetime.ApplicationStopped.Register(repositorio.Dispose);
app.UseExceptionHandler(error => error.Run(async contexto =>
{
    contexto.Response.StatusCode = 500;
    await contexto.Response.WriteAsJsonAsync(new { error = "No se pudo completar la operación. Revise el registro del servidor y el almacenamiento." });
}));
app.MapControllers();
app.Run();
