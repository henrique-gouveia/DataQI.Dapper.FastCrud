using System.Collections.Generic;
using DataQI.Dapper.FastCrud.Repository;

namespace DataQI.Dapper.FastCrud.Test.Repository.Sample
{
    public interface IPersonRepository : IDapperRepository<Person>
    {
        // Person FindByFullName(string fullName);
        IEnumerable<Person> FindByFullName(string fullName);

        // Person FindByPhone(string phone);
        IEnumerable<Person> FindByPhone(string phone);
    }
}