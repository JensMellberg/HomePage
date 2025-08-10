using System.Reflection;
using System.Text;

namespace HomePage
{
    public abstract class Repository<T> where T : SaveableItem
    {
        private const string DirectoryPath = "Database";
        private string Path => DirectoryPath + "/" + FileName;
        public Dictionary<string, T> GetValues()
        {
            var result = new Dictionary<string, T>();
            if (!Directory.Exists(DirectoryPath))
            {
                Directory.CreateDirectory(DirectoryPath);
            }

            if (!File.Exists(Path))
            {
                File.Create(Path).Close();
                return [];
            }

            var lines = File.ReadAllLines(Path);
            foreach (var line in lines)
            {
                var pair = ParseFromLine(line);
                result.Add(pair.key, pair.item);
            }

            return result;
        }

        public void Delete(T value) => Delete(value.Key);

        public void Delete(string key)
        {
            var values = this.GetValues();
            values.Remove(key);
            this.SaveValues(values);
        }

        public T? TryGetValue(string key)
        {
            GetValues().TryGetValue(key, out var returnValue);
            return returnValue;
        }

        public void SaveValue(T value)
        {
            var existing = GetValues();
            if (existing.ContainsKey(value.Key))
            {
                existing[value.Key] = value;
            }
            else
            {
                existing.Add(value.Key, value);
            }

            SaveValues(existing);
        }

        public void SaveValues(Dictionary<string, T> values)
        {
            if (!Directory.Exists(DirectoryPath))
            {
                Directory.CreateDirectory(DirectoryPath);
            }

            if (!File.Exists(Path))
            {
                File.Create(Path).Close();
            }

            var lines = new List<string>();
            foreach (var pair in values)
            {
                var sb = new StringBuilder();
                sb.Append(pair.Key);
                sb.Append('{');
                foreach (var prop in typeof(T).GetProperties().Where(prop => prop.IsDefined(typeof(SavePropertyAttribute), false))) {
                    var value = prop.GetValue(pair.Value);
                    if (value == null)
                    {
                        continue;
                    }
                    sb.Append(prop.Name);
                    sb.Append(':');

                    if (prop.IsDefined(typeof(SaveAsListAttribute)) && value is IEnumerable<string> listValue)
                    {
                        sb.Append(string.Join('¤', listValue));
                    }
                    else
                    {
                        sb.Append(value.ToString().Replace("\n", "").Replace("\r", ""));
                    }

                    sb.Append('€');
                }

                sb.Append('}');
                lines.Add(sb.ToString());
            }

            File.WriteAllLines(Path, lines);
        }

        public abstract string FileName { get; }

        private (string key, T item) ParseFromLine(string line)
        {
            var item = (T)Activator.CreateInstance(typeof(T));
            var index = 0;
            var sb = new StringBuilder();
            while (line[index] != '{')
            {
                sb.Append(line[index]);
                index++;
            }

            index++;
            var key = sb.ToString();
            sb.Clear();
            item.Key = key;

            while (line[index] != '}')
            {
                while (line[index] != ':')
                {
                    sb.Append(line[index]);
                    index++;
                }

                var property = sb.ToString();
                sb.Clear();
                index++;

                while (line[index] != '€')
                {
                    sb.Append(line[index]);
                    index++;
                }

                var value = sb.ToString();
                sb.Clear();
                index++;

                var propertyInfo = typeof(T).GetProperty(property);
                if (propertyInfo?.IsDefined(typeof(SaveAsListAttribute)) == true)
                {
                    propertyInfo?.SetValue(item, value.Split('¤', StringSplitOptions.RemoveEmptyEntries).ToList(), null);
                }
                else
                {
                    propertyInfo?.SetValue(item, Convert.ChangeType(value, propertyInfo.PropertyType), null);
                }
            }

            return (key, item!);
        }
    }
}
