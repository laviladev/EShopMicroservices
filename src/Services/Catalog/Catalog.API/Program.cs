using Carter;
using Catalog.API.Data.PostgreSQL;
using FluentValidation;
using BuildingBlocks.Behaviors;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCarter();
builder.Services.AddMediatR(config => {
    config.RegisterServicesFromAssembly(typeof(Program).Assembly);
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
});
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
builder.Services.AddSingleton<INpgsqlConnectionProvider, NpgsqlConnectionProvider>();
builder.Services.AddSingleton<DataBaseCommands, DataBaseCommands>();

var app = builder.Build();

app.MapCarter();

app.Run();
