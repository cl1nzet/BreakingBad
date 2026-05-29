using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.Xna.Framework.Input;

namespace Game.Utils
{
    public sealed class Storage
    {
        private string _configPath;
        private readonly Dictionary<string, string> _stringSettings = new(32, StringComparer.Ordinal);
        private readonly Dictionary<string, object> _typedCache = new(32, StringComparer.Ordinal);
        private readonly object _lock = new();

        public event Action onFileCreated;

        public void Initialize(string path)
        {
            _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
            lock (_lock)
            {
                if (File.Exists(_configPath))
                {
                    LoadInternal();
                }
                else
                {
                    SaveInternal();
                    onFileCreated?.Invoke();
                }
            }
        }

        public T Get<T>(string key, T defaultValue = default)
        {
            lock (_lock)
            {
                if (_typedCache.TryGetValue(key, out var cachedValue))
                {
                    return (T)cachedValue;
                }

                if (!_stringSettings.TryGetValue(key, out var stringValue))
                {
                    _typedCache[key] = defaultValue;
                    _stringSettings[key] = defaultValue?.ToString() ?? string.Empty;
                    return defaultValue;
                }

                T parsedValue = ParseValue<T>(stringValue);
                _typedCache[key] = parsedValue;
                return parsedValue;
            }
        }

        public void Set(string key, string value)
        {
            lock (_lock)
            {
                _stringSettings[key] = value;
                _typedCache.Remove(key);
            }
        }

        public void Load()
        {
            lock (_lock)
            {
                LoadInternal();
            }
        }

        private void LoadInternal()
        {
            _stringSettings.Clear();
            _typedCache.Clear();

            string content = File.ReadAllText(_configPath);
            ReadOnlySpan<char> textSpan = content.AsSpan();

            int position = 0;
            while (position < textSpan.Length)
            {
                int nextNewLine = textSpan[position..].IndexOf('\n');
                ReadOnlySpan<char> line = nextNewLine == -1
                    ? textSpan[position..]
                    : textSpan[position..(position + nextNewLine)];

                position += nextNewLine == -1 ? textSpan.Length : nextNewLine + 1;

                if (line.IsWhiteSpace()) continue;

                int separator = line.IndexOf('=');
                if (separator <= 0) continue;

                ReadOnlySpan<char> keySpan = line[..separator].Trim();
                ReadOnlySpan<char> valueSpan = line[(separator + 1)..].Trim();

                if (keySpan.IsEmpty || valueSpan.IsEmpty) continue;

                _stringSettings[keySpan.ToString()] = valueSpan.ToString();
            }
        }

        public void Save()
        {
            lock (_lock)
            {
                SaveInternal();
            }
        }

        private void SaveInternal()
        {
            string tempPath = _configPath + ".tmp";

            using (var writer = new StreamWriter(tempPath, false, System.Text.Encoding.UTF8, 4096))
            {
                foreach (var kvp in _stringSettings)
                {
                    writer.Write(kvp.Key);
                    writer.Write('=');
                    writer.WriteLine(kvp.Value);
                }
            }

            if (File.Exists(_configPath))
            {
                File.Replace(tempPath, _configPath, null);
            }
            else
            {
                File.Move(tempPath, _configPath);
            }
        }

        private static T ParseValue<T>(string value)
        {
            Type type = typeof(T);

            if (type == typeof(int))
                return (T)(object)int.Parse(value, CultureInfo.InvariantCulture);

            if (type == typeof(float))
                return (T)(object)float.Parse(value, CultureInfo.InvariantCulture);

            if (type == typeof(bool))
                return (T)(object)bool.Parse(value);

            if (type == typeof(Keys))
                return (T)Enum.Parse(typeof(Keys), value);

            return (T)(object)value;
        }
    }
}