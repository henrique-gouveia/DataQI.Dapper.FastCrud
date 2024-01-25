# DataQI Dapper.FastCrud

Data Query Interface Provider for [Dapper.FastCrud](https://github.com/MoonStorm/Dapper.FastCRUD) written in C# and built around essential features of the .NET Standard that uses infrastructure provided by [DataQI.Commons](https://github.com/henrique-gouveia/DataQI.Commons) and it turns your Data Repositories a live interface. Its purpose is to facilitate the construction of data access layers making possible the definition repository interfaces, providing behaviors for standard operations as well to defines customized queries through method signatures.

[![Build](https://github.com/henrique-gouveia/DataQI.Dapper.FastCrud/actions/workflows/dotnet.yml/badge.svg)](https://github.com/henrique-gouveia/DataQI.Dapper.FastCrud/actions/workflows/dotnet.yml)
[![codecov](https://codecov.io/gh/henrique-gouveia/DataQI.Dapper.FastCrud/branch/main/graph/badge.svg)](https://codecov.io/gh/henrique-gouveia/DataQI.Dapper.FastCrud)
[![NuGet](https://img.shields.io/nuget/v/DataQI.Dapper.FastCrud.svg)](https://www.nuget.org/packages/DataQI.Dapper.FastCrud/)
[![License](https://img.shields.io/github/license/henrique-gouveia/DataQI.Dapper.FastCrud.svg)](https://github.com/henrique-gouveia/DataQI.Dapper.FastCrud/blob/main/LICENSE.txt)

## Getting Started

### Installing

This library can add in to the project by way:

    dotnet add package DataQI.Dapper.FastCrud

See [Nuget](https://www.nuget.org/packages/DataQI.Dapper.FastCrud) for other options.

### Defining a Repository

A Repository Interface should extends the interface `IDapperRepository<TEntity>` localized in the namespace `DataQI.Dapper.FastCrud.Repository`, where the `TEntity` is a _Plain Old CSharp Object (POCO)_.

```csharp
[Table("Person")]
public class Person
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("person_id")]
    public int Id { get; set; }
    [Column("first_name")]
    public string FirstName { get; set; }
    [Column("last_name")]
    public string LastName { get; set; }
    [Column("birth_date")]
    public DateTime BirthDate { get; set; }
    [Column("register_date")]
    public DateTime RegisterDate { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public boolean Active { get; set; }
}

public interface IPersonRepository : IDapperRepository<Person>
{

}
```

### Instancing a Repository

Should to use a instance of the `DapperRepositoryFactory` class to instantiate a Repository, localizated in the namespace `DataQI.Dapper.FastCrud.Repository.Support`, that requires a `IDbConnection` to make its calls:

```csharp
IDbConnection connection = CreateConnection();
var repositoryFactory = new DapperRepositoryFactory();

personRepository = repositoryFactory.GetRepository<IPersonRepository>(connection);
```

### Configuring by using Microsoft's Dependency Injection

After you have installed this lib, if you're using some type of .NET Web Application, you can do the following:

```csharp
var builder = WebApplication.CreateBuilder(args);
```

Don't forget to configure your database connection:

```csharp
// Configure the Database Connection like you prefer...
builder.Services.AddSingleton<IDbConnection>(...);
```

If you don't need a specific repository interface, you can just register a service by doing the following:

```csharp
// It'll add IDapperRepository<Entity> repository service...
builder.Services.AddDefaultDapperRepository<Person, IDbConnection>();
```

If you need a specific repository interface, you can register it by doing the following:

```csharp
// It'll add IPersonRepository repository service...
builder.Services.AddDapperRepository<IPersonRepository, IDbConnection>();
```

Now, if you need to add custom support using a repository implementation, you can register it by doing the following:

```csharp
// It'll add IPersonRepository repository service supported by SomeCustomRepository ...
builder.Services.AddDapperRepository<IPersonRepository, SomeCustomRepository, IDbConnection>();
```

Take a look at the [Samples](https://github.com/henrique-gouveia/DataQI.Dapper.FastCrud/tree/main/samples) to see how this works.

### Using Default Methods

A Repository Interface that extends `IDapperRepository<TEntity>` inherit its standard operations:

```csharp
personRepository.Insert(person);
await personRepository.InsertAsync(person);

personRepository.Save(person);
await personRepository.SaveAsync(person);

personRepository.Delete(person);
await personRepository.DeleteAsync(person);

var exists = personRepository.Exists(person);
exists = await personRepository.ExistsAsync(person);

var allPersons = personRepository.FindAll();
allPersons = await personRepository.FindAllAsync();

var onePerson = personRepository.FindOne(new Person{ Id = 1 });
onePerson = await personRepository.FindOneAsync(new Person{ Id = 1 });
```

### Using Criteria Definitions

Customized Queries can be specified by a simple Criteria Query API where are the main artifacts is localized in the namespace `DataQI.Common.Query` and `DataQI.Common.Query.Support`.

```csharp
var personsByCriteria = personRepository.Find(criteria =>
    criteria
        .Add(Restrictions.Like("FirstName", "Name%"))
        .Add(Restrictions
            .Disjuction()
            .Add(Restrictions.Between("BirthDate", new DateTime(2015, 1, 1), new DateTime(2020, 1, 1)))
            .Add(Restrictions.Equal("Active", true)))
    );

var personsByCriteriaAsync = await personRepository.FindAsync(criteria =>
    criteria
        .Add(Restrictions.Like("LastName", "%Name%"))
        .Add(Restrictions
            .Disjuction()
            .Add(Restrictions.Between("BirthDate", new DateTime(2015, 1, 1), new DateTime(2020, 1, 1)))
            .Add(Restrictions.GreaterThan("RegisterDate", new DateTime(2019, 1, 1))))
    );
```

### Using Query Methods

Customized Queries can be defined through method signatures with the following conventions:

- The method name can be initiated with the prefix `FindBy`.
- Next step, should be indicated the field that will be want to apply a operator.
- After the field name, should be indicated the operator (column `Operador` from the table below). The `Equal` is assumed how default operator if nothing it's indicate.
- Finaly, each sentence composition can be combined with anothers through of the `Conjunctions` _AND_ and `Disjunction` _OR_.

#### Supported keywords inside method names

| **Keyword** | **Sample** | **Fragment**
|-------------|------------|-------------
| **Equal** | FindByName, FindByName**Equal** | where Name **=** @0
| **NotEqual** | FindByName**Not**, FindByName**NotEqual** | where Name **<>** @0
| **Between** | FindByAge**Between** | where Age **between** @0 **and** @1
| **NotBetween** | FindByAge**NotBetween** | where Age **not between** @0 **and** @1
| **GreaterThan** | FindByBirthDate**GreaterThan** | where BirthDate **>** @0
| **GreaterThanEqual** | FindByBirthDate**GreaterThanEqual** | where BirthDate **>=** @0
| **LessThan** | FindByBirthDate**LessThan** | where BirthDate **<** @0
| **LessThanEqual** | FindByBirthDate**LessThanEqual** | where BirthDate **<=** @0
| **In** | FindByAddressType**In** | where AddressType **in** (@0)
| **NotIn** | FindByAddressType**NotIn** | where AddressType **not in** (@0)
| **Null** | FindByEmail**Null** | where Email **is null**
| **NotNull** | FindByEmail**NotNull** | where Email **is not null**
| **StartingWith** | FindByName**StartingWith** | where Name **like** @0
| **NotStartingWith** | FindByName**NotStartingWith** | where Name **not like** @0
| **EndingWith** | FindByName**EndingWith** | where Name **like** @0
| **NotEndingWith** | FindByName**NotEndingWith** | where Name **not like** @0
| **Containing** | FindByName**Containing** | where Name **like** @0
| **NotContaining** | FindByName**NotContaining** | where Name **not like** @0
| **Like** | FindByName**Like** | where Name **like** @0
| **NotLike** | FindByName**NotLike** | where Name **not like** @0
| **And** | FindByName**And**Email | where (Name = @0 **and** Email = @1)
| **Or** | FindByName**Or**Email | where (Name = @0 **or** Email = @1)

#### Sample

```csharp
public interface IPersonRepository : IDapperRepository<Person>
{
    IEnumerable<Person> FindByLastName(string name);
    IEnumerable<Person> FindByBirthDateBetween(DateTime startDate, DateTime endDate);
    IEnumerable<Person> FindByFirstNameLikeAndActive(string name, bool active = true);
    IEnumerable<Person> FindByEmailLikeOrPhoneNotNull(string email);
    IEnumerable<Person> FindByFirstNameAndLastNameOrBirthDateGreaterThan(string firstName, string lastName, DateTime registerDate);
}

IDbConnection connection = CreateConnection();
var factory = new DapperRepositoryFactory();

var personRepository = factory.GetRepository<IPersonRepository>(connection);

var persons = personRepository.FindByLastName("A Last Name");
persons = personRepository.FindByBirthDateBetween(new DateTime(2015, 1, 1), new DateTime(2020, 1, 1));
persons = personRepository.FindByFirstNameLikeAndActive(string name, bool active = true);
persons = personRepository.FindByEmailLikeOrPhoneNotNull(string email);
persons = personRepository.FindFindByFirstNameAndLastNameOrBirthDateGreaterThan("A First Name", "A Last Name", new DateTime(2019, 1, 1));
```

### Using Customized Methods

Customized Methods can be defined as normal class:

- The method should be defined in the repository interface.
- The class with implementation may or may not implement the interface.
- This can be used for any kind of customization that you want.

```csharp
public interface IPersonRepository : IDapperRepository<Person>
{
    // Query Methods
    IEnumerable<Person> FindByEmailNotNull();
    IEnumerable<Person> FindByFirstNameStartingWith(string firstName);

    // Customized Methods
    void AddAll(IEnumerable<Person> persons);
}

public class PersonRepository : DapperRepository<Person>
{
    public void AddAll(IEnumerable<Person> persons)
    {
        foreach (var person in persons)
        {
            this.Insert(person)
        }
    }
}

// Getting Repository
IDbConnection connection = CreateConnection();
var factory = new DapperRepositoryFactory();

var personRepository = factory.GetRepository<IPersonRepository>(() => new PersonRepository(connection));

// Using Query Methods
var persons = personRepository.FindByEmailNotNull();
persons = personRepository.FindByFirstNameStartingWith("Name");

// Using Customized Methods
person1 = new Person() {...};
person2 = new Person() {...};
person3 = new Person() {...};

personRepository.AddAll(new List<Person> { person1, person2, person3 });
```

## Performance

The following metrics show how long it takes to execute top CRUD statements on a SQLServer database, comparing the provider to dry FastCrud.

The benchmarks can be found at [DataQI.Dapper.FastCrud.Benchmarks](https://github.com/henrique-gouveia/DataQI.Dapper.FastCrud/tree/main/test/DataQI.Dapper.FastCrud.Benchmarks) (contributions welcome!) and can be run via:

```bash
dotnet run --project .\test\DataQI.Dapper.FastCrud.Benchmarks\DataQI.Dapper.FastCrud.Benchmarks.csproj  -f net6.0 -c Release
```

```

BenchmarkDotNet v0.13.12, Windows 10 (10.0.19045.3930/22H2/2022Update)
Intel Core i7-8565U CPU 1.80GHz (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK 7.0.405
  [Host] : .NET 6.0.3 (6.0.322.12309), X64 RyuJIT AVX2
  Dry    : .NET 6.0.3 (6.0.322.12309), X64 RyuJIT AVX2

```
| Lib           | Method               | Note                     | Op Count | Mean       | StdDev    | Error     | Gen0     | Gen1     | Gen2    | Allocated  | 
|---------------|----------------------|--------------------------|---------:|-----------:|----------:|----------:|---------:|---------:|--------:|-----------:|
| Pure FastCrud | FindAll&lt;T&gt;     | Select ~10,000 rows / op |   10,000 | 20.1429 ms | 1.3392 ms | 1.1629 ms | 448.0000 | 202.0000 | 76.0000 | 2474.94 KB |
| DataQI        | FindAll&lt;T&gt;     | Select ~10,000 rows / op |   10,000 | 20.1434 ms | 1.3489 ms | 1.1713 ms | 444.0000 | 194.0000 | 72.0000 | 2475.01 KB |
| Pure FastCrud | FindOne&lt;T&gt;     | Select 1 row / op        |   10,000 |  1.0239 ms | 0.1277 ms | 0.1109 ms |        - |        - |       - |    4.61 KB |
| DataQI        | FindOne&lt;T&gt;     | Select 1 row / op        |   10,000 |  1.0447 ms | 0.1008 ms | 0.0875 ms |        - |        - |       - |    4.64 KB |
| Pure FastCrud | CustomQuery&lt;T&gt; | Select 1 row / op        |   10,000 | 10.4004 ms | 0.0927 ms | 0.0805 ms |   2.0000 |        - |       - |    8.32 KB |
| DataQI        | CustomQuery&lt;T&gt; | Select 1 row / op        |   10,000 | 10.6461 ms | 0.3675 ms | 0.3191 ms |   2.0000 |        - |       - |   12.11 KB | 
| Pure FastCrud | Insert&lt;T&gt;      | Insert 1 row / op        |   10,000 |  6.3114 ms | 0.0862 ms | 0.0748 ms |        - |        - |       - |    3.94 KB |
| DataQI        | Insert&lt;T&gt;      | Insert 1 row / op        |   10,000 |  6.6799 ms | 3.9383 ms | 3.4199 ms |        - |        - |       - |    3.97 KB |
| Pure FastCrud | Update&lt;T&gt;      | Update 1 row / op        |   10,000 |  6.7831 ms | 3.0867 ms | 2.6803 ms |        - |        - |       - |    3.08 KB |
| DataQI        | Update&lt;T&gt;      | Update 1 row / op        |   10,000 |  7.1635 ms | 0.1143 ms | 0.0993 ms |        - |        - |       - |    7.01 KB |
| Pure FastCrud | Delete&lt;T&gt;      | Delete 1 row / op        |   10,000 |  6.5956 ms | 0.0741 ms | 0.0643 ms |        - |        - |       - |    2.17 KB |
| DataQI        | Delete&lt;T&gt;      | Delete 1 row / op        |   10,000 |  7.0347 ms | 4.3003 ms | 3.7342 ms |        - |        - |       - |    2.21 KB |

## Limitations and caveats

The DataQI FastCrud Provider library is not an ORM or it attempts to solve all data persistence problems. It provides a structure based on Repository Pattern that facilitates the rapid creation of repositories with methods that allow the creation, modification and deletion of data, as well as the preparation of simple queries by signing the methods declared in an interface, in order to avoid most of the effort involved in writing standard code in projects that use the [Dapper.FastCrud](https://github.com/MoonStorm/FastCrud) library.

## Release notes

**v1.5.0 - 2023/01**

- New! Added project to perform benchmarks
- New! Added support to register repositories for Microsoft's Dependency Injection
- New! Added sample project

**v1.4.0 - 2022/06**

- Change! Updated minimum version of supported packages

**v1.3.0 - 2022/01**

- Change! Added support to the new `DataQI.Commons` features

**v1.2.0 - 2022/01**

- New! Added support to the new `RepositoryFactory` features
- New! Added capability to invokes non-standard methods defined on client
- Change! `TEntity` requirements on generic interface `IDapperRepository`
- **Breaking Change!** Removed `DbConnection` as argument on `DapperRepositoryFactory` constructor

**v1.1.0 - 2020/09**

- New! Added support to new Criteria Query API
- New! Added Criteria Parser

**v1.0.0 - 2020/03**

- Provided initial core base

## License

DataQI Dapper.FastCrud is released under the [MIT License](https://opensource.org/licenses/MIT).
