using SQLite;

namespace way.Models
{
    [Table("Exercises")]
    public class Exercise
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
