using System.Text.Json;

namespace Music.Services;

public class MusicRepository<T> where T : class
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };
    private static readonly object _lock = new();

    public MusicRepository(string fileName)
    {
        _filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);
        if (!File.Exists(_filePath))
            File.WriteAllText(_filePath, "[]");
    }

    private async Task<List<T>> ReadFileAsync()
    {
        lock (_lock)
        {
            using var fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            return JsonSerializer.Deserialize<List<T>>(fs) ?? new List<T>();
        }
    }

    private async Task WriteFileAsync(List<T> items)
    {
        lock (_lock)
        {
            using var fs = new FileStream(_filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            JsonSerializer.Serialize(fs, items, _jsonOptions);
        }
    }

    public async Task<List<T>> GetAllAsync() => await ReadFileAsync();

    public async Task AddAsync(T item)
    {
        var items = await ReadFileAsync();
        items.Add(item);
        await WriteFileAsync(items);
    }

    public async Task UpdateAsync(T item)
    {
        var items = await ReadFileAsync();
        var prop = typeof(T).GetProperty("Id");
        int id = (int)prop!.GetValue(item)!;
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
        items.RemoveAll(x => (int)prop.GetValue(x)! == id);
        await WriteFileAsync(items);
    }
}
