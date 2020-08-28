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
        public IEnumerable<Person> FindByFullName(string fullName)
        {
            var persons = connection
                    .Find<Person>(statement => statement
                    .Where($"{nameof(Person.FullName):C} = @fullName")
                    .WithParameters(new { fullName }));

            return persons;
        }

        public IEnumerable<Person> FindByFullNameLikeAndActive(string fullName, bool active = true)
        {
            var persons = connection
                .Find<Person>(statement => statement
                .Where($"{nameof(Person.FullName):C} LIKE @fullName AND {nameof(Person.Active):C} = @active")
                .WithParameters(new { fullName, active }));

            return persons;
        }

        public IEnumerable<Person> FindByEmailLikeAndPhoneIsNotNull(string email)
        {
            var persons = connection
                .Find<Person>(statement => statement
                .Where($"{nameof(Person.Email):C} LIKE @email AND {nameof(Person.Phone):C} IS NOT NULL")
                .WithParameters(new { email }));

            return persons;
        }

        public IEnumerable<Person> FindByDateOfBirthBetween(DateTime startDate, DateTime endDate)
        {
            var persons = connection
                .Find<Person>(statement => statement
                .Where($"{nameof(Person.DateOfBirth):C} BETWEEN @startDate AND @endDate")
                .WithParameters(new { startDate, endDate }));

            return persons;
        }

        public IEnumerable<Person> FindByDateRegisterLessThanEqualOrDateOfBirthGreaterThan(DateTime dateRegister, DateTime dateOfBirth)
        {
            var persons = connection
                .Find<Person>(statement => statement
                .Where($"{nameof(Person.DateRegister):C} <= @dateRegister OR {nameof(Person.DateOfBirth):C} > @dateOfBirth")
                .WithParameters(new { dateRegister, dateOfBirth }));

            return persons;
        }
    }
}