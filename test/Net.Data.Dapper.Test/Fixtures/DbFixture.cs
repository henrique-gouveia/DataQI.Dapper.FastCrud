using System.Collections.Generic;
using Dapper.FastCrud;
using Net.Data.Dapper.Test.Repository.Domain;

namespace Net.Data.Dapper.Test.Fixtures
{
    public class DbFixture
    {
        public DbFixture()
        {
            OrmConfiguration.DefaultDialect = SqlDialect.SqLite;

            DbTest = DbTest.Instance;
            PersonRepository = new PersonRepository(DbTest.Connection);

            DbTest.CreateTables();
            DbTest.DeleteAllPersons();

            InsertDefaultPersons();
        }

        private void InsertDefaultPersons()
        {
            var persons = new List<Person>()
            {
                PersonBuilder.NewInstance().Build(),
                PersonBuilder.NewInstance().Build(),
                PersonBuilder.NewInstance().Build(),
                PersonBuilder.NewInstance().Build(),
                PersonBuilder.NewInstance().Build(),
            };
            persons.ForEach(p => DbTest.InsertPerson(p));
        }

        public DbTest DbTest { get; private set; }

        public IPersonRepository PersonRepository { get; private set; }
    }
}