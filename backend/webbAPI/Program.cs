using webbAPI.DataService;
using webbAPI.Hubs;
using webbAPI.Repositories;
using webbAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHostedService<ShutdownService>();

builder.Services.AddSignalR();
builder.Services.AddSingleton<SharedDB>();

builder.Services.AddSingleton<GameRepository>();
builder.Services.AddSingleton<GameRoundRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<GameService>();

builder.Services.AddHostedService<GameBackgroundService>();

builder.Services.AddCors(opt => {
    opt.AddPolicy("react-app", builder => {
        builder.WithOrigins("http://localhost:5173")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseCors("react-app");

app.MapHub<DrawHub>("/draw");

app.Run();
