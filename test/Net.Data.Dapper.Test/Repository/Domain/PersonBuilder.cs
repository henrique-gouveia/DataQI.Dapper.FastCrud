using System;
using Bogus;

namespace Net.Data.Dapper.Test.Repository.Domain
{
    public class PersonBuilder
    {
        private Person person;

        private PersonBuilder()
        {
            var faker = new Faker();

            person = new Person()
            {
                FullName = faker.Person.FullName,
                Phone = faker.Phone.PhoneNumber("(##) # ####-####"),
                Email = faker.Person.Email,
                DateOfBirth = faker.Person.DateOfBirth
            };
        }

        public static PersonBuilder NewInstance()
        {
            return new PersonBuilder();
        }

        public PersonBuilder SetId(int id)
        {
            person.Id = id;
            return this;
        }

        public PersonBuilder SetFullName(string fullName)
        {
            person.FullName = fullName;
            return this;
        }

        public PersonBuilder SetPhone(string phone)
        {
            person.Phone = phone;
            return this;
        }

        public PersonBuilder SetEmail(string email)
        {
            person.Email = email;
            return this;
        }

        public PersonBuilder SetDateOfBirth(DateTime dateOfBirth)
        {
            person.DateOfBirth = dateOfBirth;
            return this;
        }

        public Person Build() => person;
    }
}