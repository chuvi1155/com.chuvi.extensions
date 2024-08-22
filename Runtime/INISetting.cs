using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using UnityEngine;

public class INISetting
{
    private static UserINISetting instance;
    public static Dictionary<UserINISetting.GroupValue, UserINISetting.INI_Values> Data => instance.Data;
    public static string FileName { get { return instance.FileName; } }

    public static string DEFAULT_GROUP
    {
        get { return instance.DEFAULT_GROUP; }
    }

    static INISetting()
    {
#if !UNITY_ANDROID && !UNITY_IOS
            instance = ReloadINI("settings.ini");
#else
        instance = ReloadINI(Path.Combine(Application.persistentDataPath, "settings.ini"));
#endif
#if CHUVI_EXTENSIONS && CHUVI_SETTINGS
        UnityEngine.GameObject go = UnityEngine.Resources.Load<UnityEngine.GameObject>("Settings");
        var inst = UnityEngine.GameObject.Instantiate(go);
        var contr = inst.GetComponent<ISettingsController>();
        contr.Settings = instance;
#endif
    }

    public static void ReloadINI()
    {
        instance = new UserINISetting(instance.FileName);
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

    public static void SetValue(string group, string key, object value)
    {
        instance.SetValue(group, key, value, false);
    }
    public static void SetValue(string key, string value)
    {
        instance.SetValue(key, value, false);
    }
    public static void SetValue(string key, object value)
    {
        instance.SetValue(key, value, false);
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
#if CHUVI_EXTENSIONS
        : ISettings
#endif
{
    private Dictionary<GroupValue, INI_Values> groups = new Dictionary<GroupValue, INI_Values>();
    private string defaultGroup = "Default";

    public Dictionary<GroupValue, INI_Values> Data => groups;
    public string FileName = "";

    public string DEFAULT_GROUP
    {
        get { return defaultGroup; }
    }

#if CHUVI_EXTENSIONS
    string raw;
    object ISettings.RawData => raw;
#endif


    public UserINISetting(string filename)
    {
        CultureInfo ci = new System.Globalization.CultureInfo("en-US");
        Thread.CurrentThread.CurrentCulture = ci;
        Thread.CurrentThread.CurrentUICulture = ci;

        FileName = filename;
        if (!File.Exists(filename)) return;
        groups.Clear();

        string[] lines = File.ReadAllLines(filename);
#if CHUVI_EXTENSIONS
        raw = string.Join("\n", lines); 
#endif

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
            /*else
            {
                groups[group].Add(key, new ValueResult(default(T).ToString(), ValueType.Comments));
                Save(FileName);
            }*/
        }
        else
        {
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
            val = defaultValue;
            INI_Values _iv = new INI_Values();
            _iv.Add(key, new ValueResult(defaultValue.ToString()));
            groups.Add(group, _iv);
            if (typeof(T).IsEnum && !groups.ContainsKey("ENUM:" + typeof(T).Name))
            {
                INI_Values _iv2 = new INI_Values();
                string[] names = Enum.GetNames(typeof(T));
                for (int i = 0; i < names.Length; i++)
                    _iv2.Add("names[" + i + "]", new ValueResult(names[i]));
                groups.Add("ENUM:" + typeof(T).Name, _iv2);
            }
            Save(FileName);
            return val;
        }
        INI_Values iv = groups[group];
        if (!iv.ContainsKey(key) || iv[key].IsEmpty)
        {
            val = defaultValue;
            bool isExists = iv.ContainsKey(key);
            if (isExists && iv[key].IsEmpty)
                iv[key] = new ValueResult(defaultValue.ToString());
            else if (!isExists)
                iv.Add(key, new ValueResult(defaultValue.ToString()));

            if (typeof(T).IsEnum && !groups.ContainsKey("ENUM:" + typeof(T).Name))
            {
                INI_Values _iv2 = new INI_Values();
                string[] names = Enum.GetNames(typeof(T));
                for (int i = 0; i < names.Length; i++)
                    _iv2.Add("names[" + i + "]", new ValueResult(names[i]));
                groups.Add("ENUM:" + typeof(T).Name, _iv2);
            }

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
        if (!groups.ContainsKey(group))
            return false;
        bool isComment = false;
        foreach (var g in groups.Keys)
        {
            if (g.Name == group)
            {
                if (g.Type == ValueType.Value && isComment)
                {
                    isComment = false;
                    break;
                }
                else if (g.Type == ValueType.Comments)
                {
                    isComment = true;
                } 
            }
        }
        return !isComment;
        //return groups.ContainsKey(group);
    }

    public bool Exists(string group, string key)
    {
        if(!groups.ContainsKey(group))
            return false;
        if(!groups[group].ContainsKey(key))
            return false;
        var k = groups[group][key];
        return k.Type == ValueType.Value;
        //return groups.ContainsKey(group) && groups[group].ContainsKey(key);
    }

    public void SetValue(string group, string key, object value, bool isComment)
    {
        if (value == null)
            value = "";
        if (Exists(group, key))
        {
            if (!(value is float))
                groups[group][key] = new ValueResult(value.ToString(), isComment ? ValueType.Comments : ValueType.Value);
            else
                groups[group][key] = new ValueResult(value.ToString().Replace(",", "."), isComment ? ValueType.Comments : ValueType.Value);
        }
        else
        {
            if (!groups.ContainsKey(group))
                groups.Add(group, new INI_Values());
            if (!groups[group].ContainsKey(key))
            {
                if (!(value is float)) groups[group].Add(key, new ValueResult(value.ToString(), isComment ? ValueType.Comments : ValueType.Value));
                else
                    groups[group].Add(key, new ValueResult(value.ToString().Replace(",", "."), isComment ? ValueType.Comments : ValueType.Value));
            }
        }
        if (value.GetType().IsEnum && !groups.ContainsKey("ENUM:" + value.GetType().Name))
        {
            INI_Values _iv2 = new INI_Values();
            string[] names = Enum.GetNames(value.GetType());
            for (int i = 0; i < names.Length; i++)
                _iv2.Add("names[" + i + "]", new ValueResult(names[i], isComment ? ValueType.Comments : ValueType.Value));
            groups.Add("ENUM:" + value.GetType().Name, _iv2);
        }
    }
    public void SetValue(string key, string value, bool isComment)
    {
        SetValue(defaultGroup, key, value, isComment);
    }
    public void SetValue(string key, object value, bool isComment)
    {
        SetValue(defaultGroup, key, value, isComment);
    }

    public void Save()
    {
        Save(FileName);
    }

    public void Save(string fileName)
    {
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
                {
                    List<object> list = new List<object>();
                    if(!string.IsNullOrEmpty(item.Key))
                        list.Add(item.Key);
                    if (!string.IsNullOrEmpty(item.Value.ToString()))
                        list.Add(item.Value.ToString());
                    var comment = string.Join("=", list);
                    line += string.Format("#{0}\r\n", comment);
                }
                else
                    line += string.Format("{0}={1}\r\n", item.Key, item.Value.ToString());
            }
        }
#if CHUVI_EXTENSIONS
        raw = line;
#endif
        File.WriteAllText(fileName, line, System.Text.Encoding.UTF8);
    }

    public override string ToString()
    {
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
        return line;
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
        string value;
        public ValueType Type;

        public bool IsEmpty
        {
            get { return string.IsNullOrEmpty(value); }
        }

        public ValueResult(string _value, ValueType type = ValueType.Value)
        {
            Type = type;
            value = _value.Replace("\n", "\\n");
        }

        public T GetValue<T>()
        {
            if (string.IsNullOrEmpty(value)) return default(T);
            if (typeof(T) == typeof(string)) return (T)(object)value.Replace("\\n", "\n");

            if (typeof(T).IsEnum)
                return (T)Enum.Parse(typeof(T), value);

#if UNITY_5 || UNITY_STANDALONE_WIN
            if (typeof(T) == typeof(UnityEngine.Color))
            {
                int n1 = value.IndexOf("(") + 1;
                int n2 = value.IndexOf(")");
                string val = value.Substring(n1, n2 - n1);
                string[] bytes = val.Replace("{", "").Replace("}", "").Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (bytes.Length > 2)
                {
                    float r = float.Parse(bytes[0], new CultureInfo("en-US"));
                    float g = float.Parse(bytes[1], new CultureInfo("en-US"));
                    float b = float.Parse(bytes[2], new CultureInfo("en-US"));
                    float a = 1;
                    if (bytes.Length == 4)
                        a = float.Parse(bytes[3]);
                    object col = new UnityEngine.Color(r, g, b, a);
                    return (T)col;
                }
                else return default(T);
            }
            else if (typeof(T) == typeof(UnityEngine.Vector2))
            {
                int n1 = value.IndexOf("(") + 1;
                int n2 = value.IndexOf(")");
                string val = value.Substring(n1, n2 - n1);
                string[] bytes = val.Replace("{", "").Replace("}", "").Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (bytes.Length > 1)
                {
                    float r = float.Parse(bytes[0], new CultureInfo("en-US"));
                    float g = float.Parse(bytes[1], new CultureInfo("en-US"));
                    object col = new UnityEngine.Vector2(r, g);
                    return (T)col;
                }
                else return default(T);
            }
            else if (typeof(T) == typeof(UnityEngine.Vector2Int))
            {
                int n1 = value.IndexOf("(") + 1;
                int n2 = value.IndexOf(")");
                string val = value.Substring(n1, n2 - n1);
                string[] bytes = val.Replace("{", "").Replace("}", "").Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (bytes.Length > 1)
                {
                    int r = int.Parse(bytes[0], new CultureInfo("en-US"));
                    int g = int.Parse(bytes[1], new CultureInfo("en-US"));
                    object col = new UnityEngine.Vector2Int(r, g);
                    return (T)col;
                }
                else return default(T);
            }
            else if (typeof(T) == typeof(UnityEngine.Vector3))
            {
                int n1 = value.IndexOf("(") + 1;
                int n2 = value.IndexOf(")");
                string val = value.Substring(n1, n2 - n1);
                string[] bytes = val.Replace("{", "").Replace("}", "").Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (bytes.Length > 2)
                {
                    float r = float.Parse(bytes[0], new CultureInfo("en-US"));
                    float g = float.Parse(bytes[1], new CultureInfo("en-US"));
                    float b = float.Parse(bytes[2], new CultureInfo("en-US"));
                    object col = new UnityEngine.Vector3(r, g, b);
                    return (T)col;
                }
                else return default(T);
            }
            else if (typeof(T) == typeof(UnityEngine.Vector3Int))
            {
                int n1 = value.IndexOf("(") + 1;
                int n2 = value.IndexOf(")");
                string val = value.Substring(n1, n2 - n1);
                string[] bytes = val.Replace("{", "").Replace("}", "").Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (bytes.Length > 2)
                {
                    int r = int.Parse(bytes[0], new CultureInfo("en-US"));
                    int g = int.Parse(bytes[1], new CultureInfo("en-US"));
                    int b = int.Parse(bytes[2], new CultureInfo("en-US"));
                    object col = new UnityEngine.Vector3Int(r, g, b);
                    return (T)col;
                }
                else return default(T);
            }
            else if (typeof(T) == typeof(UnityEngine.Vector4))
            {
                int n1 = value.IndexOf("(") + 1;
                int n2 = value.IndexOf(")");
                string val = value.Substring(n1, n2 - n1);
                string[] bytes = val.Replace("{", "").Replace("}", "").Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (bytes.Length > 2)
                {
                    float r = float.Parse(bytes[0], new CultureInfo("en-US"));
                    float g = float.Parse(bytes[1], new CultureInfo("en-US"));
                    float b = float.Parse(bytes[2], new CultureInfo("en-US"));
                    float a = 1;
                    if (bytes.Length == 4)
                        a = float.Parse(bytes[3]);
                    object col = new UnityEngine.Vector4(r, g, b, a);
                    return (T)col;
                }
                else return default(T);
            }
            else if (typeof(T) == typeof(UnityEngine.Rect))
            {
                int n1 = value.IndexOf("(") + 1;
                int n2 = value.IndexOf(")");
                string val = value.Substring(n1, n2 - n1);
                string[] bytes = val.Replace("{", "").Replace("}", "").Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (bytes.Length == 4)
                {
                    float x = float.Parse(bytes[0].Substring(bytes[0].IndexOf(":") + 1), new CultureInfo("en-US"));
                    float y = float.Parse(bytes[1].Substring(bytes[1].IndexOf(":") + 1), new CultureInfo("en-US"));
                    float w = float.Parse(bytes[2].Substring(bytes[2].IndexOf(":") + 1), new CultureInfo("en-US"));
                    float h = float.Parse(bytes[3].Substring(bytes[3].IndexOf(":") + 1), new CultureInfo("en-US"));
                    object col = new UnityEngine.Rect(x, y, w, h);
                    return (T)col;
                }
                else return default(T);
            }
            else if (typeof(T) == typeof(UnityEngine.RectInt))
            {
                int n1 = value.IndexOf("(") + 1;
                int n2 = value.IndexOf(")");
                string val = value.Substring(n1, n2 - n1);
                string[] bytes = val.Replace("{", "").Replace("}", "").Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (bytes.Length == 4)
                {
                    int x = int.Parse(bytes[0].Substring(bytes[0].IndexOf(":") + 1), new CultureInfo("en-US"));
                    int y = int.Parse(bytes[1].Substring(bytes[1].IndexOf(":") + 1), new CultureInfo("en-US"));
                    int w = int.Parse(bytes[2].Substring(bytes[2].IndexOf(":") + 1), new CultureInfo("en-US"));
                    int h = int.Parse(bytes[3].Substring(bytes[3].IndexOf(":") + 1), new CultureInfo("en-US"));
                    object col = new UnityEngine.RectInt(x, y, w, h);
                    return (T)col;
                }
                else return default(T);
            }
#endif

            if (typeof(T) == typeof(float))
                return (T)System.Convert.ChangeType(value.Replace(",", "."), typeof(T), new CultureInfo("en-US"));
            return (T)System.Convert.ChangeType(value, typeof(T), new CultureInfo("en-US"));
        }

        public override string ToString()
        {
            return value;
        }
    }

#if CHUVI_EXTENSIONS
    ISettingsData[] ISettings.GetData()
    {
        List<ISettingsData> data = new List<ISettingsData>();
        foreach (var group in groups)
        {
            foreach (var keyVal in group.Value)
            {
                if (string.IsNullOrEmpty(keyVal.Key))
                    continue;

                INIData idata = new INIData(group.Key.Name, keyVal.Key, this);
                data.Add(idata);
            }
        }
        return data.ToArray();
    }

    private class INIData : ISettingsData
    {
        string group, key;
        UserINISetting settings;
        string ISettingsData.Group => group;
        string ISettingsData.Key => key;

        public INIData(string _group, string _key, UserINISetting _settings)
        {
            group = _group;
            key = _key;
            settings = _settings;
        }

        T ISettingsData.GetData<T>()
        {
            return settings.GetValue<T>(group, key);
        }
        Type ISettingsData.GetDataType()
        {
            var val = settings.GetValue(group, key);
            if(val.ToLower() == "true" || val.ToLower() == "false")
                return typeof(bool);
            return typeof(string);
        }

        void ISettingsData.SetData<T>(T value)
        {
            settings.SetValue(group, key, value, false);
        }
    }
#endif
}
