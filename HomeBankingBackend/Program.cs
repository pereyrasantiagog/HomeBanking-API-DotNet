using Microsoft.EntityFrameworkCore;
using HomeBankingBackend.Data;

var builder = WebApplication.CreateBuilder(args);

// Leer la cadena de conexión
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Configurar Entity Framework para MySQL
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();