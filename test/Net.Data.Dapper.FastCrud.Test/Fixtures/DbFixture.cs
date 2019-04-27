using System;
using System.Data;
using System.Data.SQLite;
using System.Collections.Generic;

using Dapper.FastCrud;

using Net.Data.Commons.Repository.Core;
using Net.Data.Dapper.FastCrud.Test.Repository.Sample;
using Net.Data.Dapper.FastCrud.Test.Extensions;
using Net.Data.Dapper.FastCrud.Test.Resources;
using Net.Data.Dapper.FastCrud.Repository.Support;

namespace Net.Data.Dapper.FastCrud.Test.Fixtures
{
    public class DbFixture
    {
        public DbFixture()
        {
            OrmConfiguration.DefaultDialect = SqlDialect.SqLite;

            Connection = new SQLiteConnection("Data Source=:memory:");
            Connection.Open();

            //PersonRepository = new PersonRepository(Connection);
            PersonRepository = RepositoryProxy.Create<IPersonRepository>(() => new DapperRepository<Person>(Connection));

            CreateTables();
        }

        private void CreateTables() 
        {
            using (var command = Connection.CreateCommand())
            {
                command
                    .AddCommandText(SqlResource.PERSON_CREATE_SCRIPT)
                    .PrepareAndExecuteNonQuery();
            }
        }

        public IDbConnection Connection { get; }

        public IPersonRepository PersonRepository { get; }
    }
}