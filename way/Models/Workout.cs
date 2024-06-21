using SQLite;
using System.Text.Json;

namespace way.Models
{
    [Table("Workouts")]
    public class Workout
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int ExerciseId { get; set; }
        public int TrainingId { get; set; }
        public string Sets { get; set; } = string.Empty;
        public int CountSets { get; set; }
        public int TimeRest { get; set; }

        public List<OperatingSet>? GetSets()
        {
            if (Sets == string.Empty)
                return null;
            return JsonSerializer.Deserialize<List<OperatingSet>>(Sets);
        }
        public void SetSets(List<OperatingSet> sets)
        {
            Sets = JsonSerializer.Serialize(sets);
        }
    }
}
