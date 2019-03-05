using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

using Net.Data.Dapper.FastCrud.Test.Extensions;
using Net.Data.Dapper.FastCrud.Test.Repository.Domain;
using Net.Data.Dapper.FastCrud.Test.Resources;

namespace Net.Data.Dapper.FastCrud.Test
{
    public class DbTest
    {
        private static DbTest instance;

        protected DbTest()
        {
            Connection = new SQLiteConnection("Data Source=:memory:");
            //connection = new SqliteConnection("Data Source=sharedmemdb;Mode=Memory;Cache=Shared");
            Connection.Open();
        }

        public void CreateTables()
        {
            using (var command = Connection.CreateCommand())
            {
                command
                    .AddCommandText(SqlResource.PERSON_CREATE_SCRIPT)
                    .PrepareAndExecuteNonQuery();
            }
        }

        public object SelectLastInsertRowId()
        {
            using (var command = Connection.CreateCommand())
            {
                object id = null;

                var reader = command
                    .AddCommandText(SqlResource.SELECT_LAST_INSERT_ROW_ID)
                    .PreparaAndExecuteQuery();

                if (reader.Read())
                    id = reader["ID"];

                return id;
            }
        }

        public Person SelectPerson(int id)
        {
            using (var command = Connection.CreateCommand())
            {
                var reader = command
                    .AddCommandText(SqlResource.PERSON_SELECT_ONE_SCRIPT)
                    .AddParameter("@PERSON_ID", id)
                    .PreparaAndExecuteQuery();

                if (reader.Read())
                {
                    var person = new Person()
                    {
                        Id = Convert.ToInt32(reader["PERSON_ID"]),
                        FullName = Convert.ToString(reader["FULL_NAME"]),
                        Email = Convert.ToString(reader["EMAIL"]),
                        Phone = Convert.ToString(reader["TELEPHONE"]),
                        DateOfBirth = Convert.ToDateTime(reader["DATE_BIRTH"])
                    };

                    return person;
                }
            }

            return null;
        }

        public IEnumerable<Person> SelectPersons()
        {
            var persons = new List<Person>();

            using (var command = Connection.CreateCommand())
            {
                var reader = command
                    .AddCommandText(SqlResource.PERSON_SELECT_ALL_SCRIPT)
                    .PreparaAndExecuteQuery();

                if (reader.Read())
                {
                    var person = new Person()
                    {
                        Id = Convert.ToInt32(reader["PERSON_ID"]),
                        FullName = Convert.ToString(reader["FULL_NAME"]),
                        Email = Convert.ToString(reader["EMAIL"]),
                        Phone = Convert.ToString(reader["TELEPHONE"]),
                        DateOfBirth = Convert.ToDateTime(reader["DATE_BIRTH"])
                    };

                    persons.Add(person);
                }
            }

            return persons;
        }

        public void InsertPerson(Person person)
        {
            using (var command = Connection.CreateCommand())
            {
                int personId = 0;

                var affects = command
                    .AddCommandText(SqlResource.PERSON_INSERT_SCRIPT)
                    .AddParameter("@FULL_NAME", person.FullName)
                    .AddParameter("@EMAIL", person.Email)
                    .AddParameter("@TELEPHONE", person.Phone)
                    .AddParameter("@DATE_BIRTH", person.DateOfBirth)
                    .PrepareAndExecuteNonQuery();

                if (affects > 0)
                {
                    personId = Convert.ToInt32(SelectLastInsertRowId());
                    person.Id = personId;
                }
            }
        }

        public void DeleteAllPersons()
        {
            using (var command = Connection.CreateCommand())
            {
                command
                    .AddCommandText(SqlResource.PERSON_DELETE_ALL_SCRIPT)
                    .PrepareAndExecuteNonQuery();
            }
        }

        public IDbConnection Connection { get; private set; }

        public static DbTest Instance
        {
            get
            {
                if (instance == null)
                    instance = new DbTest();

                return instance;
            }
        }
    }
}