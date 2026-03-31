using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ResearchHub.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ResearchHub.Data.ResearchHubContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentity<ResearchHub.Models.ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;

    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.AllowedForNewUsers = true;
})
    .AddEntityFrameworkStores<ResearchHub.Data.ResearchHubContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets().AllowAnonymous();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}")
    .WithStaticAssets();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    await EnsureRolesAndAdminAsync(roleManager, userManager);
}

app.Run();

static async Task EnsureRolesAndAdminAsync(
    RoleManager<IdentityRole> roleManager,
    UserManager<ApplicationUser> userManager)
{
    if (!await roleManager.RoleExistsAsync(Roles.Administrador))
    {
        await roleManager.CreateAsync(new IdentityRole(Roles.Administrador));
    }

    if (!await roleManager.RoleExistsAsync(Roles.Usuario))
    {
        await roleManager.CreateAsync(new IdentityRole(Roles.Usuario));
    }

    const string adminEmail = "admin@researchhub.local";
    const string adminPassword = "Admin123";

    var admin = await userManager.FindByEmailAsync(adminEmail);
    if (admin == null)
    {
        admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(admin, adminPassword);
        if (!createResult.Succeeded)
        {
            return;
        }
    }

    if (!await userManager.IsInRoleAsync(admin, Roles.Administrador))
    {
        await userManager.AddToRoleAsync(admin, Roles.Administrador);
    }
}


