using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataQI.Dapper.FastCrud.Test.Repository.Employees
{
    [Table("DEPARTMENT")]
    public class Department
    {
        public Department()
        { }

        public Department(string name)
        {
            Name = name;
        }

        [Key]
        [Column("DEPARTMENT_ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Column("NAME")]
        public string Name { get; set; }
    }
}
