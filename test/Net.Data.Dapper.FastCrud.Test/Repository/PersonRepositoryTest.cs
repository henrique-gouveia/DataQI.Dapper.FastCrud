using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Dapper.FastCrud;

using Xunit;
using ExpectedObjects;

using Net.Data.Dapper.FastCrud.Repository;
using Net.Data.Dapper.FastCrud.Test.Fixtures;
using Net.Data.Dapper.FastCrud.Test.Repository.Sample;

namespace Net.Data.Dapper.FastCrud.Test.Repository
{
    public class PersonRepositoryTest : IClassFixture<DbFixture>
    {
        private readonly IDbConnection connection;

        private readonly IPersonRepository personRepository;

        public PersonRepositoryTest(DbFixture fixture)
        {
            connection = fixture.Connection;
            personRepository = fixture.PersonRepository;
        }

        [Fact]
        public void TestInsert()
        {
            var countBefore = connection.Count<Person>();
            var countExpected = countBefore + 1;

            var personExpected = PersonBuilder.NewInstance().Build();
            personRepository.Insert(personExpected);

            Assert.True(personExpected.Id > 0);
            Assert.Equal(countExpected, connection.Count<Person>());
        }

        [Fact]
        public async Task TestInsertAsync()
        {
            var countBefore = await connection.CountAsync<Person>();
            var countExpected = countBefore + 1;

            var personExpected = PersonBuilder.NewInstance().Build();
            await personRepository.InsertAsync(personExpected);

            Assert.True(personExpected.Id > 0);
            Assert.Equal(countExpected, await connection.CountAsync<Person>());
        }

        [Fact]
        public void TestSave()
        {
            var countBefore = connection.Count<Person>();
            var countExpected = countBefore + 1;

            var personInserted = PersonBuilder.NewInstance().Build();
            personRepository.Save(personInserted);

            Assert.True(personInserted.Id > 0);
            Assert.Equal(countExpected, connection.Count<Person>());

            var personUpdated = PersonBuilder.NewInstance().SetId(personInserted.Id).Build();
            personRepository.Save(personUpdated);

            Assert.Equal(personInserted.Id, personUpdated.Id);
            Assert.Equal(countExpected, connection.Count<Person>());
        }

        [Fact]
        public async Task TestSaveAsync()
        {
            var countBefore = await connection.CountAsync<Person>();
            var countExpected = countBefore + 1;

            var personInserted = PersonBuilder.NewInstance().Build();
            await personRepository.SaveAsync(personInserted);

            Assert.True(personInserted.Id > 0);
            Assert.Equal(countExpected, connection.Count<Person>());

            var personUpdated = PersonBuilder.NewInstance().SetId(personInserted.Id).Build();
            await personRepository.SaveAsync(personUpdated);

            Assert.Equal(personInserted.Id, personUpdated.Id);
            Assert.Equal(countExpected, connection.Count<Person>());
        }

        [Fact]
        public void TestExists()
        {
            var personsExpected = InsertTestPersons();

            while (personsExpected.MoveNext()) 
            {
                var personExpected = personsExpected.Current;
                var personExists = personRepository.Exists(new Person() { Id = personExpected.Id });

                Assert.True(personExists);
            }
        }

        [Fact]
        public void TestExistsReturnsFalseWhenNotFoundEntity()
        {
            InsertTestPersons();
            var personExists = personRepository.Exists(new Person());
            Assert.False(personExists);
        }

        [Fact]
        public async void TestExistsAsync()
        {
            var personsExpected = InsertTestPersons();

            while (personsExpected.MoveNext()) 
            {
                var personExpected = personsExpected.Current;
                var personExists = await personRepository.ExistsAsync(new Person() { Id = personExpected.Id });
                
                Assert.True(personExists);
            }
        }

        [Fact]
        public void TestFindAll()
        {
            var personsExpected = InsertTestPersonsList();
            var persons = personRepository.FindAll();

            personsExpected.ToExpectedObject().ShouldEqual(persons);
        }

        [Fact]
        public async Task TestFindAllAsync()
        {
            var personsExpected = InsertTestPersonsList();
            var persons = await personRepository.FindAllAsync();

            personsExpected.ToExpectedObject().ShouldEqual(persons);
        }

        [Fact]
        public void TestFindOne()
        {
            var personsExpected = InsertTestPersons();

            while (personsExpected.MoveNext())
            {
                var personExpected = personsExpected.Current;
                var person = personRepository.FindOne(personExpected);

                personExpected.ToExpectedObject().ShouldEqual(person);
            }
        }

        [Fact]
        public void TestFindOneReturnsNullWhenNotFoundEntity() 
        {
            InsertTestPersons();
            var person = personRepository.FindOne(new Person());
            
            Assert.Null(person);
        }

        [Fact]
        public async void TestFindOneAsync()
        {
            var personsExpected = InsertTestPersons();

            while (personsExpected.MoveNext())
            {
                var personExpected = personsExpected.Current;
                var person = await personRepository.FindOneAsync(new Person() { Id = personExpected.Id });
                
                personExpected.ToExpectedObject().ShouldEqual(person);
            }
        }

        [Fact]
        public async void TestFindOneReturnsNullWhenNotFoundEntityAsync() 
        {
            InsertTestPersons();
            var person = await personRepository.FindOneAsync(new Person());
            
            Assert.Null(person);
        }

        [Fact]
        public void TestDelete()
        {
            var persons = InsertTestPersons();

            while (persons.MoveNext())
            {
                var person = persons.Current;
                personRepository.Delete(person);

                Assert.False(personRepository.Exists(person));
                Assert.Null(personRepository.FindOne(person));
            }
        }

        [Fact]
        public async Task TestDeleteAsync()
        {
            var persons = InsertTestPersons();

            while (persons.MoveNext())
            {
                var person = persons.Current;
                await personRepository.DeleteAsync(person);

                Assert.False(await personRepository.ExistsAsync(person));
                Assert.Null(await personRepository.FindOneAsync(person));
            }
        }        

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // 1. Quando desejo criar uma busca por outra propriedade tenho que implementar, porém cada desenvolvedor,   
        //    de acordo com sua experiência, de uma forma, exemplo:                                                 
        //
        //    1.1. Estende a classe DapperRepository, porém instancia a classe concreta;
        //
        //         public class PersonRepository : DapperRepository<Person>
        //         {
        //             public Person FindByFullName(string fullName) { ... }
        //         }
        //
        //         PersonRepository personRepository = new PersonRepository(connection);
        //         personRepository.FindByName("FIRSTNAME LASTNAME");  
        //
        //    1.2. Estende a classe DapperRepository, porém realiza um cast quando necessário utilizar o método 
        //         desejado;
        //
        //         public class PersonRepository : DapperRepository<Person>
        //         {
        //             public Person FindByFullName(string fullName) { ... }
        //         }
        //
        //         IDapperRepository<Person> personRepository = new PersonRepository(connection);
        //         ((PersonRepository)personRepository).FindByName("FIRSTNAME LASTNAME");  
        //
        //    1.3. Cria uma interface IPersonRepository e define a classe PersonRepository que a implemente
        //         
        //         public class PersonRepository : DapperRepository<Person>, IPersonRepository
        //         {
        //             public Person FindByFullName(string fullName) { ... }
        //         }
        //         
        //         IPersonRepository personRepository = new PersonRepository(connection);
        //         personRepository.FindByName("FIRSTNAME LASTNAME");  
        //    
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////

        // [Fact]
        // public void TestFindByFullName()
        // {
        //     var personsExpected = InsertTestPersons();
        //     var personRepositoryCustom = new PersonRepository(connection);

        //     while (personsExpected.MoveNext())
        //     {
        //         var personExpected = personsExpected.Current;
        //         var person = personRepositoryCustom.FindByFullName(personExpected.FullName);

        //         personExpected.ToExpectedObject().ShouldEqual(person);
        //     }
        // }
        
        [Fact]
        public void TestFindByFullName()
        {
            var personsExpected = InsertTestPersons();

            while (personsExpected.MoveNext())
            {
                var personExpected = personsExpected.Current;
                var persons = personRepository.FindByFullName(personExpected.FullName);

                personExpected.ToExpectedObject().ShouldEqual(persons.FirstOrDefault());
            }
        }

        // [Fact]
        // public void TestFindByPhone()
        // {
        //     var personsExpected = InsertTestPersons();
        //     var personRepositoryCustom = new PersonRepository(connection);

        //     while(personsExpected.MoveNext())
        //     {
        //         var personExpected = personsExpected.Current;
        //         var person = personRepositoryCustom.FindByPhone(personExpected.Phone);

        //         personExpected.ToExpectedObject().ShouldEqual(person);
        //     }
        // }

        [Fact]
        public void TestFindByPhone()
        {
            var personsExpected = InsertTestPersons();

            while(personsExpected.MoveNext())
            {
                var personExpected = personsExpected.Current;
                var persons = personRepository.FindByPhone(personExpected.Phone);

                personExpected.ToExpectedObject().ShouldEqual(persons.FirstOrDefault());
            }
        }

        private IEnumerator<Person> InsertTestPersons()
        {
            var persons = InsertTestPersonsList();
            return persons.GetEnumerator();
        }

        private IList<Person>  InsertTestPersonsList()
        {
            var persons = new List<Person>()
            {
                PersonBuilder.NewInstance().Build(),
                PersonBuilder.NewInstance().Build(),
                PersonBuilder.NewInstance().Build(),
                PersonBuilder.NewInstance().Build(),
                PersonBuilder.NewInstance().Build(),
            };

            persons.ForEach(p => 
            {
                personRepository.Save(p);
                Assert.True(personRepository.Exists(p));
            });

            return persons;
        }        
    }
}