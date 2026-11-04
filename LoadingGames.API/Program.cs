
using LoadingGames.API.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// OpenAPI
builder.Services.AddOpenApi();

// Conexão com o MySQL
var connectionString =
	builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<LoadingGamesDbContext>(options =>
	options.UseMySql(
		connectionString,
		ServerVersion.AutoDetect(connectionString)
	)
);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

// Diagnóstico das pastas
Console.WriteLine("Pasta da API: " +
	app.Environment.ContentRootPath);

Console.WriteLine("Pasta das imagens: " +
	app.Environment.WebRootPath);

// Permite acessar imagens em wwwroot
app.UseStaticFiles();

// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run("http://0.0.0.0:5289");
