using System;
using System.Collections.Generic;
using System.Data;

using Dapper.FastCrud;
using DataQI.Dapper.FastCrud.Repository.Support;

namespace DataQI.Dapper.FastCrud.Test.Repository.Persons
{
    public class PersonRepository : DapperRepository<Person>, IPersonRepository
    {
        public PersonRepository(IDbConnection connection) : base(connection)
        {

        }
        
        public IEnumerable<Person> FindByFullName(string fullName)
        {
            var persons = connection
                    .Find<Person>(statement => statement
                    .Where($"({nameof(Person.FullName):C} = @fullName)")
                    .WithParameters(new { fullName }));

            return persons;
        }

        public IEnumerable<Person> FindByFullNameLikeAndActive(string fullName, bool active = true)
        {
            var persons = connection
                .Find<Person>(statement => statement
                .Where($"({nameof(Person.FullName):C} LIKE @fullName AND {nameof(Person.Active):C} = @active)")
                .WithParameters(new { fullName, active }));

            return persons;
        }

        public IEnumerable<Person> FindByEmailLikeAndPhoneIsNotNull(string email)
        {
            var persons = connection
                .Find<Person>(statement => statement
                .Where($"({nameof(Person.Email):C} LIKE @email AND {nameof(Person.Phone):C} IS NOT NULL)")
                .WithParameters(new { email }));

            return persons;
        }

        public IEnumerable<Person> FindByDateOfBirthBetween(DateTime startDate, DateTime endDate)
        {
            var persons = connection
                .Find<Person>(statement => statement
                .Where($"({nameof(Person.DateOfBirth):C} BETWEEN @startDate AND @endDate)")
                .WithParameters(new { startDate, endDate }));

            return persons;
        }

        public IEnumerable<Person> FindByDateRegisterLessThanEqualOrDateOfBirthGreaterThan(DateTime dateRegister, DateTime dateOfBirth)
        {
            var persons = connection
                .Find<Person>(statement => statement
                .Where($"({nameof(Person.DateRegister):C} <= @dateRegister) OR ({nameof(Person.DateOfBirth):C} > @dateOfBirth)")
                .WithParameters(new { dateRegister, dateOfBirth }));

            return persons;
        }
    }
}