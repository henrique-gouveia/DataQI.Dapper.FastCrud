using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataQI.Dapper.FastCrud.Benchmarks.Support;

[Table("entities")]
public class Entity
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    [Column("name")]
    public string? Name { get; set; }
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}