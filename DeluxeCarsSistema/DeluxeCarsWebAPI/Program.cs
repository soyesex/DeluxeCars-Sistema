
using DeluxeCarsWebAPI.Services;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddRazorPages(); // O AddControllersWithViews() para MVC

// --- REGISTRAR NUESTRO API CLIENT ---
builder.Services.AddHttpClient<IApiClient, ApiClient>(client =>
{
    // Leemos la URL base desde appsettings.json
    string baseUrl = builder.Configuration["WebApiBaseUrl"]
        ?? throw new InvalidOperationException("La URL de la WebAPI no está configurada en appsettings.json");

    client.BaseAddress = new Uri(baseUrl);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.MapRazorPages();

app.Run();
