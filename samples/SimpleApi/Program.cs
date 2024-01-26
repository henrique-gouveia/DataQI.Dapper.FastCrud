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

// DataQI - If you are using version 1.4.x ou lower, you can do that...
builder.Services.AddScoped<DapperRepositoryFactory>();

builder.Services.AddScoped(sp =>
{
    var dbConnection = sp.GetRequiredService<IDbConnection>();
    var repositoryFactory = sp.GetRequiredService<DapperRepositoryFactory>();
    return repositoryFactory.GetRepository<IEntityRepository>(dbConnection);
});

// DataQI - If you are using version 1.5.x or higher, you can do that...

// It'll add IDapperRepository<Entity> repository service...
// builder.Services.AddDefaultDapperRepository<Entity, IDbConnection>();

// It'll add IEntityRepository repository service...
// builder.Services.AddDapperRepository<IEntityRepository, IDbConnection>();

// It'll add IEntityRepository repository service supported by EntityRepository ...
// builder.Services.AddDapperRepository<IEntityRepository, EntityRepository, IDbConnection>();

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

