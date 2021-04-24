using System.Text;
using System.Data;
using System.Data.SQLite;

using Dapper.FastCrud;

using DataQI.Dapper.FastCrud.Repository.Support;

using DataQI.Dapper.FastCrud.Test.Extensions;
using DataQI.Dapper.FastCrud.Test.Resources;
using DataQI.Dapper.FastCrud.Test.Repository.Customers;
using DataQI.Dapper.FastCrud.Test.Repository.Products;
using DataQI.Dapper.FastCrud.Test.Repository.Employees;
using DataQI.Dapper.FastCrud.Repository;

namespace DataQI.Dapper.FastCrud.Test.Fixtures
{
    public class DbFixture
    {
        public DbFixture()
        {
            OrmConfiguration.DefaultDialect = SqlDialect.SqLite;
            Connection = CreateConnection();
            
            CreateTables();

            // 1. Default
            // CustomerRepository = new CustomerRepository(Connection);
            // EmployeeRepository = new EmployeeRepository(Connection);
            // ProductRepository = new ProductRepository(Connection);

            // 2. Provided
            var repositoryFactory = new DapperRepositoryFactory();

            CustomerRepository = repositoryFactory.GetRepository<IDapperRepository<Customer>>(Connection);
            EmployeeRepository = repositoryFactory.GetRepository<IEmployeeRepository>(() => new EmployeeRepository(Connection));
            ProductRepository = repositoryFactory.GetRepository<IProductRepository>(Connection);
        }

        private IDbConnection CreateConnection() 
        {
            var connection = new SQLiteConnection("Data Source=:memory:");
            connection.Open();

            return connection;
        }

        private void CreateTables()
        {
            using (var command = Connection.CreateCommand())
            {
                var sql = new StringBuilder()
                    .Append(SqlResource.CUSTOMER_CREATE_SCRIPT)
                    .Append(SqlResource.DEPARTMENT_CREATE_SCRIT)
                    .Append(SqlResource.EMPLOYEE_CREATE_SCRIT)
                    .Append(SqlResource.PRODUCT_CREATE_SCRIPT)
                    .ToString();

                command
                    .AddCommandText(sql)
                    .PrepareAndExecuteNonQuery();
            }
        }

        public IDbConnection Connection { get; }

        public IDapperRepository<Customer> CustomerRepository { get; }
        public IEmployeeRepository EmployeeRepository { get; }
        public IProductRepository ProductRepository { get; }
    }
}

