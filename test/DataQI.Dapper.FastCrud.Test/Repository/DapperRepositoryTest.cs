using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

using Dapper.FastCrud;
using Dapper.FastCrud.Configuration.StatementOptions.Builders;

using Xunit;
using ExpectedObjects;

using DataQI.Commons.Query;
using DataQI.Commons.Query.Support;

using DataQI.Dapper.FastCrud.Repository;
using DataQI.Dapper.FastCrud.Test.Fixtures;
using DataQI.Dapper.FastCrud.Test.Repository.Customers;

namespace DataQI.Dapper.FastCrud.Test.Repository
{
    public class DapperRepositoryTest : IClassFixture<DbFixture>, IDisposable
    {
        private readonly IDbConnection connection;
        private readonly IDapperRepository<Customer> customerRepository;

        public DapperRepositoryTest(DbFixture fixture)
        {
            connection = fixture.Connection;
            customerRepository = fixture.CustomerRepository;
        }
        
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task TestInsertRejectsNullEntity(bool useAsyncMethod)
        {
            try
            {
                if (useAsyncMethod)
                    await customerRepository.InsertAsync(null);
                else
                    customerRepository.Insert(null);
            }
            catch (Exception e)
            {
                var baseException = e.GetBaseException();
                Assert.IsType<ArgumentException>(baseException);
                Assert.Equal("Entity must not be null", baseException.Message);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TestInsert(bool useAsyncMethod)
        {
            var countBefore = connection.Count<Customer>();
            var countExpected = ++countBefore;

            var customerExpected = CustomerBuilder.NewInstance().Build();
            if (useAsyncMethod)
                customerRepository.InsertAsync(customerExpected).Wait();
            else
                customerRepository.Insert(customerExpected);

            Assert.True(customerExpected.Id > 0);
            Assert.Equal(countExpected, connection.Count<Customer>());
        }
        
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task TestSaveRejectsNullEntity(bool useAsyncMethod)
        {
            try
            {
                if (useAsyncMethod)
                    await customerRepository.SaveAsync(null);
                else
                    customerRepository.Save(null);
            }
            catch (Exception e)
            {
                var baseException = e.GetBaseException();
                Assert.IsType<ArgumentException>(baseException);
                Assert.Equal("Entity must not be null", baseException.Message);
            }
        }
        
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TestSave(bool useAsyncMethod)
        {
            var countBefore = connection.Count<Customer>();
            var countExpected = ++countBefore;

            var customerInserted = CustomerBuilder.NewInstance().Build();
            SaveCustomer(customerInserted, useAsyncMethod);

            var customerUpdated = CustomerBuilder.NewInstance().SetId(customerInserted.Id).Build();
            SaveCustomer(customerUpdated, useAsyncMethod);

            var customerFinded = connection.Get<Customer>(customerUpdated);

            customerUpdated.ToExpectedObject().ShouldMatch(customerFinded);
            Assert.Equal(countExpected, connection.Count<Customer>());
        }

        private void SaveCustomer(Customer customer, bool useAsyncMethod)
        {
            if (useAsyncMethod)
                customerRepository.SaveAsync(customer).Wait();
            else
                customerRepository.Save(customer);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TestExistsReturnsTrue(bool useAsyncMethod)
        {
            var customersExpected = InsertTestCustomers();

            while (customersExpected.MoveNext())
            {
                var customer = customersExpected.Current;
                bool customerExists = ExistsCustomer(customer, useAsyncMethod);

                Assert.True(customerExists);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TestExistsReturnsFalse(bool useAsyncMethod)
        {
            InsertTestCustomers();
            var customerExists = ExistsCustomer(new Customer(), useAsyncMethod);
            Assert.False(customerExists);
        }
                
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task TestFindRejectsNullStatementBuilder(bool useAsyncMethod)
        {
            try
            {
                Func<
                    IRangedBatchSelectSqlSqlStatementOptionsOptionsBuilder<Customer>,
                    IRangedBatchSelectSqlSqlStatementOptionsOptionsBuilder<Customer>
                > statementBuilder = null;
                if (useAsyncMethod)
                    await customerRepository.FindAsync(statementBuilder);
                else
                    customerRepository.Find(statementBuilder);
            }
            catch (Exception e)
            {
                var baseException = e.GetBaseException();
                Assert.IsType<ArgumentException>(baseException);
                Assert.Equal("StatementBuilder must not be null", baseException.Message);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TestFindByStatementBuilder(bool useAsyncMethod)
        {
            var customersList = InsertTestCustomersList();
            using var customersEnumerator = customersList.GetEnumerator();
            while (customersEnumerator.MoveNext())
            {
                var customer = customersEnumerator.Current;
                var customersExpected = customersList
                    .Where(c => c.Document == customer?.Document);

                Func<
                    IRangedBatchSelectSqlSqlStatementOptionsOptionsBuilder<Customer>,
                    IRangedBatchSelectSqlSqlStatementOptionsOptionsBuilder<Customer>
                > statementBuilder = statement => statement
                    .Where($"{nameof(Customer.Document):C} = @Document")
                    .WithParameters(new { customer?.Document });

                IEnumerable<Customer> customers;
                if (useAsyncMethod)
                    customers = customerRepository.FindAsync(statementBuilder).Result;
                else
                    customers = customerRepository.Find(statementBuilder);
                
                customersExpected.ToExpectedObject().ShouldMatch(customers);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task TestFindRejectsNullCriteria(bool useAsyncMethod)
        {
            try
            {
                Func<ICriteria, ICriteria> criteriaBuilder = null;
                if (useAsyncMethod)
                    await customerRepository.FindAsync(criteriaBuilder);
                else
                    customerRepository.Find(criteriaBuilder);
            }
            catch (Exception e)
            {
                var baseException = e.GetBaseException();
                Assert.IsType<ArgumentException>(baseException);
                Assert.Equal("CriteriaBuilder must not be null", baseException.Message);
            }
        }
        
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TestFindByCriteria(bool useAsyncMethod)
        {
            var customersList = InsertTestCustomersList();
            var customersEnumerator = customersList.GetEnumerator();

            while (customersEnumerator.MoveNext())
            {
                var customer = customersEnumerator.Current;
                var customerFullNameStartsWith = customer.FullName.Substring(0, 5);

                Func<ICriteria, ICriteria> criteriaBuilder = criteria =>
                    criteria.Add(Restrictions.Like($"{nameof(Customer.FullName)}", $"{customerFullNameStartsWith}%"));

                var customersExpected = customersList
                    .Where(c => c.FullName.Substring(0, 5) == customerFullNameStartsWith)
                    .ToList();

                IEnumerable<Customer> customers;

                if (useAsyncMethod)
                    customers = customerRepository.FindAsync(criteriaBuilder).Result;
                else
                    customers = customerRepository.Find(criteriaBuilder);

                customersExpected.ToExpectedObject().ShouldMatch(customers);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TestFindAll(bool useAsyncMethod)
        {
            var customersExpected = InsertTestCustomersList();
            IEnumerable<Customer> customers;

            if (useAsyncMethod)
                customers = customerRepository.FindAllAsync().Result;
            else
                customers = customerRepository.FindAll();

            customersExpected.ToExpectedObject().ShouldMatch(customers);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TestFindOneReturnsEntity(bool useAsyncMethod)
        {
            var customersExpected = InsertTestCustomers();

            while (customersExpected.MoveNext())
            {
                var customerExpected = customersExpected.Current;
                Customer customer = FindOneCustomer(customerExpected, useAsyncMethod);

                customerExpected.ToExpectedObject().ShouldMatch(customer);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TestFindOneReturnsNull(bool useAsyncMethod)
        {
            InsertTestCustomers();
            var customer = FindOneCustomer(new Customer(), useAsyncMethod);

            Assert.Null(customer);
        }

        private Customer FindOneCustomer(Customer customer, bool useAsyncMethod)
        {
            if (useAsyncMethod)
                return customerRepository.FindOneAsync(customer).Result;
            else
                return customerRepository.FindOne(customer);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TestDelete(bool useAsyncMethod)
        {
            var customers = InsertTestCustomers();

            while (customers.MoveNext())
            {
                var customer = customers.Current;
                if (useAsyncMethod)
                    customerRepository.DeleteAsync(customer).Wait();
                else
                    customerRepository.Delete(customer);

                Assert.False(ExistsCustomer(customer, useAsyncMethod));
                Assert.Null(FindOneCustomer(customer, useAsyncMethod));
            }
        }

        private bool ExistsCustomer(Customer customer, bool useAsyncMethod)
        {
            if (useAsyncMethod)
                return customerRepository.ExistsAsync(customer).Result;
            else
                return customerRepository.Exists(customer);
        }

        private IEnumerator<Customer> InsertTestCustomers()
        {
            var customers = InsertTestCustomersList();
            return customers.GetEnumerator();
        }

        private IList<Customer> InsertTestCustomersList()
        {
            var customers = new List<Customer>()
            {
                CustomerBuilder.NewInstance().Build(),
                CustomerBuilder.NewInstance().Build(),
                CustomerBuilder.NewInstance().Build(),
                CustomerBuilder.NewInstance().Build(),
                CustomerBuilder.NewInstance().Build(),
            };

            customers.ForEach(p =>
            {
                customerRepository.Save(p);
                Assert.True(customerRepository.Exists(p));
            });

            return customers;
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                connection.BulkDelete<Customer>();
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
