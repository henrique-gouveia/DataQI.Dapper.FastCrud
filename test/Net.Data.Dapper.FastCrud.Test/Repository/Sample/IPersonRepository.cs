using System.Collections.Generic;
using Net.Data.Dapper.FastCrud.Repository;

namespace Net.Data.Dapper.FastCrud.Test.Repository.Sample
{
    public interface IPersonRepository : IDapperRepository<Person>
    {
        Person FindByFullName(string fullName);

        Person FindByPhone(string phone);
    }
}