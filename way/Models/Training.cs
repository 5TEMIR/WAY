using SQLite;

namespace way.Models
{
    [Table("Trainings")]
    public class Training
    {
        [PrimaryKey,AutoIncrement]
        public int Id { get; set; }
        public DateTime TrainingDate { get; set; }
    }
}
