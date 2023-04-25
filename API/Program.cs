using API.Extensions;
using Infraestructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

//Configuracion de AutoMapper para el mapeo de entidades a dtos y viceversa
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());
//Configuracion para la limitacion de peticiones por un rango de tiempo
builder.Services.ConfigureRateLimitiong();

builder.Services.ConfigureCors();
builder.Services.AddAplicacionServices();
//Configuracion para el versionado de la API
builder.Services.ConfigureApiVersioning();
//Configurar JWT
builder.Services.AddJwt(builder.Configuration);

// Add services to the container.
builder.Services.AddControllers();

//Manejo de errores del model state(Anotaciones como Required, Email, etc)
builder.Services.AddValidationErrors();

//Comunication with MYSQL Database
builder.Services.AddDbContext<SecurityContext>(options =>
{
    var conectionString = Environment.GetEnvironmentVariable("TEMPALERT_SECURITY_DATABASE_CONNECTION");
    options.UseMySql(conectionString, ServerVersion.AutoDetect(conectionString));
});

//Add identity service 
builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<SecurityContext>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseStatusCodePagesWithReExecute("/errors/{0}");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //Database migration
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var loggerFactory = services.GetRequiredService<ILoggerFactory>();
        try
        {
            var context = services.GetRequiredService<SecurityContext>();
            await context.Database.MigrateAsync();
            await SecurityContextSeed.SeedRolsAsync(context, loggerFactory);
        }
        catch (Exception ex)
        {
            var _logger = loggerFactory.CreateLogger<Program>();
            _logger.LogError(ex, "Ocurrio un error durante la migracion");
        }
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("CorsPolicy");

app.UseHttpsRedirection();

//Enable authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
