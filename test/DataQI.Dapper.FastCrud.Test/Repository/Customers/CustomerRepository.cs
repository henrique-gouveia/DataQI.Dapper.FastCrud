using System;
using System.Collections.Generic;
using System.Data;

using Dapper.FastCrud;
using DataQI.Dapper.FastCrud.Repository.Support;

namespace DataQI.Dapper.FastCrud.Test.Repository.Customers
{
    public class CustomerRepository : DapperRepository<Customer>
    {
        public CustomerRepository(IDbConnection connection) : base(connection)
        {

        }

        public IEnumerable<Customer> FindByFullName(string fullName)
        {
            var customers = connection
                    .Find<Customer>(statement => statement
                    .Where($"({nameof(Customer.FullName):C} = @fullName)")
                    .WithParameters(new { fullName }));

            return customers;
        }

        public IEnumerable<Customer> FindByFullNameLikeAndActive(string fullName, bool active = true)
        {
            var customers = connection
                .Find<Customer>(statement => statement
                .Where($"({nameof(Customer.FullName):C} LIKE @fullName AND {nameof(Customer.Active):C} = @active)")
                .WithParameters(new { fullName, active }));

            return customers;
        }

        public IEnumerable<Customer> FindByEmailLikeAndPhoneNotNull(string email)
        {
            var customers = connection
                .Find<Customer>(statement => statement
                .Where($"({nameof(Customer.Email):C} LIKE @email AND {nameof(Customer.Phone):C} IS NOT NULL)")
                .WithParameters(new { email }));

            return customers;
        }

        public IEnumerable<Customer> FindByDateOfBirthBetween(DateTime startDate, DateTime endDate)
        {
            var customers = connection
                .Find<Customer>(statement => statement
                .Where($"({nameof(Customer.DateOfBirth):C} BETWEEN @startDate AND @endDate)")
                .WithParameters(new { startDate, endDate }));

            return customers;
        }

        public IEnumerable<Customer> FindByDateRegisterLessThanEqualOrDateOfBirthGreaterThan(DateTime dateRegister, DateTime dateOfBirth)
        {
            var customers = connection
                .Find<Customer>(statement => statement
                .Where($"({nameof(Customer.DateRegister):C} <= @dateRegister) OR ({nameof(Customer.DateOfBirth):C} > @dateOfBirth)")
                .WithParameters(new { dateRegister, dateOfBirth }));

            return customers;
        }
    }
}