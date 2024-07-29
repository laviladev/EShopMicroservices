using Carter;
using Catalog.API.Data.PostgreSQL;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCarter();
builder.Services.AddMediatR(config => {
    config.RegisterServicesFromAssembly(typeof(Program).Assembly);
});
builder.Services.AddSingleton<INpgsqlConnectionProvider, NpgsqlConnectionProvider>();
builder.Services.AddSingleton<DataBaseCommands, DataBaseCommands>();

var app = builder.Build();

app.MapCarter();

app.Run();
