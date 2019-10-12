using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataQI.Dapper.FastCrud.Test.Repository.Sample
{
    [Table("PERSON")]
    public class Person
    {
        [Key]
        [Column("PERSON_ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("FULL_NAME")]
        public string FullName { get; set; }

        [Column("TELEPHONE")]
        public string Phone { get; set; }

        [Column("EMAIL")]
        public string Email { get; set; }

        [Column("DATE_BIRTH")]
        public DateTime DateOfBirth { get; set; }
    }
}