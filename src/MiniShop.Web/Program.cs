using System.Globalization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using MiniShop.Application.Interfaces;
using MiniShop.Application.Services;
using MiniShop.Domain.Interfaces;
using MiniShop.Infrastructure.Persistence;
using MiniShop.Infrastructure.Repositories;
using MiniShop.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddControllersWithViews().AddViewLocalization();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
    });

builder.Services.AddAuthorization(options =>
    options.AddPolicy("AdminOnly", policy => policy.RequireClaim("is_admin", "true")));

builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();

// Clean Architecture wiring: Web -> Application services -> Domain interfaces -> Infrastructure.
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IShopRepository, ShopRepository>();
builder.Services.AddScoped<ShopService>();
builder.Services.AddScoped<ShippingService>();
builder.Services.AddScoped<AssistantService>();
builder.Services.AddScoped<CatalogService>();
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<ICartStore, SessionCartStore>();
builder.Services.AddScoped<CartService>();

var app = builder.Build();

// Create the SQLite database on first run and seed sample data.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    SeedData.Initialize(db, builder.Configuration["ADMIN_SEED_PASSWORD"]);
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

var supportedCultures = new[] { new CultureInfo("en"), new CultureInfo("ar") };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Products}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
