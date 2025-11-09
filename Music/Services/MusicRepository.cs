using System.Text.Json;
using System.Reflection;
using System.Threading;

namespace Music.Services
{
    public class MusicRepository<T> where T : class
    {
        private readonly string _filePath;
        private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public MusicRepository(string fileName)
        {
            _filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);
            if (!File.Exists(_filePath))
                File.WriteAllText(_filePath, "[]");
        }

        private async Task<List<T>> ReadFileAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                await Task.Delay(500); // имитация задержки чтения

                using var fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var data = await JsonSerializer.DeserializeAsync<List<T>>(fs);
                return data ?? new List<T>();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task WriteFileAsync(List<T> items)
        {
            await _semaphore.WaitAsync();
            try
            {
                await Task.Delay(500); // имитация задержки записи

                using var fs = new FileStream(_filePath, FileMode.Create, FileAccess.Write, FileShare.None);
                await JsonSerializer.SerializeAsync(fs, items, _jsonOptions);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<List<T>> GetAllAsync() => await ReadFileAsync();

        public async Task AddAsync(T item)
        {
            var items = await ReadFileAsync();

            // Централизованное управление ID
            var prop = typeof(T).GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
            if (prop != null)
            {
                int newId = items.Any() ? items.Max(x => (int)prop.GetValue(x)!) + 1 : 1;
                prop.SetValue(item, newId);
            }

            items.Add(item);
            await WriteFileAsync(items);
        }

        public async Task UpdateAsync(T item)
        {
            var items = await ReadFileAsync();
            var prop = typeof(T).GetProperty("Id");
            if (prop == null) return;

            int id = (int)prop.GetValue(item)!;
            int idx = items.FindIndex(x => (int)prop.GetValue(x)! == id);

            if (idx != -1)
            {
                items[idx] = item;
                await WriteFileAsync(items);
            }
        }

        public async Task DeleteAsync(int id)
        {
            var items = await ReadFileAsync();
            var prop = typeof(T).GetProperty("Id");
            if (prop == null) return;

            items.RemoveAll(x => (int)prop.GetValue(x)! == id);
            await WriteFileAsync(items);
        }
    }
}
