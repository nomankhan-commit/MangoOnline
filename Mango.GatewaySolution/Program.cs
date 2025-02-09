using Mango.GatewaySolution.Extension;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Values;


var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("ocelot.json", optional:false,reloadOnChange:true);
builder.AddAppAuthentication();
builder.Services.AddOcelot(builder.Configuration);
var app = builder.Build();
app.MapGet("/", () => "Hello World!");
await app.UseOcelot();
app.Run();
