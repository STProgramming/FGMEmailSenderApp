using EmailService;
using FGMEmailSenderApp.Attribute;
using FGMEmailSenderApp.Helpers;
using FGMEmailSenderApp.Models.EntityFrameworkModels;
using FGMEmailSenderApp.Models.Interfaces;
using FGMEmailSenderApp.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//TODO AUTO VALIDATE ANTI FORGERY TOKEN 
builder.Services.AddControllersWithViews();

builder.Services.AddControllersWithViews().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.WriteIndented = true;
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.AddSwaggerGen();

#region PREVENZIONE AL CSFR

builder.Services.AddAntiforgery(options => {
    options.Cookie.Name = "X-CSRF-TOKEN-MyFGMIdentity";
    options.HeaderName = "X-CSRF-TOKEN-MyFGMIdentity";
    options.FormFieldName = "X-CSRF-TOKEN-MyFGMIdentity";
});

#endregion

#region CONFIGURAZIONE AL COOKIE DI AUTENTICAZIONE

builder.Services.Configure<CookiePolicyOptions>(options => 
{
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

builder.Services.ConfigureApplicationCookie(option =>
   {
        option.Cookie.Name = "MyFGMIdentity";
        option.Cookie.HttpOnly = true;
        option.ExpireTimeSpan = System.TimeSpan.FromDays(2);
        option.SlidingExpiration = true;
        option.LoginPath = "/Identity/User/Login";
        option.LogoutPath = "/Identity/User/Logout";
        option.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
        option.SlidingExpiration = true;
});

#endregion

#region ADD IDENTITY

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
#endregion

#region CONNESSIONE AL DATABASE SQL

var connection = builder.Services.AddDbContext<ApplicationDbContext>(options =>
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

var emailConfig = builder.Configuration.GetSection("EmailConfiguration")
    .Get<EmailConfiguration>();

builder.Services.AddSingleton(emailConfig);

var apiKey = builder.Configuration.GetSection("ApiKeyConfiguration").Get<ApiKeyConfiguration>();

builder.Services.AddSingleton(apiKey);

builder.Services.AddTransient<ILightCriptoHelper, LightCriptoHelper>();

builder.Services.AddTransient<ICompanyService, CompanyService>();

builder.Services.AddTransient<ILocationService, LocationService>();

builder.Services.AddSingleton<IEmailSender, EmailSender>();

builder.Services.AddHostedService<BackgroundTimedHostedServices>();

#endregion

#region POLICIES

///summary///
///CASO in cui partita iva non e' presente
///Il permesso di abilitare l'utente a aggiungere i dati della compagnia
builder.Services.AddAuthorization(option =>
{
    option.AddPolicy("AddDataCompany", policy => policy.RequireClaim(ClaimTypes.Role, RoleHelper.AddCompanyPermissionRole));
});

///summary///
///CASO in cui partita iva e' presente
///Il permesso di abilitare l'utente alla modifica dei dati della compagnia
builder.Services.AddAuthorization(option =>
{
    option.AddPolicy("EditDataCompanyPermission", policy => policy.RequireClaim(ClaimTypes.Role, RoleHelper.EditCompanyPermissionRole));
});

#endregion

#region API KEY FILTER GLOBAL

builder.Services.AddControllers(options => 
    options.Filters.Add<ApiKeyAttribute>());

#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions()
{
    HttpsCompression = Microsoft.AspNetCore.Http.Features.HttpsCompressionMode.Compress,
    OnPrepareResponse = (context) =>
    {
        var headers = context.Context.Response.GetTypedHeaders();
        headers.CacheControl = new Microsoft.Net.Http.Headers.CacheControlHeaderValue
        {
            Public = true,
            MaxAge = TimeSpan.FromDays(7)
        };
        headers.Expires = DateTime.UtcNow.AddDays(7);
    }
});

app.UseCookiePolicy();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html"); ;

app.Run();
