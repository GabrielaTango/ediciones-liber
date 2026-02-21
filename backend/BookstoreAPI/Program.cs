using BookstoreAPI.Data;
using BookstoreAPI.Models.Afip;
using BookstoreAPI.Repositories;
using BookstoreAPI.Services;
using BookstoreAPI.Services.Afip;
using BookstoreAPI.Services.Pdf;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configurar CORS para permitir comunicación con el frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("*") // Vite default ports
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// Configurar AFIP
builder.Services.Configure<AfipConfig>(builder.Configuration.GetSection("AfipConfig"));

// Registrar DapperContext como Singleton
builder.Services.AddSingleton<DapperContext>();

// Registrar Repositories
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IArticuloRepository, ArticuloRepository>();
builder.Services.AddScoped<IReferenceRepository, ReferenceRepository>();
builder.Services.AddScoped<IComprobanteRepository, ComprobanteRepository>();
builder.Services.AddScoped<ICuotaRepository, CuotaRepository>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IGastoRepository, GastoRepository>();
builder.Services.AddScoped<ICategoriaGastoRepository, CategoriaGastoRepository>();
builder.Services.AddScoped<IRemitoRepository, RemitoRepository>();
builder.Services.AddScoped<IProveedorRepository, ProveedorRepository>();
builder.Services.AddScoped<IComprobanteProveedorRepository, ComprobanteProveedorRepository>();
builder.Services.AddScoped<ICuotaProveedorRepository, CuotaProveedorRepository>();

// Registrar Services
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IArticuloService, ArticuloService>();
builder.Services.AddScoped<IReferenceService, ReferenceService>();
builder.Services.AddScoped<IComprobanteService, ComprobanteService>();
builder.Services.AddScoped<IGastoService, GastoService>();
builder.Services.AddScoped<ICategoriaGastoService, CategoriaGastoService>();
builder.Services.AddScoped<IRemitoService, RemitoService>();
builder.Services.AddScoped<IProveedorService, ProveedorService>();
builder.Services.AddScoped<IComprobanteProveedorService, ComprobanteProveedorService>();

// Registrar Services AFIP
builder.Services.AddScoped<IAfipAuthService, AfipAuthService>();
builder.Services.AddScoped<IAfipFacturacionService, AfipFacturacionService>();
builder.Services.AddScoped<IAfipQrService, AfipQrService>();

// Registrar Services PDF
builder.Services.AddScoped<IComprobantePdfService, ComprobantePdfService>();
builder.Services.AddScoped<ICuotaPdfService, CuotaPdfService>();
builder.Services.AddScoped<IIvaVentasPdfService, IvaVentasPdfService>();
builder.Services.AddScoped<IIvaComprasPdfService, IvaComprasPdfService>();
builder.Services.AddScoped<IRemitoPdfService, RemitoPdfService>();
builder.Services.AddScoped<IArticulosVendidosZonaPdfService, ArticulosVendidosZonaPdfService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();
