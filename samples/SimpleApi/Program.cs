var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext
builder.Services.AddSingleton<IDbConnection>(_ =>
{
    OrmConfiguration.DefaultDialect = SqlDialect.SqLite;
    
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    var connection = new SqliteConnection(connectionString);
    connection.EnsureDatabaseCreated();
    return connection;
});

// DataQI
builder.Services.AddScoped<DapperRepositoryFactory>();

builder.Services.AddScoped(sp =>
{
    var dbConnection = sp.GetRequiredService<IDbConnection>();
    var repositoryFactory = sp.GetRequiredService<DapperRepositoryFactory>();
    return repositoryFactory.GetRepository<IEntityRepository>(dbConnection);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Routes
app.MapRoutes();

app.Run();

