using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Dapper.FastCrud;
using Dapper.FastCrud.Configuration.StatementOptions.Builders;

using Xunit;
using ExpectedObjects;
using Moq;

using DataQI.Commons.Query;
using DataQI.Commons.Query.Ast;
using DataQI.Commons.Query.Support;

using DataQI.Dapper.FastCrud.Repository;
using DataQI.Dapper.FastCrud.Repository.Support;
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
        public void TestFindByCriteriaAppliesOrderByCorrectly(bool useAsyncMethod)
        {
            var customersList = InsertTestCustomersList();

            Func<ICriteria, ICriteria> criteriaBuilder = criteria => criteria
                .Add(Restrictions.Not(Restrictions.Null(nameof(Customer.FullName))))
                .AddOrder(Order.Desc(nameof(Customer.FullName)));

            var customersExpected = customersList
                .OrderByDescending(c => c.FullName)
                .ToList();

            IEnumerable<Customer> customers;

            if (useAsyncMethod)
                customers = customerRepository.FindAsync(criteriaBuilder).Result;
            else
                customers = customerRepository.Find(criteriaBuilder);

            var customersActual = customers.ToList();
            Assert.Equal(customersExpected.Count, customersActual.Count);
            for (var i = 0; i < customersExpected.Count; i++)
                customersExpected[i].ToExpectedObject().ShouldMatch(customersActual[i]);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task TestFindOneByCriteriaWithOrderingStillThrowsWhenMultipleMatches(bool useAsyncMethod)
        {
            InsertTestCustomersList();

            Func<ICriteria, ICriteria> criteriaBuilder = criteria => criteria
                .Add(Restrictions.Not(Restrictions.Null(nameof(Customer.FullName))))
                .AddOrder(Order.Desc(nameof(Customer.FullName)));

            if (useAsyncMethod)
                await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                    await customerRepository.FindOneAsync(criteriaBuilder));
            else
            {
                var exception = Assert.Throws<TargetInvocationException>(() =>
                    customerRepository.FindOne(criteriaBuilder));
                Assert.IsType<InvalidOperationException>(exception.GetBaseException());
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

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public async Task TestFindByEqualNullMatchesCorrectly(bool useAsyncMethod, bool negated)
        {
            var nullEmailCustomer = CustomerBuilder.NewInstance().SetEmail(null).Build();
            var nonNullEmailCustomer = CustomerBuilder.NewInstance().SetEmail("adams@example.com").Build();
            customerRepository.Insert(nullEmailCustomer);
            customerRepository.Insert(nonNullEmailCustomer);

            var criterion = Restrictions.Equal(nameof(Customer.Email), null);
            if (negated)
                criterion = Restrictions.Not(criterion);
            Func<ICriteria, ICriteria> criteriaBuilder = criteria => criteria.Add(criterion);

            var customers = useAsyncMethod
                ? await customerRepository.FindAsync(criteriaBuilder)
                : customerRepository.Find(criteriaBuilder);

            var customer = Assert.Single(customers);
            Assert.Equal(negated ? nonNullEmailCustomer.Id : nullEmailCustomer.Id, customer.Id);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TestFindWithEmptyCriteriaReturnsAllEntities(bool useAsyncMethod)
        {
            var customersExpected = InsertTestCustomersList();

            Func<ICriteria, ICriteria> criteriaBuilder = criteria => criteria;

            IEnumerable<Customer> customers;
            if (useAsyncMethod)
                customers = customerRepository.FindAsync(criteriaBuilder).Result;
            else
                customers = customerRepository.Find(criteriaBuilder);

            customersExpected.ToExpectedObject().ShouldMatch(customers);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TestFindWithOnlyOrderingAndNoCriteriaKeepsOrdering(bool useAsyncMethod)
        {
            var customersList = InsertTestCustomersList();

            Func<ICriteria, ICriteria> criteriaBuilder = criteria =>
                criteria.AddOrder(Order.Desc(nameof(Customer.FullName)));

            var customersExpected = customersList
                .OrderByDescending(c => c.FullName, StringComparer.Ordinal)
                .ToList();

            IEnumerable<Customer> customers;
            if (useAsyncMethod)
                customers = customerRepository.FindAsync(criteriaBuilder).Result;
            else
                customers = customerRepository.Find(criteriaBuilder);

            customersExpected.ToExpectedObject().ShouldMatch(customers);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TestFindWithUnknownOrderPropertyThrowsClearly(bool useAsyncMethod)
        {
            Func<ICriteria, ICriteria> criteriaBuilder = criteria =>
                criteria.AddOrder(Order.Asc("DoesNotExist"));

            var exception = Assert.ThrowsAny<Exception>(() =>
            {
                if (useAsyncMethod)
                    customerRepository.FindAsync(criteriaBuilder).Wait();
                else
                    customerRepository.Find(criteriaBuilder);
            });

            var baseException = exception.GetBaseException();
            Assert.IsType<ArgumentException>(baseException);
            Assert.Contains("DoesNotExist", baseException.Message);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task TestFindOneByCriteriaLimitsQueryToTwoRows(bool useAsyncMethod)
        {
            InsertTestCustomersList();

            var commands = new List<IDbCommand>();
            var observedConnection = new Mock<IDbConnection>(MockBehavior.Strict);
            observedConnection.SetupGet(c => c.State).Returns(connection.State);
            observedConnection.SetupGet(c => c.ConnectionString).Returns(connection.ConnectionString);
            observedConnection.Setup(c => c.CreateCommand()).Returns(() =>
            {
                var command = connection.CreateCommand();
                commands.Add(command);
                return command;
            });

            var repository = new DapperRepository<Customer>(observedConnection.Object);
            Func<ICriteria, ICriteria> criteriaBuilder = criteria => criteria
                .Add(Restrictions.Not(Restrictions.Null(nameof(Customer.FullName))))
                .AddOrder(Order.Desc(nameof(Customer.FullName)));

            if (useAsyncMethod)
                await Assert.ThrowsAsync<InvalidOperationException>(() => repository.FindOneAsync(criteriaBuilder));
            else
                Assert.Throws<InvalidOperationException>(() => repository.FindOne(criteriaBuilder));

            var executedCommand = Assert.Single(commands);
            Assert.Matches(@"(?i)\bLIMIT\s+2\b", executedCommand.CommandText);
        }

        [Theory]
        [InlineData(LogicalKind.And, false, false)]
        [InlineData(LogicalKind.And, false, true)]
        [InlineData(LogicalKind.And, true, false)]
        [InlineData(LogicalKind.And, true, true)]
        [InlineData(LogicalKind.Or, false, false)]
        [InlineData(LogicalKind.Or, false, true)]
        [InlineData(LogicalKind.Or, true, false)]
        [InlineData(LogicalKind.Or, true, true)]
        public async Task TestCriteriaQueriesRejectEmptyJunction(LogicalKind kind, bool findOne, bool useAsyncMethod)
        {
            Func<ICriteria, ICriteria> criteriaBuilder = criteria => criteria.Add(new Junction(kind));
            Exception exception;

            if (useAsyncMethod)
            {
                exception = findOne
                    ? await Assert.ThrowsAnyAsync<Exception>(() => customerRepository.FindOneAsync(criteriaBuilder))
                    : await Assert.ThrowsAnyAsync<Exception>(() => customerRepository.FindAsync(criteriaBuilder));
            }
            else
            {
                exception = findOne
                    ? Assert.ThrowsAny<Exception>(() => customerRepository.FindOne(criteriaBuilder))
                    : Assert.ThrowsAny<Exception>(() => customerRepository.Find(criteriaBuilder).ToArray());
            }

            var baseException = Assert.IsType<InvalidOperationException>(exception.GetBaseException());
            Assert.Equal($"Junction '{kind}' must contain at least one criterion.", baseException.Message);
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
