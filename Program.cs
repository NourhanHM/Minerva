using Microsoft.EntityFrameworkCore;
using Minerva.Data;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();


builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(61320); // Listen on port 5000 for any IP address
    options.ListenAnyIP(61321, listenOptions => listenOptions.UseHttps()); // HTTPS

});

// ... other configurations

// Add services to the container
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Controllers
builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder => builder
            .SetIsOriginAllowed(origin => true) // Allow all origins (including FlutterLab)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()); // Allow cookies/authentication if needed
});

// Add Swagger for API documentation
builder.Services.AddEndpointsApiExplorer();


builder.Services.AddSwaggerGen(options =>
{

    options.OperationFilter<SwaggerFileOperationFilter>();
    // Customize Swagger to resolve route conflicts (if needed)
    options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
     
    // Add custom metadata for Swagger
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Minerva API",
        Version = "v1",
        Description = "API documentation for Minerva project",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Your Name",
            Email = "your_email@example.com",
            Url = new Uri("https://github.com/your-github-profile")
        }
    });
});

var app = builder.Build();
app.UseCors("AllowAllOrigins");

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    // Enable developer-friendly exception page
    app.UseDeveloperExceptionPage();

    // Enable Swagger in development mode
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Minerva API V1");
        c.RoutePrefix = string.Empty; // Makes Swagger UI available at the root URL
    });
}

// Middleware for serving static files (e.g., images, CSS, JavaScript)
app.UseStaticFiles();

// Add routing middleware
app.UseRouting();

// Add endpoints for API controllers
app.MapControllers();

// Start the application
app.Run();
