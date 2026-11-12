using CarInsurance.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);
// Register MVC and an EF Core SQLite database stored locally in App_Data.
builder.Services.AddControllersWithViews();
var dataDirectory = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
Directory.CreateDirectory(dataDirectory);
// Keep local antiforgery keys with the demo's other non-source runtime files.
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(dataDirectory, "Keys")));
builder.Services.AddDbContext<InsuranceContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("InsuranceContext")
        ?? $"Data Source={Path.Combine(dataDirectory, "CarInsurance.db")}"));

var app = builder.Build();
// Create the schema automatically for this standalone coursework project.
using (var scope = app.Services.CreateScope())
    scope.ServiceProvider.GetRequiredService<InsuranceContext>().Database.EnsureCreated();

app.UseStaticFiles();
app.UseRouting();
app.MapControllerRoute("default", "{controller=Insuree}/{action=Index}/{id?}");
app.Run();
