DataQI Dapper.FastCrud 
DataQI.Dapper.FastCrud is a Dapper.FastCrud provider that use infrastructure provided by DataQI.Commons built around essential features of the C# 6 / VB that to make it easy to implement repositories.
Getting Started
DataQI Dapper.FastCrud is a library heavily inspired by Pivotal's Spring Data JPA library, and it turns your Data Repositories a live interface, making possible a custom Query Methods definitions:
public interface IPersonRepository : IDapperRepository<Person>
{
    IEnumerable<Person> FindByFullName(string fullName);
  
    IEnumerable<Person> FindByEmailLike(string email);
  
    IEnumerable<Person> FindByDateRegisterBetween(DateTime startDate, DateTime endDate);
}
The DapperRepositoryFactory class extends RepositoryFactory to define abstract method GetCustomImplementation to generate an implementation of IPersonRepository that uses a Data Base Connection to make its calls:
var repositoryFactory = new DapperRepositoryFactory(Connection);
personRepository = repositoryFactory.GetRepository<IPersonRepository>();
Visit Use Default Methods, Using Criteria Definitions and Using Query Methods for examples to use personRepository.
Limitations and caveats
DataQI is in the experimental phase. It does attempt to solve some problems but is expected that some improvements turn possible to arrive at a stable version.
License
DataQI Dapper.FastCrud is released under the MIT License.
