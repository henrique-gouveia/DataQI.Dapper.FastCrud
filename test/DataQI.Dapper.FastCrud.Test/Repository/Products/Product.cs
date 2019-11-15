using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataQI.Dapper.FastCrud.Test.Repository.Products
{
    [Table("PRODUCT")]
    public class Product
    {
        [Key]
        [Column("PRODUCT_ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("ACTIVE")]
        public bool Active { get; set; }

        [Column("EAN")]
        public string Ean { get; set; }

        [Column("REFERENCE")]
        public string Reference { get; set; }

        [Column("NAME")]
        public string Name { get; set; }

        [Column("DEPARTMENT")]
        public string Department { get; set; }

        [Column("PRICE")]
        public decimal Price { get; set; }

        [Column("LIST_PRICE")]
        public decimal ListPrice { get; set; }

        [Column("KEYWORDS")]
        public string Keywords { get; set; }

        [Column("STOCK")]
        public decimal Stock { get; set; }

        [Column("DATE_REGISTER")]
        public DateTime DateRegister { get; set; }
    }
}