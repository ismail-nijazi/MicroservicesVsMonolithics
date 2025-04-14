using InventoryManagement.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var connectionString = Environment.GetEnvironmentVariable("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("Database connection string is missing.");
}
else
{
    Console.WriteLine($"Connection string is: {connectionString}");
}
builder.Services.AddDbContext<InventoryContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddControllers();


var app = builder.Build();

app.UseCors("AllowAll");

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<InventoryContext>();
    bool isConnected = dbContext.Database.CanConnect();
    if (isConnected)
    {
        Console.WriteLine("Database connection successful!");
    }
    else
    {
        Console.WriteLine("Database connection failed!");
    }
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
