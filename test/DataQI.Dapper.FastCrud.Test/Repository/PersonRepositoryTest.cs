using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

using Dapper.FastCrud;

using Xunit;
using ExpectedObjects;

using DataQI.Dapper.FastCrud.Repository;
using DataQI.Dapper.FastCrud.Test.Fixtures;
using DataQI.Dapper.FastCrud.Test.Repository.Sample;

namespace DataQI.Dapper.FastCrud.Test.Repository
{
    public class PersonRepositoryTest : IClassFixture<DbFixture>, IDisposable
    {
        private readonly IDbConnection connection;

        private readonly IPersonRepository personRepository;

        public PersonRepositoryTest(DbFixture fixture)
        {
            connection = fixture.Connection;
            personRepository = fixture.PersonRepository;
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TestInsert(bool useAsyncMethod)
        {
            var countBefore = connection.Count<Person>();
            var countExpected = ++countBefore;

            var personExpected = PersonBuilder.NewInstance().Build();
            if (useAsyncMethod)
                personRepository.InsertAsync(personExpected).Wait();
            else
                personRepository.Insert(personExpected);

            Assert.True(personExpected.Id > 0);
            Assert.Equal(countExpected, connection.Count<Person>());
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TestSave(bool useAsyncMethod)
        {
            var countBefore = connection.Count<Person>();
            var countExpected = ++countBefore;

            var personInserted = PersonBuilder.NewInstance().Build();
            SavePerson(personInserted, useAsyncMethod);
            Assert.True(personInserted.Id > 0);
            Assert.Equal(countExpected, connection.Count<Person>());

            var personUpdated = PersonBuilder.NewInstance().SetId(personInserted.Id).Build();
            SavePerson(personUpdated, useAsyncMethod);
            Assert.Equal(personInserted.Id, personUpdated.Id);
            Assert.Equal(countExpected, connection.Count<Person>());
        }

        private void SavePerson(Person person, bool useAsyncMethod)
        {
            if (useAsyncMethod)
                personRepository.SaveAsync(person).Wait();
            else
                personRepository.Save(person);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TestExists(bool useAsyncMethod)
        {
            var personsExpected = InsertTestPersons();

            while (personsExpected.MoveNext()) 
            {
                var person = personsExpected.Current;
                bool personExists = ExistsPerson(person, useAsyncMethod);

                Assert.True(personExists);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TestExistsEntityNotFoundReturnsFalse(bool useAsyncMethod)
        {
            InsertTestPersons();
            var personExists = ExistsPerson(new Person(), useAsyncMethod);
            Assert.False(personExists);
        }

        private bool ExistsPerson(Person person, bool useAsyncMethod)
        {
            if (useAsyncMethod)
                return personRepository.ExistsAsync(person).Result;
            else
                return personRepository.Exists(person);
        }
        
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TestFindAll(bool useAsyncMethod)
        {
            var personsExpected = InsertTestPersonsList();
            IEnumerable<Person> persons = null;

            if (useAsyncMethod)
                persons = personRepository.FindAllAsync().Result;
            else
                persons = personRepository.FindAll();

            personsExpected.ToExpectedObject().ShouldEqual(persons);
        }
        
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TestFindOne(bool useAsyncMethod)
        {
            var personsExpected = InsertTestPersons();

            while (personsExpected.MoveNext())
            {
                var personExpected = personsExpected.Current;
                Person person = FindOnePerson(personExpected, useAsyncMethod);

                personExpected.ToExpectedObject().ShouldEqual(person);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TestFindOneEntityNotFoundReturnsNull(bool useAsyncMethod) 
        {
            InsertTestPersons();
            var person = FindOnePerson(new Person(), useAsyncMethod);
            
            Assert.Null(person);
        }

        private Person FindOnePerson(Person person, bool useAsyncMethod)
        {
            if (useAsyncMethod)
                return personRepository.FindOneAsync(person).Result;
            else
                return personRepository.FindOne(person);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TestDelete(bool useAsyncMethod)
        {
            var persons = InsertTestPersons();

            while (persons.MoveNext())
            {
                var person = persons.Current;
                if (useAsyncMethod)
                    personRepository.DeleteAsync(person).Wait();
                else
                    personRepository.Delete(person);

                Assert.False(ExistsPerson(person, useAsyncMethod));
                Assert.Null(FindOnePerson(person, useAsyncMethod));
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

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                connection.BulkDelete<Person>();
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}