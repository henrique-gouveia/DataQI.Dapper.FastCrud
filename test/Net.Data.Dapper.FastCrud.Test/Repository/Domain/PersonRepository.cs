using System.Collections.Generic;
using System.Data;
using System.Linq;

using Dapper.FastCrud;

using Net.Data.Dapper.FastCrud.Repository;
using Net.Data.Dapper.FastCrud.Test.Repository.Domain;

namespace Net.Data.Dapper.FastCrud.Test.Repository.Domain
{
    public class PersonRepository : DapperRepository<Person>, IPersonRepository
    {
        public PersonRepository(IDbConnection connection) : base(connection)
        {

        }

        public Person FindByFullName(string fullName)
        {
            // Aqui, temos pelos menos 2 formas diferentes de implementar...
            // 1. Customizando um statement, por exemplo, "SELECT * FROM PERSON WHERE FULL_NAME = ?"
            //
            //   1.1.1 "?" poderia ser substituído por concatenação possibilitando SQL INJECTION, "... FULL_NAME = " + fullName
            //   1.1.2 "?" poderia ser utilizado como um parâmetro, por exemplo, "... FULL_NAME = @NAME"
            //   1.1.3 Outro problema explicito é o acoplamento rigido com o metadados especifico do banco, inclusive detalhes
            //         especificos de cada banco para prover uma paginação, por exemplo.
            //
            // 2. A segunda, seria utilizar o parâmetro statementOptions, porém ainda existirá a necessidade do acoplamento
            //    com o metadados especifico do banco, no caso, com o nome de cada respectiva coluna no banco de dados.
 
            var persons = connection.Find<Person>(options => options.Where($" FULL_NAME = '{fullName}' "));
            return persons.FirstOrDefault();
        }

        public IEnumerable<Person> FindByTelefone(string telefone)
        {
            throw new System.NotImplementedException();
        }
    }
}