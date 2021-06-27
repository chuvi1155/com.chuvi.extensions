using System;
using System.Collections.Generic;
public static class StringExtentions
{
    public static string After(this string s, string value)
    {
        if (string.IsNullOrEmpty(s)) return "";
        if (s.Contains(value))
            return s.Substring(s.IndexOf(value) + value.Length);
        return "";
    }
    public static string AfterLast(this string s, string value)
    {
        if (string.IsNullOrEmpty(s)) return "";
        if (s.Contains(value))
            return s.Substring(s.LastIndexOf(value) + value.Length);
        return "";
    }

    public static bool TryAfter(this string s, string value, out string result)
    {
        result = "";
        if (string.IsNullOrEmpty(s)) return false;
        if (s.Contains(value))
        {
            result = s.Substring(s.IndexOf(value) + value.Length);
            return true;
        }
        return false;
    }
    public static bool TryAfterLast(this string s, string value, out string result)
    {
        result = "";
        if (string.IsNullOrEmpty(s)) return false;
        if (s.Contains(value))
        {
            result = s.Substring(s.LastIndexOf(value) + value.Length);
            return true;
        }
        return false;
    }

    public static string Before(this string s, string value)
    {
        if (string.IsNullOrEmpty(s)) return "";
        if (s.Contains(value))
            return s.Substring(0, s.IndexOf(value));
        return "";
    }
    public static string BeforeLast(this string s, string value)
    {
        if (string.IsNullOrEmpty(s)) return "";
        if (s.Contains(value))
            return s.Substring(0, s.LastIndexOf(value));
        return "";
    }

    public static bool TryBefore(this string s, string value, out string result)
    {
        result = "";
        if (string.IsNullOrEmpty(s)) return false;
        if (s.Contains(value))
        {
            result = s.Substring(0, s.IndexOf(value));
            return true;
        }
        return false;
    }
    public static bool TryBeforeLast(this string s, string value, out string result)
    {
        result = "";
        if (string.IsNullOrEmpty(s))
            return false;
        if (s.Contains(value))
        {
            result = s.Substring(0, s.LastIndexOf(value));
            return true;
        }
        return false;
    }
    /// <summary>
    /// разбивает строки по указанному разделителю и удаляет пустые строки
    /// </summary>
    /// <param name="s"></param>
    /// <param name="separator"></param>
    /// <returns></returns>
    public static string[] Split(this string s, string separator)
    {
        if (string.IsNullOrEmpty(s)) return new string[0];
        return s.Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries);
    }
    /// <summary>
    /// удаляет пробелы вначале строки и в конце
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static string TrimWhitespaceFrontEnd(this string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        int indx = 0;
        while (indx < s.Length && s[indx] == ' ') indx++;
        s = s.Substring(indx);
        indx = s.Length;
        while (indx > 0 && s[indx - 1] == ' ') indx--;
        return s.Substring(0, indx);
    }

    public static int IndexOfAfter(this string s, string value)
    {
        if (string.IsNullOrEmpty(s)) return -1;
        if (s.Contains(value))
            return s.IndexOf(value) + value.Length;
        return -1;
    }
    public static int IndexOfAfterLast(this string s, string value)
    {
        if (string.IsNullOrEmpty(s)) return -1;
        if (s.Contains(value))
            return s.LastIndexOf(value) + value.Length;
        return -1;
    }
    public static int IndexOfBefore(this string s, string value)
    {
        if (string.IsNullOrEmpty(s)) return -1;
        if (s.Contains(value))
            return s.IndexOf(value);
        return -1;
    }
    public static int IndexOfBeforeLast(this string s, string value)
    {
        if (string.IsNullOrEmpty(s)) return -1;
        if (s.Contains(value))
            return s.LastIndexOf(value);
        return -1;
    }

    public static int ToInt(this string s)
    {
        int res;
        int.TryParse(s, out res);
        return res;
    }
    public static float ToFloat(this string s)
    {
        float res;
        float.TryParse(s, out res);
        return res;
    }
    public static double ToDouble(this string s)
    {
        double res;
        double.TryParse(s, out res);
        return res;
    }
    /// <summary>
    /// перемешать список
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    public static void Shuffle<T>(this IList<T> list)
    {
        Random random = new Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}
