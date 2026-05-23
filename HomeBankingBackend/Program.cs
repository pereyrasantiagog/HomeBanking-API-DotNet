using Microsoft.EntityFrameworkCore;
using HomeBankingBackend.Data;
using HomeBankingBackend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// --- INICIO CONFIGURACIÓN CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
// --- FIN CONFIGURACIÓN CORS ---

// --- INICIO CONFIGURACIÓN JWT ---
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? ""))
        };
    });
// --- FIN CONFIGURACIÓN JWT ---

// Leer la cadena de conexión
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Configurar Entity Framework para MySQL
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

// Registrar la capa de servicios
builder.Services.AddScoped<ITransactionService, TransactionService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
// --- INICIO CONFIGURACIÓN SWAGGER CON JWT ---
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "HomeBankingAPI", Version = "v1" });

    // Definimos el esquema de seguridad (El botón "Authorize")
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Autorización JWT usando el esquema Bearer. \r\n\r\n Escribe 'Bearer' [espacio] y luego tu token.\r\n\r\nEjemplo: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Le decimos a Swagger que use este esquema en todos los endpoints
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});
// --- FIN CONFIGURACIÓN SWAGGER CON JWT ---

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// La regla de oro: UseCors DEBE ir antes de UseAuthentication y UseAuthorization
app.UseCors("AllowReactFrontend");

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
