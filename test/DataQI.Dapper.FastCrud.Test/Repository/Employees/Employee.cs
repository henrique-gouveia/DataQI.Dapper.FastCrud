using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataQI.Dapper.FastCrud.Test.Repository.Employees
{
    [Table("EMPLOYEE")]
    public class Employee
    {
        [Key]
        [Column("EMPLOYEE_ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("LAST_NAME")]
        public string LastName { get; set; }
        [Column("FIRST_NAME")]
        public string FirstName { get; set; }
        [Column("TITLE")]
        public string Title { get; set; }

        [ForeignKey(nameof(Department))]
        [Column("DEPARTMENT_ID")]
        public int? DepartmentId { get; set; }
        public Department Department { get; set; }

        [Column("BIRTH_DATE")]
        public DateTime BirthDate { get; set; }
        [Column("HIRE_DATE")]
        public DateTime HireDate { get; set; }

        [Column("PHONE_NUMBER")]
        public string Phone { get; set; }
        [Column("EMAIL")]
        public string Email { get; set; }
    }
}
