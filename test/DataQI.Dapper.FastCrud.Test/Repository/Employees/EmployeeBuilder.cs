using Bogus;
using System;

namespace DataQI.Dapper.FastCrud.Test.Repository.Employees
{
    public class EmployeeBuilder
    {
        private readonly Employee employee;

        private EmployeeBuilder()
        {
            var faker = new Faker();

            employee = new Employee()
            {
                LastName = faker.Person.LastName,
                FirstName = faker.Person.FirstName,
                BirthDate = faker.Person.DateOfBirth,
                HireDate = faker.Person.DateOfBirth,
                Phone = faker.Phone.PhoneNumber("(##) # ####-####"),
                Email = faker.Person.Email,
            };
        }

        public static EmployeeBuilder NewInstance() => new EmployeeBuilder();

        public EmployeeBuilder SetId(int Id)
        {
            employee.Id = Id;
            return this;
        }

        public EmployeeBuilder SetLastName(string lastName)
        {
            employee.LastName = lastName;
            return this;
        }

        public EmployeeBuilder SetFirstName(string firstName)
        {
            employee.FirstName = firstName;
            return this;
        }

        public EmployeeBuilder SetTitle(string title)
        {
            employee.Title = title;
            return this;
        }

        public EmployeeBuilder SetDepartmentId(int departmentId)
        {
            employee.Department.Id = departmentId;
            employee.DepartmentId = departmentId;
            return this;
        }

        public EmployeeBuilder SetDepartment(Department department)
        {
            employee.DepartmentId = department.Id;
            employee.Department = department;
            return this;
        }

        public EmployeeBuilder SetBirthDate(DateTime birthDate)
        {
            employee.BirthDate = birthDate;
            return this;
        }

        public EmployeeBuilder SetHireDate(DateTime hireDate)
        {
            employee.HireDate = hireDate;
            return this;
        }

        public EmployeeBuilder SetPhone(string phone)
        {
            employee.Phone = phone;
            return this;
        }

        public EmployeeBuilder SetEmail(string email)
        {
            employee.Email = email;
            return this;
        }

        public Employee Build() => employee;
    }
}
