using RetailCoreEcommerce.API;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration) // load Serilog settings
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog(Log.Logger); // replace default logging with Serilog

builder.Configure();
var app = builder.Build();
app.Configure();
app.Run();