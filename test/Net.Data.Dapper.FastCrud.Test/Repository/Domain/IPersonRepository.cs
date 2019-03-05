using System.Collections.Generic;
using Net.Data.Dapper.FastCrud.Repository;

namespace Net.Data.Dapper.FastCrud.Test.Repository.Domain
{
    public interface IPersonRepository : IDapperRepository<Person>
    {
        Person FindByFullName(string fullName);

        IEnumerable<Person> FindByTelefone(string telefone);
    }
}