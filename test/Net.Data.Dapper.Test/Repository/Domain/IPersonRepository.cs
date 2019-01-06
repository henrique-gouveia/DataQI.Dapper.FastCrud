using System.Collections.Generic;
using Net.Data.Dapper.Repository;

namespace Net.Data.Dapper.Test.Repository.Domain
{
    public interface IPersonRepository : IDapperRepository<Person>
    {
        Person FindByFullName(string fullName);

        IEnumerable<Person> FindByTelefone(string telefone);
    }
}