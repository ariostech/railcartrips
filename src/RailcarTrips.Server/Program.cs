using Microsoft.EntityFrameworkCore;
using RailcarTrips.Server.Data;
using RailcarTrips.Server.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=railcartrips.db"));

builder.Services.AddScoped<TripProcessingService>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseWebAssemblyDebugging();
}

app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILoggerFactory>()
        .CreateLogger("RequestLogger");
    logger.LogInformation(">>> {Method} {Path} Content-Type: {ContentType}",
        context.Request.Method, context.Request.Path, context.Request.ContentType ?? "(none)");
    await next();
    logger.LogInformation("<<< {Method} {Path} => {StatusCode}",
        context.Request.Method, context.Request.Path, context.Response.StatusCode);
});

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.MapControllers();

app.MapFallbackToFile("index.html");

// TODO: In production, use a proper migration strategy (CLI or deployment pipeline)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();
