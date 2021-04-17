using System.Collections.Generic;
using System.Data;
using Dapper.FastCrud;
using DataQI.Commons.Util;
using DataQI.Dapper.FastCrud.Repository.Support;

namespace DataQI.Dapper.FastCrud.Test.Repository.Employees
{
    public class EmployeeRepository : DapperRepository<Employee>
    {
        public EmployeeRepository(IDbConnection connection) : base(connection)
        { }

        public void InsertDepartment(Department department)
        {
            Assert.NotNull(department, "Department must not be null");
            connection.Insert(department);
        }

        public IEnumerable<Employee> FindByDepartmentName(string name)
        {
            var employees = connection.Find<Employee>(statement => statement
                .Include<Department>(join => join
                    .LeftOuterJoin()
                    .Where($"{nameof(Department.Name):TC} = @name"))
                .WithParameters(new { name }));

            return employees;
        }
    }
}
