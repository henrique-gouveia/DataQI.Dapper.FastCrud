using DataQI.Dapper.FastCrud.Repository;
using DataQI.Dapper.FastCrud.Repository.Support;

namespace DataQI.Dapper.FastCrud.Benchmarks;

[Description("DataQI")]
public class DataQiBenchmark : BenchmarkBase
{
    private IEntityRepository entityRepository = null!;
    
    [GlobalSetup]
    public void Setup() 
    {
        entityRepository = new DapperRepositoryFactory().GetRepository<IEntityRepository>(Connection);
        InsertInitialEntitiesUsingAdoNet();
    }

    [GlobalCleanup]
    public void Cleanup() => DeleteAllEntitiesUsingAdoNet();
    
    [Benchmark(Description = "FindAll<T>")]
    public IList<Entity> FindAll()
    {
        PrepareStep();
        
        var entities = entityRepository.FindAll().ToList();
        entities.Should().HaveCount(InsertedEntities.Count);
        return entities;
    }
    
    [Benchmark(Description = "FindOne<T>")]
    public Entity? FindOne()
    {
        PrepareStep();

        var entityToFind = CurrentStepEntity;
        var entity = entityRepository.FindOne(entityToFind);
        entity.Should().NotBe(null);
        return entity;
    }

    [Benchmark(Description = "CustomQuery<T>")]
    public IList<Entity> CustomQuery()
    {
        PrepareStep();
        
        var entityToQuery = CurrentStepEntity;
        var entities = entityRepository.FindByName(entityToQuery.Name!).ToList();
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
        entityRepository.Insert(entity);
        return entity;
    }
    
    [Benchmark(Description = "Update<T>")]
    public Entity Update()
    {
        PrepareStep();
        
        var entityToUpdate = CurrentStepEntity;
        entityToUpdate.Name = $"{entityToUpdate.Name} - Updated";
        entityRepository.Save(entityToUpdate);
        return entityToUpdate;
    }
    
    [Benchmark(Description = "Delete<T>")]
    public Entity Delete()
    {
        PrepareStep();
        
        var entityToDelete = CurrentStepEntity;
        entityRepository.Delete(entityToDelete);
        return entityToDelete;
    }
    
    private interface IEntityRepository : IDapperRepository<Entity>
    {
        IEnumerable<Entity> FindByName(string name);
    }
}