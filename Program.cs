using TodoApi;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDbContext<ToDoDbContext>(options =>
options.UseMySql(builder.Configuration.GetConnectionString("ToDoDB"),
new MySqlServerVersion(new Version(8, 0, 40))));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder => builder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add IConfiguration to services
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

var app = builder.Build();

// הגדרת פורט 5117, חשוב שהשרת יאזין על הפורט הזה
app.Urls.Add("http://localhost:5117");

// Middleware נוסף לדיבוג בקשות
app.Use(async (context, next) =>
{
    Console.WriteLine($"Request: {context.Request.Method} {context.Request.Path}");
    await next();
});

// Use middleware
app.UseCors("AllowAll");
app.UseSwagger();
app.UseSwaggerUI();

// Routes
app.MapGet("/items", async (ToDoDbContext db) => await db.Items.ToListAsync());

app.MapPost("/items", async (ToDoDbContext db, Item newItem) => {
    db.Items.Add(newItem);
    await db.SaveChangesAsync();
    return Results.Created($"/items/{newItem.Id}", newItem);
});

app.MapPut("/items/{id}", async (ToDoDbContext db, int id, Item updatedItem) => {
    var item = await db.Items.FindAsync(id);
    if (item == null) return Results.NotFound();

    if (updatedItem.Name != null)
    {
        item.Name = updatedItem.Name;
    }

    item.IsComplete = updatedItem.IsComplete;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/items/{id}", async (ToDoDbContext db, int id) => {
    var item = await db.Items.FindAsync(id);
    if (item == null) return Results.NotFound();
    db.Items.Remove(item);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// Default route
//app.MapGet("/", () => "Hello World!");

// הפעלת השרת
app.Run();