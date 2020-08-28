using System;
using System.Collections.Generic;

using DataQI.Dapper.FastCrud.Repository;

namespace DataQI.Dapper.FastCrud.Test.Repository.Persons
{
    public interface IPersonRepository : IDapperRepository<Person>
    {
        IEnumerable<Person> FindByFullName(string fullName);

        IEnumerable<Person> FindByFullNameLikeAndActive(string name, bool active = true);

        IEnumerable<Person> FindByEmailLikeAndPhoneNotNull(string email);

        IEnumerable<Person> FindByDateOfBirthBetween(DateTime startDate, DateTime endDate);

        IEnumerable<Person> FindByDateRegisterLessThanEqualOrDateOfBirthGreaterThan(DateTime dateRegister, DateTime dateOfBirth);
    }
}