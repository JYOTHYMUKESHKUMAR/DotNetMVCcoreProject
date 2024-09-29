using AutoMapper;
using DotNetCoreMVCApp.Models;
using DotNetCoreMVCApp.Models.Entities;
using DotNetCoreMVCApp.Models.Repository;
using DotNetCoreMVCApp.Models.Web;
using DotNetCoreMVCApp.Repository.Implementation;
using DotNetCoreMVCApp.Service.Abstraction;
using DotNetCoreMVCApp.Service.Implementation;
using log4net;
using log4net.Config;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Reflection;


OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.Commercial;

var builder = WebApplication.CreateBuilder(args);

//Add log4net as logging provider
System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
var _logger = LogManager.GetLogger(typeof(Program));

_logger.Info("Logger registered");

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
_logger.Info($"connectionString: {connectionString}");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString), ServiceLifetime.Transient);

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
    options.Lockout.AllowedForNewUsers = false;
}).AddRoles<ApplicationRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews(options => options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true).AddRazorRuntimeCompilation();

//Services - DI
builder.Services.AddTransient<ApplicationSeeder>();
builder.Services.AddTransient<UnitOfWork>();
builder.Services.AddTransient<ICustomerService, CustomerService>();

//Automapper
var config = new MapperConfiguration(cfg =>
{
    cfg.CreateMap<Customer, CustomerViewModel>().ReverseMap();
});
IMapper mapper = config.CreateMapper();
builder.Services.AddTransient<IMapper>(c => mapper);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

//using (var scope = app.Services.CreateScope())
//{
//    var seeder = scope.ServiceProvider.GetRequiredService<ApplicationSeeder>();
//    seeder?.Seed().Wait();
//}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");
app.MapRazorPages();

app.Run();
