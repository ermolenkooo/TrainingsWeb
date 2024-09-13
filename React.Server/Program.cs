using BLL;
using Microsoft.AspNetCore.Http;
using React.Server;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using MagicOnion;

var builder = WebApplication.CreateBuilder(args);

//подключаем сервисы SignalR
//builder.Services.AddSignalR();
builder.Services.AddSignalR(options =>
{
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30); // Таймаут для клиента
    options.KeepAliveInterval = TimeSpan.FromSeconds(15); // Интервал KeepAlive
    options.EnableDetailedErrors = true; // Включить подробные ошибки, чтобы увидеть, что происходит
});

//builder.Services.AddCors(options =>
//{
//    options.AddDefaultPolicy(
//        policy =>
//        {
//            //policy.WithOrigins("https://localhost:5173")
//            policy.SetIsOriginAllowed(_ => true)
//            .AllowAnyHeader()
//            .AllowAnyMethod();
//        });
//});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddTransient<MyOptions>();
//builder.Services.AddTransient<Task<MyOptions>>(async serviceProvider =>
//{
//    var myOptions = new MyOptions();
//    await myOptions.InitializeAsync();
//    return myOptions;
//});
builder.Services.AddSingleton<MessageManager>();
//builder.Services.AddSingleton<React.Server.WebSocketManager>();

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

//MessageHub будет обрабатывать запросы по пути /message
//app.MapHub<MessageHub>("/api/message");
app.MapHub<MessageHub>("/hub");

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
