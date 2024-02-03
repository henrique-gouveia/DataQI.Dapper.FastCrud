using Dapper.FastCrud;

namespace DataQI.Dapper.FastCrud.Benchmarks;

public abstract class BenchmarkBase
{
    protected BenchmarkBase()
    {
        OrmConfiguration.DefaultDialect = SqlDialect.MsSql;
    }

    private int currentStep;
    protected void PrepareStep()
    {
        currentStep++;
        if (currentStep > Consts.OperationCount) currentStep = 1;
    }
    
    protected Entity NewEntity()
        => NewEntity(currentStep);

    protected Entity NewEntity(int index)
        => new() { Name = $"This is the entity {index}" };

    protected void InsertInitialEntitiesUsingAdoNet()
    { 
        const string insertSql = "INSERT INTO entities (name, created_at) VALUES (@Name, @CreatedAt); SELECT SCOPE_IDENTITY() AS [ID]";
        using var transaction = Connection.BeginTransaction();
        for (var entityIndex = 1; entityIndex <= Consts.OperationCount; entityIndex++)
        {
            var generatedEntity = NewEntity(entityIndex);

            using var command = Connection.CreateCommand();
            command.CommandText = insertSql;
            command.Transaction = transaction;
            command.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar) { Value = generatedEntity.Name });
            command.Parameters.Add(new SqlParameter("@CreatedAt", SqlDbType.DateTime) { Value = generatedEntity.CreatedAt });
            
            generatedEntity.Id = Convert.ToInt32(command.ExecuteScalar());
            insertedEntities.Add(generatedEntity);
        }
        transaction.Commit();
    }

    protected void DeleteAllEntitiesUsingAdoNet()
    {
        const string deleteSql = "DELETE FROM entities;";
        using var transaction = Connection.BeginTransaction();
        using var command = Connection.CreateCommand();
        command.CommandText = deleteSql;
        command.Transaction = transaction;
        command.ExecuteNonQuery();
        transaction.Commit();
    }

    private readonly List<Entity> insertedEntities = new();
    protected IReadOnlyCollection<Entity> InsertedEntities => insertedEntities;
    
    protected Entity CurrentStepEntity => insertedEntities[currentStep - 1];
    
    protected IDbConnection Connection { get; } = ConnectionFactory.NewConnection();
}