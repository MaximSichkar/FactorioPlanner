using FactorioPlanner.Application.Services;
using FactorioPlanner.Application.Interfaces;
using FactorioPlanner.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddSingleton<IFactorioDataService>(
    new FactorioDataService(
        Path.Combine(builder.Environment.ContentRootPath, "Data")
    )
);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ProductionCalculationService>();
builder.Services.AddScoped<ProductionBlockCalculationService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=ProductionPlanner}/{action=Index}/{id?}");




app.Run();
