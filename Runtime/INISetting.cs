using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class INISetting
{
    private static UserINISetting instance;

    public static string FileName { get { return instance.FileName; } }

    public static string DEFAULT_GROUP
    {
        get { return instance.DEFAULT_GROUP; }
    }

    static INISetting()
    {
        instance = ReloadINI("settings.ini");
    }

    public static UserINISetting ReloadINI(string filename)
    {
        var instance = new UserINISetting(filename);
        return instance;
    }

    public static string GetValue(string group, string key)
    {
        return instance.GetValue(group, key);
    }
    public static T GetValue<T>(string group, string key)
    {
        return instance.GetValue<T>(group, key);
    }
    public static T GetValue<T>(string key)
    {
        return instance.GetValue<T>(key);
    }

    public static bool TryGetValue<T>(string group, string key, out T val)
    {
        return instance.TryGetValue<T>(group, key, out val);
    }
    public static T GetValueWithAdd<T>(string group, string key, T defaultValue)
    {
        return instance.GetValueWithAdd<T>(group, key, defaultValue);
    }

    public static bool TryGetValue<T>(string key, out T val)
    {
        return instance.TryGetValue<T>(key, out val);
    }
    public static T GetValueWithAdd<T>(string key, T defaultValue)
    {
        return instance.GetValueWithAdd<T>(key, defaultValue);
    }

    public static bool Exists(string group)
    {
        return instance.Exists(group);
    }

    public static bool Exists(string group, string key)
    {
        return instance.Exists(group, key);
    }

    public static void SetValue(string group, string key, object value, bool isComment = false)
    {
        instance.SetValue(group, key, value, isComment);
    }
    public static void SetValue(string key, string value, bool isComment = false)
    {
        instance.SetValue(key, value, isComment);
    }

    public static void Save()
    {
        instance.Save();
    }

    public static void Save(string fileName)
    {
        instance.Save(fileName);
    }

}

public class UserINISetting
{
    private Dictionary<GroupValue, INI_Values> groups = new Dictionary<GroupValue, INI_Values>();
    private string defaultGroup = "Default";

    public string FileName { get; private set; }

    public string DEFAULT_GROUP
    {
        get { return defaultGroup; }
    }


    public UserINISetting(string filename)
    {
#if UNITY_ANDROID
        filename = Path.Combine(UnityEngine.Application.persistentDataPath, filename);
#endif
        FileName = filename;
        if (!File.Exists(filename))
            return;
        groups.Clear();
        //#if UNITY_5
        //        UnityEngine.Debug.Log("found settings file");
        //#endif
        string[] lines = File.ReadAllLines(filename);

        GroupValue curent_group_key = defaultGroup;

        for (int i = 0; i < lines.Length; i++)
        {
            if (string.IsNullOrEmpty(lines[i])) continue;


            bool isComment = lines[i].TrimStart().StartsWith("#");
            if (isComment)
            {
                lines[i] = lines[i].Substring(1);
            }

            if (lines[i].TrimStart().StartsWith("[") && lines[i].Contains("]"))
            {
                int indx1 = lines[i].IndexOf("[") + 1;
                int indx2 = lines[i].IndexOf("]");
                curent_group_key = new GroupValue(lines[i].Substring(indx1, indx2 - indx1), isComment ? ValueType.Comments : ValueType.Value);

                if (!groups.ContainsKey(curent_group_key))
                {
                    groups.Add(curent_group_key, new INI_Values());
                }
                continue;
            }
            isComment |= curent_group_key.Type == ValueType.Comments;
            string[] ab = lines[i].Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
            string _key = ab[0].Trim().Replace("&equal;", "=");
            ValueResult vr = null;
            if (ab.Length == 2)
                vr = new ValueResult(ab[1].Trim().Replace("&equal;", "="), isComment ? ValueType.Comments : ValueType.Value);
            else vr = new ValueResult("", isComment ? ValueType.Comments : ValueType.Value);
            if (!groups.ContainsKey(curent_group_key))
                groups.Add(curent_group_key, new INI_Values());
            groups[curent_group_key][_key] = vr;
        }
    }

    public string GetValue(string group, string key)
    {
        string val;
        TryGetValue<string>(group, key, out val);
        return val;
    }
    public T GetValue<T>(string group, string key)
    {
        T val;
        TryGetValue<T>(group, key, out val);
        return val;
    }
    public T GetValue<T>(string key)
    {
        T val;
        TryGetValue<T>(defaultGroup, key, out val);
        return val;
    }

    public bool TryGetValue<T>(string group, string key, out T val)
    {
        if (groups.ContainsKey(group))
        {
            if (groups[group].ContainsKey(key))
            {
                val = groups[group][key].GetValue<T>();
                return true;
            }
            else
            {
                groups[group].Add(key, new ValueResult(default(T).ToString(), ValueType.Comments));
                Save(FileName);
            }
        }
        else
        {
            //string line = "";
            //foreach (var item_group in groups)
            //{
            //    line += string.Format("[{0}]\r\n", item_group.Key);
            //    foreach (var item in item_group.Value)
            //    {
            //        line += string.Format("{0}={1}\r\n", item.Key, item.Value.ToString());
            //    }
            //}
            //line += string.Format("#[{0}]\r\n", group);
            //line += string.Format("#{0}={1}\r\n", key, default(T));
            //File.WriteAllText(FileName, line, System.Text.Encoding.UTF8);
            groups.Add(new GroupValue(group, ValueType.Comments), new INI_Values());
            groups[group].Add(key, new ValueResult(default(T).ToString(), ValueType.Comments));
            Save(FileName);
        }

        val = default(T);
        return false;
    }
    public T GetValueWithAdd<T>(string group, string key, T defaultValue)
    {
        T val;
        if (!groups.ContainsKey(group))
        {
            //string line = "";
            //foreach (var item_group in groups)
            //{
            //    line += string.Format("[{0}]\r\n", item_group.Key);
            //    foreach (var item in item_group.Value)
            //    {
            //        line += string.Format("{0}={1}\r\n", item.Key, item.Value.ToString());
            //    }
            //}
            //line += string.Format("[{0}]\r\n", group);
            //line += string.Format("{0}={1}\r\n", key, defaultValue);
            //File.WriteAllText(FileName, line, System.Text.Encoding.UTF8);
            val = defaultValue;
            INI_Values _iv = new INI_Values();
            _iv.Add(key, new ValueResult(defaultValue.ToString()));
            groups.Add(group, _iv);
            Save(FileName);
            return val;
        }
        INI_Values iv = groups[group];
        if (!iv.ContainsKey(key) || iv[key].IsEmpty)
        {
            //#if UNITY_5
            //            UnityEngine.Debug.LogWarningFormat("Settings.ini >> key:'{0}', or value is empty", key);
            //#endif
            val = defaultValue;
            //string line = "";
            bool isExists = iv.ContainsKey(key);
            if (isExists && iv[key].IsEmpty)
                iv[key] = new ValueResult(defaultValue.ToString());
            else if (!isExists)
                iv.Add(key, new ValueResult(defaultValue.ToString()));
            //foreach (var item_group in groups)
            //{
            //    line += string.Format("[{0}]\r\n", item_group.Key);
            //    foreach (var item in item_group.Value)
            //    {
            //        line += string.Format("{0}={1}\r\n", item.Key, item.Value.ToString());
            //    }
            //}
            //File.WriteAllText(FileName, line, System.Text.Encoding.UTF8);
            Save();
        }
        else val = iv[key].GetValue<T>();
        return val;
    }

    public bool TryGetValue<T>(string key, out T val)
    {
        return TryGetValue<T>(defaultGroup, key, out val);
    }
    public T GetValueWithAdd<T>(string key, T defaultValue)
    {
        return GetValueWithAdd<T>(defaultGroup, key, defaultValue);
    }

    public bool Exists(string group)
    {
        return groups.ContainsKey(group);
    }

    public bool Exists(string group, string key)
    {
        return groups.ContainsKey(group) && groups[group].ContainsKey(key);
    }

    public void SetValue(string group, string key, object value, bool isComment = false)
    {
        if (Exists(group, key))
        {
            groups[group][key] = new ValueResult(value.ToString());
        }
        else
        {
            if (!groups.ContainsKey(group))
                groups.Add(group, new INI_Values());
            if (!groups[group].ContainsKey(key))
                groups[group].Add(key, new ValueResult(value.ToString()));
        }
    }
    public void SetValue(string key, string value, bool isComment = false)
    {
        SetValue(defaultGroup, key, value, isComment);
    }

    public void Save()
    {
        Save(FileName);
    }

    public void Save(string fileName)
    {
        UnityEngine.Debug.Log("Save INI to file: " + fileName);
        string line = "";
        foreach (var item_group in groups)
        {
            if (item_group.Key.Type == ValueType.Comments)
                line += string.Format("#{0}\r\n", item_group.Key);
            else
                line += string.Format("{0}\r\n", item_group.Key);
            foreach (var item in item_group.Value)
            {
                if (item.Value.Type == ValueType.Comments || item_group.Key.Type == ValueType.Comments)
                    line += string.Format("#{0}={1}\r\n", item.Key, item.Value.ToString());
                else
                    line += string.Format("{0}={1}\r\n", item.Key, item.Value.ToString());
            }
        }
        File.WriteAllText(fileName, line, System.Text.Encoding.UTF8);
    }

    public enum ValueType
    {
        Value = 0,
        Comments
    }
    public class GroupValue : IEqualityComparer
    {
        public string Name;
        public ValueType Type = ValueType.Value;

        public GroupValue(string name, ValueType type)
        {
            Name = name;
            Type = type;
        }

        public override int GetHashCode()
        {
            return (Name + "/" + Type).GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is GroupValue)
            {
                return (obj as GroupValue).Name == Name && (obj as GroupValue).Type == Type;
            }
            return false;
        }

        public override string ToString()
        {
            return string.Format("{0}[{1}]", Type == ValueType.Value ? "" : "#", Name);
        }

        public new bool Equals(object x, object y)
        {
            if (x is GroupValue && y is GroupValue)
            {
                return (x as GroupValue).Name == (y as GroupValue).Name && (x as GroupValue).Type == (y as GroupValue).Type;
            }
            return false;
        }

        public int GetHashCode(object obj)
        {
            return obj.GetHashCode();
        }

        public static implicit operator string(GroupValue g)
        {
            return g.ToString();
        }
        public static implicit operator GroupValue(string s)
        {
            return new GroupValue(s, ValueType.Value);
        }
    }
    public class INI_Values : Dictionary<string, ValueResult>
    { }
    public class ValueResult
    {
        static System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("en-US");
        string value;
        public ValueType Type;

        public bool IsEmpty
        {
            get { return string.IsNullOrEmpty(value); }
        }

        public ValueResult(string _value, ValueType type = ValueType.Value)
        {
            value = _value.Replace("\n", "\\n");
        }

        public T GetValue<T>()
        {
            Type returnedType = typeof(T);
            if (string.IsNullOrEmpty(value)) return default(T);
            if (returnedType == typeof(string)) return (T)(object)value.Replace("\\n", "\n");

            if (returnedType.IsEnum)
                return (T)Enum.Parse(returnedType, value);

#if UNITY_2018_1_OR_NEWER
            if (returnedType == typeof(UnityEngine.Color))
            {
                int n1 = value.IndexOf("(") + 1;
                int n2 = value.IndexOf(")");
                string val = value.Substring(n1, n2 - n1);
                string[] bytes = val.Replace("{", "").Replace("}", "").Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (bytes.Length > 2)
                {
                    float r = float.Parse(bytes[0], ci);
                    float g = float.Parse(bytes[1], ci);
                    float b = float.Parse(bytes[2], ci);
                    float a = 1;
                    if (bytes.Length == 4)
                        a = float.Parse(bytes[3], ci);
                    object col = new UnityEngine.Color(r, g, b, a);
                    return (T)col;
                }
                else return default(T);
            }
            else if (returnedType == typeof(UnityEngine.Color32))
            {
                int n1 = value.IndexOf("(") + 1;
                int n2 = value.IndexOf(")");
                string val = value.Substring(n1, n2 - n1);
                string[] bytes = val.Replace("{", "").Replace("}", "").Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (bytes.Length > 2)
                {
                    byte r = byte.Parse(bytes[0]);
                    byte g = byte.Parse(bytes[1]);
                    byte b = byte.Parse(bytes[2]);
                    byte a = 1;
                    if (bytes.Length == 4)
                        a = byte.Parse(bytes[3]);
                    object col = new UnityEngine.Color32(r, g, b, a);
                    return (T)col;
                }
                else return default(T);
            }
            else if (returnedType == typeof(UnityEngine.Vector2))
            {
                int n1 = value.IndexOf("(") + 1;
                int n2 = value.IndexOf(")");
                string val = value.Substring(n1, n2 - n1);
                string[] bytes = val.Replace("{", "").Replace("}", "").Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (bytes.Length > 1)
                {
                    float r = float.Parse(bytes[0], ci);
                    float g = float.Parse(bytes[1], ci);
                    object col = new UnityEngine.Vector2(r, g);
                    return (T)col;
                }
                else return default(T);
            }
            else if (returnedType == typeof(UnityEngine.Vector2Int))
            {
                int n1 = value.IndexOf("(") + 1;
                int n2 = value.IndexOf(")");
                string val = value.Substring(n1, n2 - n1);
                string[] bytes = val.Replace("{", "").Replace("}", "").Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (bytes.Length > 1)
                {
                    int r = int.Parse(bytes[0]);
                    int g = int.Parse(bytes[1]);
                    object col = new UnityEngine.Vector2Int(r, g);
                    return (T)col;
                }
                else return default(T);
            }
            else if (returnedType == typeof(UnityEngine.Vector3))
            {
                int n1 = value.IndexOf("(") + 1;
                int n2 = value.IndexOf(")");
                string val = value.Substring(n1, n2 - n1);
                string[] bytes = val.Replace("{", "").Replace("}", "").Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (bytes.Length > 2)
                {
                    float r = float.Parse(bytes[0], ci);
                    float g = float.Parse(bytes[1], ci);
                    float b = float.Parse(bytes[2], ci);
                    object col = new UnityEngine.Vector3(r, g, b);
                    return (T)col;
                }
                else if (bytes.Length == 2)
                {
                    float r = float.Parse(bytes[0], ci);
                    float g = float.Parse(bytes[1], ci);
                    object col = new UnityEngine.Vector3(r, g, 0);
                    return (T)col;
                }
                else return default(T);
            }
            else if (returnedType == typeof(UnityEngine.Vector3Int))
            {
                int n1 = value.IndexOf("(") + 1;
                int n2 = value.IndexOf(")");
                string val = value.Substring(n1, n2 - n1);
                string[] bytes = val.Replace("{", "").Replace("}", "").Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (bytes.Length > 2)
                {
                    int r = int.Parse(bytes[0]);
                    int g = int.Parse(bytes[1]);
                    int b = int.Parse(bytes[2]);
                    object col = new UnityEngine.Vector3Int(r, g, b);
                    return (T)col;
                }
                else return default(T);
            }
            else if (returnedType == typeof(UnityEngine.Vector4))
            {
                int n1 = value.IndexOf("(") + 1;
                int n2 = value.IndexOf(")");
                string val = value.Substring(n1, n2 - n1);
                string[] bytes = val.Replace("{", "").Replace("}", "").Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (bytes.Length > 2)
                {
                    float r = float.Parse(bytes[0], ci);
                    float g = float.Parse(bytes[1], ci);
                    float b = float.Parse(bytes[2], ci);
                    float a = 1;
                    if (bytes.Length == 4)
                        a = float.Parse(bytes[3], ci);
                    object col = new UnityEngine.Vector4(r, g, b, a);
                    return (T)col;
                }
                else return default(T);
            }
            else if (returnedType == typeof(UnityEngine.Rect))
            {
                int n1 = value.IndexOf("(") + 1;
                int n2 = value.IndexOf(")");
                string val = value.Substring(n1, n2 - n1);
                string[] bytes = val.Replace("{", "").Replace("}", "").Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (bytes.Length == 4)
                {
                    float x = float.Parse(bytes[0].Substring(bytes[0].IndexOf(":") + 1), ci);
                    float y = float.Parse(bytes[1].Substring(bytes[1].IndexOf(":") + 1), ci);
                    float w = float.Parse(bytes[2].Substring(bytes[2].IndexOf(":") + 1), ci);
                    float h = float.Parse(bytes[3].Substring(bytes[3].IndexOf(":") + 1), ci);
                    object col = new UnityEngine.Rect(x, y, w, h);
                    return (T)col;
                }
                else return default(T);
            }
            else if (returnedType == typeof(UnityEngine.RectInt))
            {
                int n1 = value.IndexOf("(") + 1;
                int n2 = value.IndexOf(")");
                string val = value.Substring(n1, n2 - n1);
                string[] bytes = val.Replace("{", "").Replace("}", "").Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (bytes.Length == 4)
                {
                    int x = int.Parse(bytes[0].Substring(bytes[0].IndexOf(":") + 1));
                    int y = int.Parse(bytes[1].Substring(bytes[1].IndexOf(":") + 1));
                    int w = int.Parse(bytes[2].Substring(bytes[2].IndexOf(":") + 1));
                    int h = int.Parse(bytes[3].Substring(bytes[3].IndexOf(":") + 1));
                    object col = new UnityEngine.RectInt(x, y, w, h);
                    return (T)col;
                }
                else return default(T);
            }
#endif
            return (T)System.Convert.ChangeType(value, returnedType, ci);
        }

        public override string ToString()
        {
            return value;
        }
    }
}
