using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using Dapper.FastCrud;

using Xunit;
using ExpectedObjects;

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
        [InlineData(false)]
        [InlineData(true)]
        public void TestFindOneReturnsNull(bool useAsyncMethod)
        {
            InsertTestCustomers();
            var customer = FindOneCustomer(new Customer(), useAsyncMethod);

            Assert.Null(customer);
        }

        private bool ExistsCustomer(Customer customer, bool useAsyncMethod)
        {
            if (useAsyncMethod)
                return customerRepository.ExistsAsync(customer).Result;
            else
                return customerRepository.Exists(customer);
        }

        private Customer FindOneCustomer(Customer customer, bool useAsyncMethod)
        {
            if (useAsyncMethod)
                return customerRepository.FindOneAsync(customer).Result;
            else
                return customerRepository.FindOne(customer);
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
