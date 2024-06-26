using BLL;
using Microsoft.AspNetCore.Http;
using React.Server;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            //policy.WithOrigins("https://localhost:5173")
            policy.SetIsOriginAllowed(_ => true)
            .AllowAnyHeader()
            .AllowAnyMethod();
        });
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddTransient<MyOptions>();
builder.Services.AddSingleton<React.Server.WebSocketManager>();
//builder.Services.AddSingleton<MyOptions>();
//builder.Services.AddSingleton<MyOptionsFactory>();
//builder.Services.AddSingleton(provider =>
//{
//    var factory = provider.GetRequiredService<MyOptionsFactory>();
//    return factory.Create();
//});

var app = builder.Build();

app.UseWebSockets();

var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(5)
};

app.UseWebSockets(webSocketOptions);

app.UseCors();

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

await app.RunAsync();
