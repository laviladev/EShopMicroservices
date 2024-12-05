using Carter;
using Catalog.API.Data.PostgreSQL;
using FluentValidation;
using BuildingBlocks.Behaviors;

var builder = WebApplication.CreateBuilder(args);

var Assembly = typeof(Program).Assembly;

builder.Services.AddMediatR(config => {
  config.RegisterServicesFromAssembly(Assembly);
  config.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

builder.Services.AddCarter();
builder.Services.AddValidatorsFromAssembly(Assembly);
builder.Services.AddSingleton<INpgsqlConnectionProvider, NpgsqlConnectionProvider>();
builder.Services.AddSingleton<DataBaseCommands, DataBaseCommands>();

var app = builder.Build();

app.MapCarter();

app.Run();
