using System;
using Bogus;
using Bogus.Extensions.Brazil;

namespace DataQI.Dapper.FastCrud.Test.Repository.Customers
{
    public class CustomerBuilder
    {
        private readonly Customer customer;

        private CustomerBuilder()
        {
            var faker = new Faker();

            customer = new Customer()
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

        public static CustomerBuilder NewInstance() => new CustomerBuilder();

        public CustomerBuilder SetId(int id)
        {
            customer.Id = id;
            return this;
        }

        public CustomerBuilder SetActive(bool active)
        {
            customer.Active = active;
            return this;
        }

        public CustomerBuilder SetDocument(string document)
        {
            customer.Document = document;
            return this;
        }

        public CustomerBuilder SetFullName(string fullName)
        {
            customer.FullName = fullName;
            return this;
        }

        public CustomerBuilder SetPhone(string phone)
        {
            customer.Phone = phone;
            return this;
        }

        public CustomerBuilder SetEmail(string email)
        {
            customer.Email = email;
            return this;
        }

        public CustomerBuilder SetDateOfBirth(DateTime dateOfBirth)
        {
            customer.DateOfBirth = dateOfBirth;
            return this;
        }

        public CustomerBuilder SetDateRegister(DateTime dateRegister)
        {
            customer.DateRegister = dateRegister;
            return this;
        }

        public Customer Build() => customer;
    }
}