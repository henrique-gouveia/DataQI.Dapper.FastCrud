using System.Collections.Generic;
using DataQI.Dapper.FastCrud.Repository;

namespace DataQI.Dapper.FastCrud.Test.Repository.Employees
{
    public interface IEmployeeRepository : IDapperRepository<Employee>
    {
        void InsertDepartment(Department department);
        IEnumerable<Employee> FindByDepartmentName(string name);
    }
}
