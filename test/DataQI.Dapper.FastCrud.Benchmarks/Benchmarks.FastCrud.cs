using Dapper.FastCrud;

namespace DataQI.Dapper.FastCrud.Benchmarks;

[Description("Pure FastCrud")]
public class FastCrudBenchmark : BenchmarkBase
{
    [GlobalSetup]
    public void Setup() => InsertInitialEntitiesUsingAdoNet();

    [GlobalCleanup]
    public void Cleanup() => DeleteAllEntitiesUsingAdoNet();
    
    [Benchmark(Description = "FindAll<T>")]
    public IList<Entity> FindAll()
    {
        PrepareStep();
        
        var entities = Connection.Find<Entity>().ToList();
        entities.Should().HaveCount(InsertedEntities.Count);
        return entities;
    }
    
    [Benchmark(Description = "Paging<T>")]
    public IList<Entity> FindSlice()
    {
        PrepareStep();
        var rows = Connection.Find<Entity>(s => s.OrderBy($"{Sql.Column(nameof(Entity.Id))} ASC").Skip(10).Top(11)).ToList();
        if (rows.Count > 10)
            rows.RemoveAt(10);
        return rows;
    }

    [Benchmark(Description = "FindOne<T>")]
    public Entity? FindOne()
    {
        PrepareStep();
        
        var entityToQuery = CurrentStepEntity;
        var entity = Connection.Get(entityToQuery);
        entity.Should().NotBe(null);
        return entity;
    }
    
    [Benchmark(Description = "CustomQuery<T>")]
    public IList<Entity> CustomQuery()
    {
        PrepareStep();

        var entityToQuery = CurrentStepEntity;
        var entities = Connection.Find<Entity>(statement => statement
            .Where($"{nameof(entityToQuery.Name):C} = @Name")
            .WithParameters(new { entityToQuery.Name })
        ).ToList();
        entities
            .Should().HaveCount(1)
            .And.Subject.FirstOrDefault()!.Name
            .Should().BeEquivalentTo(entityToQuery.Name);
        return entities;
    }
    
    [Benchmark(Description = "Insert<T>")]
    public Entity Insert()
    {
        PrepareStep();
    
        var entity = NewEntity();
        Connection.Insert(entity);
        return entity;
    }
    
    [Benchmark(Description = "Update<T>")]
    public Entity Update()
    {
        PrepareStep();
        
        var entityToUpdate = CurrentStepEntity;
        entityToUpdate.Name = $"{entityToUpdate.Name} - Updated";
        Connection.Update(entityToUpdate);
        return entityToUpdate;
    }
    
    [Benchmark(Description = "Delete<T>")]
    public Entity Delete()
    {
        PrepareStep();
        
        var entityToDelete = CurrentStepEntity;
        Connection.Delete(entityToDelete);
        return entityToDelete;
    }
}