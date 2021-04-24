using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using Dapper.FastCrud;
using ExpectedObjects;
using Xunit;

using DataQI.Dapper.FastCrud.Test.Fixtures;
using DataQI.Dapper.FastCrud.Test.Repository.Employees;

namespace DataQI.Dapper.FastCrud.Test.Repository
{
    public class DapperRepositoryCustomizedMethodTest : IClassFixture<DbFixture>, IDisposable
    {
        private readonly IDbConnection connection;
        private readonly IEmployeeRepository employeeRepository;

        public DapperRepositoryCustomizedMethodTest(DbFixture fixture)
        {
            connection = fixture.Connection;
            employeeRepository = fixture.EmployeeRepository;
        }
        
        [Theory]
        [InlineData("Production")]
        [InlineData("Sales")]
        public void TestInsertDepartment(string name)
        {
            var countBefore = connection.Count<Department>();
            var countExpected = ++countBefore;

            var department = new Department(name);
            employeeRepository.InsertDepartment(department);

            Assert.True(department.Id > 0);
            Assert.Equal(countExpected, connection.Count<Department>());
        }

        [Fact]
        public void TestFindByDepartmentName()
        {
            var employeeList = InsertTestEmployeesList();
            var employeeEnumerator = employeeList.GetEnumerator();

            while (employeeEnumerator.MoveNext())
            {
                var employee = employeeEnumerator.Current;
                var employeesExpected = employeeList.Where(e => e.Department.Name.StartsWith(employee.Department.Name));

                var products = employeeRepository.FindByDepartmentName(employee.Department.Name);

                employeesExpected.ToExpectedObject().ShouldMatch(products);
            }
        }

        private IList<Employee> InsertTestEmployeesList()
        {
            var productionDepartment = new Department("Production");
            var salesDepartment = new Department("Sales");
            
            employeeRepository.InsertDepartment(productionDepartment);
            employeeRepository.InsertDepartment(salesDepartment);

            var employees = new List<Employee>()
            {
                EmployeeBuilder.NewInstance().SetDepartment(productionDepartment).Build(),
                EmployeeBuilder.NewInstance().SetDepartment(productionDepartment).Build(),
                EmployeeBuilder.NewInstance().SetDepartment(productionDepartment).Build(),
                EmployeeBuilder.NewInstance().SetDepartment(salesDepartment).Build(),
                EmployeeBuilder.NewInstance().SetDepartment(salesDepartment).Build(),
            };

            employees.ForEach(o => InsertTestEmployee(o));

            return employees;
        }

        private void InsertTestEmployee(Employee employee)
        {
            employeeRepository.Save(employee);
            Assert.True(employeeRepository.Exists(employee));
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                connection.BulkDelete<Employee>();
                connection.BulkDelete<Department>();

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
