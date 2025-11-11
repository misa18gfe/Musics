using System.Text.Json;
using System.Reflection;

namespace Music.Services
{
    public class MusicRepository<T> where T : class
    {
        private readonly string _filePath;
        private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

        public MusicRepository(string fileName)
        {
            _filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);
            if (!File.Exists(_filePath))
                File.WriteAllText(_filePath, "[]");
        }

        public async Task<List<T>> GetAllAsync()
        {
            using var fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var data = await JsonSerializer.DeserializeAsync<List<T>>(fs);
            return data ?? new List<T>();
        }

        public async Task SaveAllAsync(List<T> items)
        {
            using var fs = new FileStream(_filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            await JsonSerializer.SerializeAsync(fs, items, _jsonOptions);
        }
    }
}