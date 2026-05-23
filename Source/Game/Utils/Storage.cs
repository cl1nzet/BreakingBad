using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;

namespace Game.Utils {
    public class Storage {
        private string _filePath;
        private string _backupPath;
        private readonly Dictionary<string, JsonElement> _rawCache = new(StringComparer.Ordinal);
        private readonly Dictionary<string, object> _typedCache = new(StringComparer.Ordinal);
        private readonly ReaderWriterLockSlim _lock = new();
        private bool _isDirty;
        private bool _isInitialized;

        public void Initialize(string filePath) {
            _lock.EnterWriteLock();
            try
            {
                if (_isInitialized) return;
                _filePath = Path.GetFullPath(filePath);
                _backupPath = _filePath + ".bak";
                _isInitialized = true;
                LoadInternal();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public T Get<T>(string key, T defaultValue = default)
        {
            if (!_isInitialized) throw new InvalidOperationException("System is not initialized. Call Initialize() first.");

            _lock.EnterUpgradeableReadLock();
            try
            {
                if (_typedCache.TryGetValue(key, out var typedValue))
                {
                    return (T)typedValue;
                }

                if (_rawCache.TryGetValue(key, out var jsonElement))
                {
                    _lock.EnterWriteLock();
                    try
                    {
                        if (_typedCache.TryGetValue(key, out typedValue))
                        {
                            return (T)typedValue;
                        }

                        T deserialized = JsonSerializer.Deserialize<T>(jsonElement);
                        _typedCache[key] = deserialized;
                        _rawCache.Remove(key);
                        return deserialized;
                    }
                    finally
                    {
                        _lock.ExitWriteLock();
                    }
                }

                return defaultValue;
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        public void Set<T>(string key, T value)
        {
            if (!_isInitialized) throw new InvalidOperationException("System is not initialized. Call Initialize() first.");

            _lock.EnterWriteLock();
            try
            {
                if (_typedCache.TryGetValue(key, out var existing) && EqualityComparer<T>.Default.Equals((T)existing, value))
                {
                    return;
                }

                _typedCache[key] = value;
                _rawCache.Remove(key);
                _isDirty = true;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Load() {
            if (!_isInitialized) throw new InvalidOperationException("System is not initialized. Call Initialize() first.");

            _lock.EnterWriteLock();
            try
            {
                LoadInternal();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        private void LoadInternal() {
            _rawCache.Clear();
            _typedCache.Clear();
            _isDirty = false;

            if (!File.Exists(_filePath))
            {
                if (File.Exists(_backupPath))
                {
                    File.Copy(_backupPath, _filePath, true);
                }
                else
                {
                    return;
                }
            }

            try
            {
                ReadFromFile(_filePath);
            }
            catch
            {
                try
                {
                    if (File.Exists(_backupPath))
                    {
                        ReadFromFile(_backupPath);
                    }
                }
                catch
                {
                    _rawCache.Clear();
                    _typedCache.Clear();
                }
            }
        }

        private void ReadFromFile(string path)
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 8192, FileOptions.SequentialScan);
            if (stream.Length == 0) return;

            using var document = JsonDocument.Parse(stream);
            foreach (var property in document.RootElement.EnumerateObject())
            {
                _rawCache[property.Name] = property.Value.Clone();
            }
        }

        public void Save()
        {
            if (!_isInitialized) throw new InvalidOperationException("System is not initialized. Call Initialize() first.");

            _lock.EnterWriteLock();
            try
            {
                if (!_isDirty) return;

                var directory = Path.GetDirectoryName(_filePath);
                if (directory != null && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string tempPath = _filePath + ".tmp";

                using (var stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, FileOptions.WriteThrough))
                using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = false }))
                {
                    writer.WriteStartObject();

                    foreach (var kvp in _rawCache)
                    {
                        writer.WritePropertyName(kvp.Key);
                        kvp.Value.WriteTo(writer);
                    }

                    foreach (var kvp in _typedCache)
                    {
                        writer.WritePropertyName(kvp.Key);
                        JsonSerializer.Serialize(writer, kvp.Value);
                    }

                    writer.WriteEndObject();
                    writer.Flush();
                }

                if (File.Exists(_filePath))
                {
                    File.Replace(tempPath, _filePath, _backupPath);
                }
                else
                {
                    if (File.Exists(_backupPath)) File.Delete(_backupPath);
                    File.Move(tempPath, _filePath);
                }

                _isDirty = false;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }
}