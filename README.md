# DataQI Dapper.FastCrud

DataQI Dapper.FastCrud creates repository implementations from C# interfaces using [Dapper.FastCrud](https://github.com/MoonStorm/FastCrud) and [DataQI.Commons](https://github.com/henrique-gouveia/DataQI.Commons). It provides CRUD operations, synchronous and asynchronous queries, single-entity queries, ordering, and custom methods. Query method names describe the filters; their return types select collection or single-entity dispatch.

[![Build](https://github.com/henrique-gouveia/DataQI.Dapper.FastCrud/actions/workflows/dotnet.yml/badge.svg)](https://github.com/henrique-gouveia/DataQI.Dapper.FastCrud/actions/workflows/dotnet.yml)
[![codecov](https://codecov.io/gh/henrique-gouveia/DataQI.Dapper.FastCrud/branch/main/graph/badge.svg)](https://codecov.io/gh/henrique-gouveia/DataQI.Dapper.FastCrud)
[![NuGet](https://img.shields.io/nuget/v/DataQI.Dapper.FastCrud.svg)](https://www.nuget.org/packages/DataQI.Dapper.FastCrud/)
[![License](https://img.shields.io/github/license/henrique-gouveia/DataQI.Dapper.FastCrud.svg)](https://github.com/henrique-gouveia/DataQI.Dapper.FastCrud/blob/main/LICENSE)

## Getting Started

### Installing

Install the package in your application:

```bash
dotnet add package DataQI.Dapper.FastCrud
```

### Defining a Repository

Extend `IDapperRepository<TEntity>` from `DataQI.Dapper.FastCrud.Repository`. Entity mapping follows FastCrud conventions and attributes. The snippets focus on repository declarations and calls; use your own connection and application setup.

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
    public bool Active { get; set; }
}

public interface IPersonRepository : IDapperRepository<Person> { }
```

### Instancing a Repository

You must use an instance of the `DapperRepositoryFactory` class to instantiate a repository, located in the `DataQI.Dapper.FastCrud.Repository.Support` namespace, which requires an `IDbConnection` to make its calls:

```csharp
IDbConnection connection = CreateConnection();
var repositoryFactory = new DapperRepositoryFactory();

var personRepository = repositoryFactory.GetRepository<IPersonRepository>(connection);
```

Configure FastCrud's dialect before querying; for example, use `OrmConfiguration.DefaultDialect = SqlDialect.SqLite` for SQLite. Operations execute immediately, and disposing a repository disposes its connection.

### Configuring by using Microsoft's Dependency Injection

After you have installed this library, if you're using a .NET web application, you can do the following:

```csharp
var builder = WebApplication.CreateBuilder(args);
```

Don't forget to configure your database connection:

```csharp
// Configure the Database Connection like you prefer...
builder.Services.AddTransient<IDbConnection>(_ => CreateConnection());
```

A repository disposes the connection it receives, so register a connection per scope or per resolution instead of a shared singleton.

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

Repositories and their factory are registered as scoped services, so resolve them within a service scope. Take a look at the [Samples](https://github.com/henrique-gouveia/DataQI.Dapper.FastCrud/tree/main/samples) to see how this works.

### Using Default Methods

`IDapperRepository<TEntity>` extends `ICrudRepository<TEntity, TEntity>`: its identifier is an entity instance with the key properties populated. In particular, `Delete`, `Exists`, and key-based `FindOne` take an entity, such as `new Person { Id = 1 }`.

| Operation | Methods |
| --- | --- |
| Delete | `Delete`, `DeleteAsync` |
| Exists | `Exists`, `ExistsAsync` |
| Find single | `FindOne`, `FindOneAsync` (key or criteria) |
| Find many | `Find`, `FindAsync` (criteria or FastCrud statement), `FindAll`, `FindAllAsync` |
| Insert | `Insert`, `InsertAsync` |
| Insert or update | `Save`, `SaveAsync` |

```csharp
personRepository.Insert(person);
await personRepository.SaveAsync(person, cancellationToken);

var key = new Person { Id = person.Id };
var exists = personRepository.Exists(key);
exists = await personRepository.ExistsAsync(key, cancellationToken);
var onePerson = personRepository.FindOne(key);
onePerson = await personRepository.FindOneAsync(key, cancellationToken);
var allPersons = await personRepository.FindAllAsync(cancellationToken);
await personRepository.DeleteAsync(key, cancellationToken);
```

**Cancellation:** all async APIs accept a `CancellationToken`, including a final token in query method signatures. This provider currently ignores it and does not cancel the underlying FastCrud operation, even if the token is already canceled.

### Using Query Methods

The [Commons query method reference](https://github.com/henrique-gouveia/DataQI.Commons/blob/main/README.md#query-methods) defines the shared grammar: keywords and aliases, argument counts and types, `And`/`Or` precedence, reserved names, prefixes, ordering, return types, and trailing cancellation tokens. `FindBy` is the recommended prefix.

#### Supported keywords inside method names

Equality is the default operator, matching is case-sensitive, and values are consumed left to right; `And` binds more tightly than `Or`. The SQL shapes below show the statement before database-specific column quoting and Dapper's list expansion; parameters are generated as `@p0`, `@p1`, and so on, and values are bound separately. Columns resolve through FastCrud's entity mapping.

| Keyword | Other forms | Args | SQL shape |
| --- | --- | ---: | --- |
| *(none)* / `Equal` | `Equals`, `IsEqual`, `IsEquals` | 1 | `FirstName = @p0` |
| `Not` + keyword | negation marker (`NotEqual`, `NotBetween`, `NotIn`, `NotNull`, `NotLike`...) | same | `Not (FirstName = @p0)` |
| `Between` | `IsBetween` | 2 | `BirthDate Between @p0 And @p1` (inclusive) |
| `GreaterThan` / `GreaterThanEqual` | `IsGreaterThan` / `IsGreaterThanEqual` | 1 | `BirthDate > @p0` / `>=` |
| `LessThan` / `LessThanEqual` | `IsLessThan` / `IsLessThanEqual` | 1 | `BirthDate < @p0` / `<=` |
| `In` | `IsIn` | 1 | `Id In @p0` (Dapper expands the bound list) |
| `Null` | `IsNull` | 0 | `Email Is Null` |
| `StartingWith` | `StartsWith`, `IsStartingWith` | 1 | `FirstName Like @p0` |
| `EndingWith` | `EndsWith`, `IsEndingWith` | 1 | `FirstName Like @p0` |
| `Containing` | `Contains`, `IsContaining` | 1 | `FirstName Like @p0` |
| `Like` | `IsLike` | 1 | `FirstName Like @p0` |
| `And`, `Or` | | 0 | `(FirstName = @p0 AND LastName = @p1)`, `(Email = @p0 OR Phone Is Null)` |
| `OrderBy`, `Asc`, `Desc` | | 0 | ordering clause (see [Ordering Query Results](#ordering-query-results)) |

Every `Not` wraps its inner expression, including comparisons and junctions. SQL null semantics apply: negating a comparison with a non-null value does not include rows whose column is null. Add an explicit `Null` alternative if those rows should match.

**Text patterns:** the provider binds the string unchanged for all four text operators. Supply SQL wildcards yourself: `"Ada%"` for a prefix, `"%Ada"` for a suffix, and `"%Ada%"` for containment. `"Ada"` adds no wildcard, even with `StartingWith`. SQL dialect and collation control matching and escaping. This differs from the EF Core provider, where `Like` and `Containing` use literal `string.Contains` semantics and `%` is ordinary text.

**Enum values:** the visitor binds supplied values without converting them to the property's type. Prefer the actual enum value, for example `Restrictions.Equal("Status", Status.Active)`, with your database's mapping. This provider does not perform the EF Core provider's conversion of numeric values or numeric strings into enum constants; database/driver behavior decides whether other representations work. Use `object[]` for `In` criteria (for example, `new object[] { 1, 2 }`); the shared grammar documents query method array restrictions.

#### Sample

```csharp
public interface IPersonRepository : IDapperRepository<Person>
{
    IEnumerable<Person> FindByLastName(string lastName);
    IEnumerable<Person> FindByBirthDateBetween(DateTime startDate, DateTime endDate);
    IEnumerable<Person> FindByFirstNameLikeAndActive(string pattern, bool active = true);
    IEnumerable<Person> FindByEmailLikeOrPhoneNotNull(string pattern);
    IEnumerable<Person> FindByFirstNameAndLastNameOrBirthDateGreaterThan(
        string firstName, string lastName, DateTime birthDate);
    IEnumerable<Person> FindByFirstNameStartingWith(string pattern);
}
```

```csharp
var persons = personRepository.FindByLastName("Lovelace");
persons = personRepository.FindByBirthDateBetween(
    new DateTime(1800, 1, 1), new DateTime(1900, 1, 1));
persons = personRepository.FindByFirstNameLikeAndActive("%Ada%", true);
persons = personRepository.FindByEmailLikeOrPhoneNotNull("%@example.com");
persons = personRepository.FindByFirstNameAndLastNameOrBirthDateGreaterThan(
    "Ada", "Lovelace", new DateTime(1900, 1, 1));
persons = personRepository.FindByFirstNameStartingWith("Ada%");
```

### Using Async Query Methods

Return `Task<IEnumerable<Person>>` or `Task<Person>` to run a query asynchronously. The return type determines dispatch, and an optional `Async` suffix is removed before parsing. A final `CancellationToken` parameter is accepted; this provider currently ignores it.

```csharp
// Added to IPersonRepository
Task<IEnumerable<Person>> FindByLastNameAsync(
    string lastName, CancellationToken cancellationToken = default);
```

```csharp
var persons = await personRepository.FindByLastNameAsync("Lovelace", cancellationToken);
```

### Using Single-Entity Query Methods

A query method can return `Person` or `Task<Person>`. Zero matches returns `null`; multiple matches throw `InvalidOperationException`. Return `IEnumerable<Person>` for collections.

```csharp
// Added to IPersonRepository
Person FindByEmail(string email);
Task<Person> FindByEmailAsync(string email, CancellationToken cancellationToken = default);
```

```csharp
var person = personRepository.FindByEmail("ada@example.com");
person = await personRepository.FindByEmailAsync("ada@example.com", cancellationToken);
```

### Ordering Query Results

Add `OrderBy` after at least one filter. Each non-final ordering property needs `Asc` or `Desc`; the final property defaults to ascending. Ordering does not suppress the multiple-match error for single-entity queries.

```csharp
// Added to IPersonRepository
IEnumerable<Person> FindByLastNameOrderByFirstNameAscBirthDateDesc(string lastName);
Task<IEnumerable<Person>> FindByLastNameOrderByFirstNameAscBirthDateDescAsync(
    string lastName, CancellationToken cancellationToken = default);
```

```csharp
var people = personRepository.FindByLastNameOrderByFirstNameAscBirthDateDesc("Lovelace");
people = await personRepository.FindByLastNameOrderByFirstNameAscBirthDateDescAsync(
    "Lovelace", cancellationToken);
people = await personRepository.FindAsync(criteria => criteria
    .Add(Restrictions.Equal(nameof(Person.LastName), "Lovelace"))
    .AddOrder(Order.Asc(nameof(Person.FirstName)))
    .AddOrder(Order.Desc(nameof(Person.BirthDate))), cancellationToken);
```

### Query-method completion

Completion is provided by [DataQI.Analyzers](https://github.com/henrique-gouveia/DataQI.Analyzers), which comes with the Commons 4.0 dependency (see the [Commons reference](https://github.com/henrique-gouveia/DataQI.Commons/blob/main/README.md#completing-query-methods-with-dataqianalyzers)). See the DataQI.Analyzers repository for compatible IDEs and editor configuration.

Specific to this provider: text operators are suggested for `string` properties, but the SQL wildcards must still be part of the argument (see [Supported keywords inside method names](#supported-keywords-inside-method-names)). The [shared grammar](https://github.com/henrique-gouveia/DataQI.Commons/blob/main/README.md#query-methods) and the provider's translation rules determine which queries work.

### Using Statement Builder

`Find` and `FindAsync` also accept FastCrud's statement builder. Use `:C` in interpolated `FormattableString` arguments to resolve mapped column names. Use named SQL parameters such as `@FirstName` with FastCrud 2.6.7. Keep values in `WithParameters`; inserting values directly into SQL text bypasses parameter binding. If you save a SQL fragment in a variable, declare it as `FormattableString` to preserve the format specifiers.

```csharp
var parameters = new
{
    FirstName = "%Ada%",
    StartDate = new DateTime(1800, 1, 1),
    EndDate = new DateTime(1900, 1, 1),
    RegisterDate = new DateTime(2020, 1, 1)
};
FormattableString predicate = $@"
    {nameof(Person.FirstName):C} LIKE @FirstName
    AND (
        {nameof(Person.BirthDate):C} BETWEEN @StartDate AND @EndDate
        OR {nameof(Person.RegisterDate):C} > @RegisterDate
    )";

var persons = personRepository.Find(statement => statement
    .Where(predicate)
    .OrderBy($"{nameof(Person.FirstName):C}")
    .Skip(0)
    .Top(20)
    .WithParameters(parameters));
var personsAsync = await personRepository.FindAsync(statement => statement
    .Where(predicate)
    .OrderBy($"{nameof(Person.FirstName):C}")
    .Skip(20)
    .Top(20)
    .WithParameters(parameters), cancellationToken);
```

### Using Criteria Definitions

The shared criteria API is in `DataQI.Commons.Query` and `DataQI.Commons.Query.Support`. Each top-level criterion is combined with `AND`; use `Disjunction()` for an explicit `OR` group.

```csharp
var persons = personRepository.Find(criteria => criteria
    .Add(Restrictions.Like(nameof(Person.FirstName), "Ada%"))
    .Add(Restrictions.Disjunction()
        .Add(Restrictions.Between(nameof(Person.BirthDate),
            new DateTime(1800, 1, 1), new DateTime(1900, 1, 1)))
        .Add(Restrictions.Equal(nameof(Person.Active), true))));

var personsAsync = await personRepository.FindAsync(criteria => criteria
    .Add(Restrictions.Like(nameof(Person.LastName), "%Lovelace%"))
    .Add(Restrictions.Disjunction()
        .Add(Restrictions.Between(nameof(Person.BirthDate),
            new DateTime(1800, 1, 1), new DateTime(1900, 1, 1)))
        .Add(Restrictions.GreaterThan(nameof(Person.RegisterDate), new DateTime(2020, 1, 1)))),
    cancellationToken);
```

### Using Customized Methods

Declare custom methods on the repository interface and provide matching public methods in a concrete class. The proxy forwards exact method matches to that class and builds query methods from their names. The class does not need to implement the interface; inherit `DapperRepository<Person>` to reuse CRUD operations.

```csharp
public interface ICustomPersonRepository : IPersonRepository
{
    void AddAll(IEnumerable<Person> persons);
}

public class PersonRepository : DapperRepository<Person>
{
    public PersonRepository(IDbConnection connection) : base(connection) { }

    public void AddAll(IEnumerable<Person> persons)
    {
        foreach (var person in persons)
        {
            Insert(person);
        }
    }
}
```

Pass the custom implementation to the factory:

```csharp
var repositoryFactory = new DapperRepositoryFactory();
var personRepository = repositoryFactory.GetRepository<ICustomPersonRepository>(
    () => new PersonRepository(connection));
personRepository.AddAll(new[]
{
    new Person { FirstName = "Ada", LastName = "Lovelace" },
    new Person { FirstName = "Grace", LastName = "Hopper" }
});
var persons = personRepository.FindByEmailNotNull();
```

## Performance

These historical SQL Server results compare CRUD operations through DataQI with direct FastCrud calls. They predate the 4.0 AST rewrite and do not measure the current implementation.

The benchmarks can be found at [DataQI.Dapper.FastCrud.Benchmarks](https://github.com/henrique-gouveia/DataQI.Dapper.FastCrud/tree/main/test/DataQI.Dapper.FastCrud.Benchmarks) (contributions welcome!) and can be run via:

```bash
dotnet run --project .\test\DataQI.Dapper.FastCrud.Benchmarks\DataQI.Dapper.FastCrud.Benchmarks.csproj  -f net10.0 -c Release
```

The measurements below are historical results from the earlier .NET 6 setup, not measurements of the current .NET 10 build.

```

BenchmarkDotNet v0.13.12, Windows 10 (10.0.19045.3930/22H2/2022Update)
Intel Core i7-8565U CPU 1.80GHz (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK 7.0.405
  [Host] : .NET 6.0.3 (6.0.322.12309), X64 RyuJIT AVX2
  Dry    : .NET 6.0.3 (6.0.322.12309), X64 RyuJIT AVX2

```
| Lib           | Method               | Note                     | Op Count |       Mean |    StdDev |     Error |     Gen0 |     Gen1 |    Gen2 |  Allocated | 
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

This provider reduces repository boilerplate for applications already using FastCrud. Database schema, connections, transactions, dialect configuration, and SQL pattern semantics remain application concerns. Query methods support the shared Commons grammar and return shapes; use criteria, the statement builder, or a custom method for queries outside it. Cancellation tokens are currently accepted but ignored.

## Release Notes

**v4.0.0 - current source**

- **Breaking Change!** Replaced the public per-operator translation pipeline with internal visitors over the Commons 4.x criteria AST; removed the translation types listed in the migration guide.
- Change! Negation emits `Not (...)`, and invalid ordering properties fail with an `ArgumentException`.
- Change! Added XML documentation for public APIs.
- Change! tests, samples, and benchmarks target .NET 10; the library remains `netstandard2.0`.

**v3.0.0 - 2026/10**

- New! Added support for async query methods
- New! Added single-entity query methods and criteria-based `FindOne`/`FindOneAsync` methods
- New! Added ordering through criteria and the `OrderBy` suffix in query method names
- Change! Adopted the new `DataQI.Commons` APIs for async query methods, single-entity queries and ordering

**v2.0.0 - 2024/12**

- New! Added support for the `Dapper.FastCrud` statement builder
- Change! Upgraded `DataQI.Commons` to `2.0.0` to support new features

**v1.5.0 - 2023/01**

- New! Added project to perform benchmarks
- New! Added support for registering repositories with Microsoft's dependency injection container
- New! Added sample project

**v1.4.0 - 2022/06**

- Change! Updated minimum version of supported packages

**v1.3.0 - 2022/01**

- Change! Added support for the new `DataQI.Commons` features

**v1.2.0 - 2022/01**

- New! Added support for the new `RepositoryFactory` features
- New! Added the ability to invoke non-standard methods defined on a client
- Change! `TEntity` requirements on generic interface `IDapperRepository`
- **Breaking Change!** Removed `DbConnection` as argument on `DapperRepositoryFactory` constructor

**v1.1.0 - 2020/09**

- New! Added support for the new Criteria Query API
- New! Added Criteria Parser

**v1.0.0 - 2020/03**

- Provided initial core base

## License

DataQI Dapper.FastCrud is released under the [MIT License](https://github.com/henrique-gouveia/DataQI.Dapper.FastCrud/blob/main/LICENSE).
