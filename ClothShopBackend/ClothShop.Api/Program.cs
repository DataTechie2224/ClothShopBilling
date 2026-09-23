using Microsoft.EntityFrameworkCore;
using ClothShop.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// Add Database Context for SQL Server Express
builder.Services.AddDbContext<ClothShopDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Enable CORS for HTML/Tailwind frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", p =>
        p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cloth Shop API v10"));
}

app.UseCors("AllowAll");
app.UseStaticFiles(); // Serves Frontend files from wwwroot
app.UseAuthorization();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();