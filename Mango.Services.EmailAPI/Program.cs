
using Mango.Services.EmailAPI.Data;
using Mango.Services.EmailAPI.Extension;
using Mango.Services.EmailAPI.Messaging;
using Mango.Services.EmailAPI.Services;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(option =>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

//look : vid 115
var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
optionsBuilder.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
builder.Services.AddSingleton(new EmailService(optionsBuilder.Options));

builder.Services.AddHostedService<RabbitMQAuthConsumer>();
builder.Services.AddHostedService<RabbitMQCartConsumer>();

builder.Services.AddSingleton<IAzureServiceBusConsumer, AzureServiceBusConsumer>();//look
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
ApplyMigration();
app.UserAzureServiceBusConsumer();
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
