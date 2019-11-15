using System.Text;
using System.Data;
using System.Data.SQLite;

using Dapper.FastCrud;

using DataQI.Dapper.FastCrud.Repository.Support;

using DataQI.Dapper.FastCrud.Test.Extensions;
using DataQI.Dapper.FastCrud.Test.Resources;
using DataQI.Dapper.FastCrud.Test.Repository.Persons;
using DataQI.Dapper.FastCrud.Test.Repository.Products;

namespace DataQI.Dapper.FastCrud.Test.Fixtures
{
    public class DbFixture
    {
        public DbFixture()
        {
            OrmConfiguration.DefaultDialect = SqlDialect.SqLite;

            Connection = new SQLiteConnection("Data Source=:memory:");
            Connection.Open();

            // 1.
            // PersonRepository = new PersonRepository(Connection);
            // ProductRepository = new ProductRepository(Connection);

            // 2.
            var repositoryFactory = new DapperRepositoryFactory(Connection);
            PersonRepository = repositoryFactory.GetRepository<IPersonRepository>();
            ProductRepository = repositoryFactory.GetRepository<IProductRepository>();

            CreateTables();
        }

        private void CreateTables()
        {
            using (var command = Connection.CreateCommand())
            {
                var sql = new StringBuilder()
                    .Append(SqlResource.PERSON_CREATE_SCRIPT)
                    .Append(SqlResource.PRODUCT_CREATE_SCRIPT)
                    .ToString();

                command
                    .AddCommandText(sql)
                    .PrepareAndExecuteNonQuery();
            }
        }

        public IDbConnection Connection { get; }

        public IPersonRepository PersonRepository { get; }

        public IProductRepository ProductRepository { get; }
    }
}

