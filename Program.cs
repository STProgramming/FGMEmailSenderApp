using FGMEmailSenderApp.Helpers;
using FGMEmailSenderApp.Models.EntityFrameworkModels;
using FGMEmailSenderApp.Models.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();

builder.Services.AddControllersWithViews().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.WriteIndented = true;
});

//PER ORA INUTILE LA SPECIFICA DATO CHE NON SI USANO LE POLICY
//builder.Services.Configure<CookiePolicyOptions>(options =>
//{
//    options.CheckConsentNeeded = context => true;
//    options.MinimumSameSitePolicy = SameSiteMode.None;
//    options.HttpOnly = HttpOnlyPolicy.Always;
//});

#region PREVENZIONE AL CSFR

builder.Services.AddAntiforgery(options => {
    options.Cookie.Name = "X-CSRF-TOKEN-FGMTrasporti";
    options.HeaderName = "X-CSRF-TOKEN-FGMTrasporti";
    options.FormFieldName = "X-CSRF-TOKEN-FGMTrasporti";
});

#endregion

#region CONFIGURAZIONE AL COOKIE DI AUTENTICAZIONE

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
});

builder.Services.ConfigureApplicationCookie(option =>
{
    option.Cookie.Name = "MyFGMTrasportiIdentity";
    option.Cookie.HttpOnly = true;
    option.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    option.ExpireTimeSpan = TimeSpan.FromDays(2);
    option.SlidingExpiration = true;
    option.LoginPath = "/Identity/User/Login";
    option.LogoutPath = "/Identity/User/Logout";
    option.ReturnUrlParameter = "/";
});

#endregion

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.Lockout.DefaultLockoutTimeSpan = System.TimeSpan.FromHours(1);
    options.Lockout.AllowedForNewUsers = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.SignIn.RequireConfirmedAccount = true;
    options.SignIn.RequireConfirmedEmail = true;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

//DA ERRORE ADD IDENTITY SERVER DA STUDIARE
//builder.Services.AddIdentityServer()
//    .AddApiAuthorization<ApplicationUser, ApplicationDbContext>();

//builder.Services.AddAuthentication("MyFGMTrasportiIdentity").AddCookie("MyFGMTrasportiIdentity", option =>
//{
//    option.Cookie.Name = "MyFGMTrasportiIdentity";
//    option.Cookie.HttpOnly = true;
//    option.ExpireTimeSpan = System.TimeSpan.FromDays(2);
//    option.SlidingExpiration = true;
//    option.LoginPath = "/Identity/User/Login";
//    option.LogoutPath = "/Identity/User/Logout";
//});

builder.Services.AddControllersWithViews();

#region CONNESSIONE AL DATABASE SQL

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("MainDbConnection")
        )
);

#endregion

#region CAMBIO DEL CICLO DI VITA TOKEN DI CONFERMA ACCOUNT

builder.Services.Configure<DataProtectionTokenProviderOptions>(option =>
{
    option.TokenLifespan = TimeSpan.FromHours(1);
});

#endregion

#region INSERIMENTO SERVICES

builder.Services.AddTransient<IDataHelper, DataHelper>();

#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html"); ;

app.Run();
