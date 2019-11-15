using System;
using Bogus;
using Bogus.Extensions.Brazil;

namespace DataQI.Dapper.FastCrud.Test.Repository.Persons
{
    public class PersonBuilder
    {
        private readonly Person person;

        private PersonBuilder()
        {
            var faker = new Faker();

            person = new Person()
            {
                Active = faker.Random.Bool(),
                FullName = faker.Person.FullName,
                Document = faker.Person.Cpf(),
                Phone = faker.Phone.PhoneNumber("(##) # ####-####"),
                Email = faker.Person.Email,
                DateOfBirth = faker.Person.DateOfBirth,
                DateRegister = faker.Date.Past(1)
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

        public PersonBuilder SetActive(bool active)
        {
            person.Active = active;
            return this;
        }

        public PersonBuilder SetDocument(string document)
        {
            person.Document = document;
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

        public PersonBuilder SetDateRegister(DateTime dateRegister)
        {
            person.DateRegister = dateRegister;
            return this;
        }

        public Person Build() => person;
    }
}