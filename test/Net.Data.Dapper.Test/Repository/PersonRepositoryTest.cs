using System.Linq;
using System.Threading.Tasks;

using Xunit;
using ExpectedObjects;

using Net.Data.Dapper.Test.Fixtures;
using Net.Data.Dapper.Test.Repository.Domain;

using Net.Data.Dapper.Repository;

namespace Net.Data.Dapper.Test.Repository
{
    public class PersonRepositoryTest : IClassFixture<DbFixture>
    {
        private DbTest dbTest;
        private IDapperRepository<Person> personRepository;

        public PersonRepositoryTest(DbFixture fixture)
        {
            dbTest = fixture.DbTest;
            personRepository = fixture.PersonRepository;
        }

        [Fact]
        public void TestFindAll()
        {
            var persons = personRepository.FindAll();
            Assert.True(persons.ToList().Count > 0);

            persons.ToList().ForEach(personExpected =>
            {
                var person = dbTest.SelectPerson(personExpected.Id);
                Assert.NotNull(person);
                personExpected.ToExpectedObject().ShouldEqual(person);
            });
        }

        [Fact]
        public async Task TestFindAllAsync()
        {
            var persons = await personRepository.FindAllAsync();
            Assert.True(persons.ToList().Count > 0);

            persons.ToList().ForEach(personExpected =>
            {
                var person = dbTest.SelectPerson(personExpected.Id);
                Assert.NotNull(person);
                personExpected.ToExpectedObject().ShouldEqual(person);
            });
        }

        [Fact]
        public void TestFindOne()
        {
            var persons = dbTest.SelectPersons();

            persons.ToList().ForEach(personExpected =>
            {
                var person = personRepository.FindOne(new Person() { Id = personExpected.Id });
                Assert.NotNull(person);
                personExpected.ToExpectedObject().ShouldEqual(person);
            });
        }

        [Fact]
        public void TestFindOneAsync()
        {
            var persons = dbTest.SelectPersons();

            persons.ToList().ForEach(async personExpected =>
            {
                var person = await personRepository.FindOneAsync(new Person() { Id = personExpected.Id });
                personExpected.ToExpectedObject().ShouldEqual(person);
            });
        }

        [Fact]
        public void TestExists()
        {
            var persons = dbTest.SelectPersons();

            persons.ToList().ForEach(personExpected =>
            {
                var personExists = personRepository.Exists(new Person() { Id = personExpected.Id });
                Assert.True(personExists);
            });
        }

        [Fact]
        public void TestExistsAsync()
        {
            var persons = dbTest.SelectPersons();

            persons.ToList().ForEach(async personExpected =>
            {
                var personExists = await personRepository.ExistsAsync(new Person() { Id = personExpected.Id });
                Assert.True(personExists);
            });
        }

        [Fact]
        public void TestInsert()
        {
            var personExpected = PersonBuilder.NewInstance().Build();
            personRepository.Insert(personExpected);

            var person = dbTest.SelectPerson(personExpected.Id);
            
            personExpected.ToExpectedObject().ShouldEqual(person);
        }

        [Fact]
        public async Task TestInsertAsync()
        {
            var personExpected = PersonBuilder.NewInstance().Build();
            await personRepository.InsertAsync(personExpected);

            var person = dbTest.SelectPerson(personExpected.Id);
            
            personExpected.ToExpectedObject().ShouldEqual(person);
        }

        [Fact]
        public void TestSave()
        {
            var personInserted = PersonBuilder.NewInstance().Build();
            personRepository.Save(personInserted);

            var personExpected = PersonBuilder.NewInstance().SetId(personInserted.Id).Build();
            personRepository.Save(personExpected);

            var person = dbTest.SelectPerson(personExpected.Id);

            personExpected.ToExpectedObject().ShouldEqual(person);
        }

        [Fact]
        public async Task TestSaveAsync()
        {
            var personInserted = PersonBuilder.NewInstance().Build();
            await personRepository.SaveAsync(personInserted);

            var personExpected = PersonBuilder.NewInstance().SetId(personInserted.Id).Build();
            await personRepository.SaveAsync(personExpected);

            var person = dbTest.SelectPerson(personExpected.Id);

            personExpected.ToExpectedObject().ShouldEqual(person);
        }

        [Fact]
        public void TestDelete()
        {
            var personExpected = PersonBuilder.NewInstance().Build();
            personRepository.Insert(personExpected);

            var person = dbTest.SelectPerson(personExpected.Id);
            Assert.NotNull(person);

            personRepository.Delete(personExpected);

            person = dbTest.SelectPerson(personExpected.Id);
            Assert.Null(person);
        }

        [Fact]
        public async Task TestDeleteAsync()
        {
            var personExpected = PersonBuilder.NewInstance().Build();
            await personRepository.InsertAsync(personExpected);

            var person = dbTest.SelectPerson(personExpected.Id);
            Assert.NotNull(person);

            await personRepository.DeleteAsync(personExpected);

            person = dbTest.SelectPerson(personExpected.Id);
            Assert.Null(person);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // 1. Quando desejo criar uma busca por outra propriedade tenho que implementar, porém cada desenvolvedor,   
        //    de acordo com sua experiência, implementa da sua forma.                                                 
        //
        //    1.1. Estende a classe DapperRepository, porém instancia a classe concreta;
        //
        //         public class PersonRepository : DapperRepository<Person>
        //         {
        //             public Person FindByFullName(string fullName) { ... }
        //         }
        //
        //         IDapperRepository<Person> personRepository = new PersonRepository(connection);
        //         ((PersonRepository)personRepository).FindByName("FIRSTNAME LASTNAME");  
        //
        //
        //    1.2. Estende a classe DapperRepository, porém realiza um cast quando necessário utilizar o método 
        //         desejado;
        //
        //         public class PersonRepository : DapperRepository<Person>
        //         {
        //             public Person FindByFullName(string fullName) { ... }
        //         }
        //
        //         PersonRepository personRepository = new PersonRepository(connection);
        //         personRepository.FindByName("FIRSTNAME LASTNAME");  
        //
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

        [Fact]
        public void TestFindByName()
        {
            var persons = dbTest.SelectPersons();
            var personRepositoryCustom = new PersonRepository(dbTest.Connection);

            persons.ToList().ForEach(personExpected =>
            {
                var person = personRepositoryCustom.FindByFullName(personExpected.FullName);
                personExpected.ToExpectedObject().ShouldEqual(person);
            });
        }
    }
}