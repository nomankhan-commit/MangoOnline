using AutoMapper;
using Mango.MessageBus;
using Mango.Services.OrderAPI;
using Mango.Services.OrderAPI.Data;
using Mango.Services.OrderAPI.RabbitMQSender;
using Mango.Services.OrderAPI.Service;
using Mango.Services.OrderAPI.Service.IService;
using Mango.Services.OrderAPI.Utility;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(option =>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

IMapper mapper = MappingConfig.RegisterMaps().CreateMapper();
builder.Services.AddSingleton(mapper);
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<BackEndApiAuthenticationHttpClientHandler>();
//builder.Services.AddScoped<IMessageBus, MessageBus>();
builder.Services.AddScoped<IRabbitMQOrderMessageSender, RabbitMQOrderMessageSender>();

builder.Services.AddHttpClient("Product", u => u.BaseAddress =
new Uri(builder.Configuration["ServiceUrls:ProductAPIBase"]))
    .AddHttpMessageHandler<BackEndApiAuthenticationHttpClientHandler>();

builder.Services.AddHttpClient("Coupon", u => u.BaseAddress =
new Uri(builder.Configuration["ServiceUrls:CouponAPIBase"]))
    .AddHttpMessageHandler<BackEndApiAuthenticationHttpClientHandler>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(op =>
{
    op.AddSecurityDefinition(name: "Bearer", securityScheme: new OpenApiSecurityScheme
    {

        Name = "Authorization",
        Description = "Bearer Token",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"

    });
    op.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
             new OpenApiSecurityScheme
             {
                Reference  =  new OpenApiReference
                 {
                     Type=ReferenceType.SecurityScheme,
                     Id = JwtBearerDefaults.AuthenticationScheme
                 }
             },new string[]{ }
         }

    });
});

var secret = builder.Configuration.GetValue<string>("ApiSettings:Secret");
var issuer = builder.Configuration.GetValue<string>("ApiSettings:Issuer");
var audience = builder.Configuration.GetValue<string>("ApiSettings:Audience");
var key = Encoding.ASCII.GetBytes(secret);
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        ValidateAudience = true,
    };
});
builder.Services.AddAuthorization();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}
Stripe.StripeConfiguration.ApiKey = "sk_test_51QldtIGb2C4QD1yW3lZ37OYo41WwzhCMYxAhObBncAPgH7yX9vlWXGvqE3VIg7NmHKmskTzl4wAeoldGmAKqon5Y00835fsGr6";
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
ApplyMigration();
app.Run();


void ApplyMigration()
{

    using (var scope = app.Services.CreateScope())
    {
        var _db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if (_db.Database.GetPendingMigrations().Count() > 0)
        {
            _db.Database.Migrate();
        }
    }

}
