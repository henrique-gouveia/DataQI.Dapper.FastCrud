using System;
using System.Collections.Generic;

using DataQI.Dapper.FastCrud.Repository;

namespace DataQI.Dapper.FastCrud.Test.Repository.Customers
{
    public interface ICustomerRepository : IDapperRepository<Customer>
    {
        IEnumerable<Customer> FindByFullName(string fullName);

        IEnumerable<Customer> FindByFullNameLikeAndActive(string name, bool active = true);

        IEnumerable<Customer> FindByEmailLikeAndPhoneNotNull(string email);

        IEnumerable<Customer> FindByDateOfBirthBetween(DateTime startDate, DateTime endDate);

        IEnumerable<Customer> FindByDateRegisterLessThanEqualOrDateOfBirthGreaterThan(DateTime dateRegister, DateTime dateOfBirth);
    }
}