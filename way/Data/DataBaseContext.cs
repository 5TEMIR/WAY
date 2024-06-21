using SQLite;
using way.Models;

namespace way.Data
{
    public class DataBaseContext
    {
        private SQLiteAsyncConnection DataBase;

        private const string DatabaseFilename = "LocalDataBase.db3";
        private static string DatabasePath => Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);
        private const SQLiteOpenFlags Flags = SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache;

        public async Task Init()
        {
            if (DataBase is not null)
                return;

            DataBase = new SQLiteAsyncConnection(DatabasePath, Flags);
            await DataBase.CreateTableAsync<Training>();
            await DataBase.CreateTableAsync<Exercise>();
            await DataBase.ExecuteAsync("CREATE TABLE IF NOT EXISTS Workouts " +
                "(" +
                "Id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                "ExerciseId INTEGER, " +
                "TrainingId INTEGER, " +
                "Sets TEXT, " +
                "CountSets INTEGER, " +
                "TimeRest INTEGER, " +
                "FOREIGN KEY(ExerciseId) REFERENCES Exercises(Id), " +
                "FOREIGN KEY(TrainingId) REFERENCES Trainings(Id)" +
                ");");
        }

        public async Task SaveExerciseAsync(Exercise exercise)
        {
            await Init();
            if (await DataBase.Table<Exercise>().Where(x => x.Name == exercise.Name).CountAsync() == 0)
                await DataBase.InsertAsync(exercise);
        }

        public async Task<int> GetExerciseIdByNameAsync(string name)
        {
            await Init();
            await SaveExerciseAsync(new() { Name = name });
            Exercise tempexercise = await DataBase.Table<Exercise>().Where(x => x.Name == name).FirstAsync();
            return tempexercise.Id;
        }

        public async Task<int> SaveTrainingAsync(Training training)
        {
            await Init();
            if (await DataBase.Table<Training>().Where(x => x.TrainingDate == training.TrainingDate).CountAsync() == 0)
                await DataBase.InsertAsync(training);
            Training temptraining = await DataBase.Table<Training>().Where(x => x.TrainingDate == training.TrainingDate).FirstAsync();
            return temptraining.Id;
        }

        public async Task<int> DeleteTrainingByIdAsync(int id)
        {
            await Init();
            return await DataBase.DeleteAsync<Training>(id);
        }

        public async Task<int> DeleteWorkoutsByTrainingIdAsync(int id)
        {
            await Init();
            return await DataBase.Table<Workout>().Where(x => x.TrainingId == id).DeleteAsync();
        }

        public async Task SaveWorkoutsAsync(List<Workout> workouts)
        {
            await Init();
            await DataBase.InsertAllAsync(workouts);
        }

        public async Task<List<CurrentWorkout>> GetTrainingsByCountAsync(int skip_count, int take_count)
        {
            await Init();
            return await DataBase.QueryAsync<CurrentWorkout>("SELECT Trainings.Id as TrainingId, Trainings.TrainingDate, Exercises.Name as ExerciseName, Workouts.Sets, Workouts.CountSets, Workouts.TimeRest " +
                                                              "FROM ( " +
                                                              "SELECT * FROM Trainings ORDER BY TrainingDate DESC " +
                                                              "LIMIT ? OFFSET ? " +
                                                              ") as Trainings " +
                                                              "JOIN Workouts ON Workouts.TrainingId = Trainings.Id " +
                                                              "JOIN Exercises ON Workouts.ExerciseId = Exercises.Id " +
                                                              "ORDER BY Trainings.TrainingDate DESC;", take_count, skip_count);
        }

        public async Task<List<int>> GetCounts()
        {
            await Init();
            int w = await DataBase.Table<Workout>().CountAsync();
            int t = await DataBase.Table<Training>().CountAsync();
            int e = await DataBase.Table<Exercise>().CountAsync();
            return [t, w, e];
        }

        public async Task<int> GetTrainingsCountAsync()
        {
            return await DataBase.Table<Training>().CountAsync();
        }

        public async Task<List<Exercise>> GetExercisesAsync()
        {
            return await DataBase.Table<Exercise>().OrderBy(x => x.Name).ToListAsync();
        }

        public async Task<List<CurrentWorkout>> GetWorkoutsByExerciseIdAsync(int exerciseid)
        {
            return await DataBase.QueryAsync<CurrentWorkout>("SELECT Trainings.Id as TrainingId, Trainings.TrainingDate, Exercises.Name as ExerciseName, Workouts.Sets, Workouts.CountSets, Workouts.TimeRest " +
                                                              "FROM ( " +
                                                              "SELECT * FROM Workouts WHERE ExerciseId = ? " +
                                                              ") as Workouts " +
                                                              "JOIN Trainings ON Workouts.TrainingId = Trainings.Id " +
                                                              "JOIN Exercises ON Workouts.ExerciseId = Exercises.Id " +
                                                              "ORDER BY Trainings.TrainingDate;", exerciseid);
        }

        //public async Task<List<Training>> GetTrainingsByCountAsync(int skip_count, int take_count)
        //{
        //    await Init();
        //    return await DataBase.Table<Training>().OrderByDescending(x => x.Date).Skip(skip_count).Take(take_count).ToListAsync();
        //}
    }
}
