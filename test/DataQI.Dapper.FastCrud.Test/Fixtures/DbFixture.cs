using System;
using System.Data;
using System.Data.SQLite;
using System.Collections.Generic;

using Dapper.FastCrud;

using DataQI.Commons.Repository.Core;
using DataQI.Dapper.FastCrud.Test.Repository.Sample;
using DataQI.Dapper.FastCrud.Test.Extensions;
using DataQI.Dapper.FastCrud.Test.Resources;
using DataQI.Dapper.FastCrud.Repository.Support;

namespace DataQI.Dapper.FastCrud.Test.Fixtures
{
    public class DbFixture
    {
        public DbFixture()
        {
            OrmConfiguration.DefaultDialect = SqlDialect.SqLite;

            Connection = new SQLiteConnection("Data Source=:memory:");
            Connection.Open();

            // 1. PersonRepository = new PersonRepository(Connection);
            // 2. PersonRepository = RepositoryProxy.Create<IPersonRepository>(() => new DapperRepository<Person>(Connection));
            // 3. 
            var repositoryFactory = new DapperRepositoryFactory(Connection);
            PersonRepository = repositoryFactory.GetRepository<IPersonRepository>();

            CreateTables();
        }

        private void CreateTables() 
        {
            using (var command = Connection.CreateCommand())
            {
                command
                    .AddCommandText(SqlResource.PERSON_CREATE_SCRIPT)
                    .PrepareAndExecuteNonQuery();
            }
        }

        public IDbConnection Connection { get; }

        public IPersonRepository PersonRepository { get; }
    }

    public class PersonDAO
    {
        private readonly IDbConnection connection;
        public PersonDAO(IDbConnection connection)
        {
            this.connection = connection;
        }

        public void Insert(Person person)
        {
            IDbTransaction insertTransaction = connection.BeginTransaction();
            try 
            {
                using (IDbCommand insertCommand = connection.CreateCommand())
                {
                    insertCommand.CommandText = @"
                        INSERT INTO PERSON (
                            PERSON_ID, FULL_NAME, TELEPHONE, EMAIL, DATE_BIRTH
                        ) VALUES (
                            @Id, @FullName, @Tehephone, @Email, @DateOfBirth
                        )";

                    IDbDataParameter idParameter = insertCommand.CreateParameter();
                    idParameter.ParameterName = "@Id";
                    idParameter.Value = person.Id;
                    insertCommand.Parameters.Add(idParameter);

                    IDbDataParameter fullNameParameter = insertCommand.CreateParameter();
                    fullNameParameter.ParameterName = "@FullName";
                    fullNameParameter.Value = person.FullName;
                    insertCommand.Parameters.Add(fullNameParameter);
                    
                    IDbDataParameter telephoneParameter = insertCommand.CreateParameter();
                    telephoneParameter.ParameterName = "@Tehephone";
                    telephoneParameter.Value = person.Phone;
                    
                    IDbDataParameter emailParameter = insertCommand.CreateParameter();
                    emailParameter.ParameterName = "@Email";
                    emailParameter.Value = person.Email;
                    
                    IDbDataParameter dataOfBirthParameter = insertCommand.CreateParameter();
                    dataOfBirthParameter.ParameterName = "@DateOfBirth";
                    dataOfBirthParameter.Value = person.DateOfBirth;

                    insertCommand.ExecuteNonQuery();
                }

                insertTransaction.Commit();
            } 
            catch 
            {
                insertTransaction.Rollback();
            }
        }

        public void Save(Person person) 
        {
            IDbTransaction transaction = connection.BeginTransaction();
            try 
            {
                if (Exists(person))
                    connection.Update<Person>(person);
                else
                    connection.Insert<Person>(person);
                    
                transaction.Commit();
            } 
            catch 
            {
                transaction.Rollback();
            }
        }

        public bool Exists(Person person)
        {
            return true;
        }
    }
}

