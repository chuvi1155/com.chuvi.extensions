using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class CSVReader
{
    [SerializeField] Row[] rows;
    public Row[] Rows { get => rows; }

    public CSVReader() 
    {
        this.rows = new Row[0];
    }
    public CSVReader(string csv, char separator = ',')
    {
        var lines = csv.Split("\r\n");//.Skip(1);
        List<Row> rows = new List<Row>();
        foreach (var line in lines)
        {
            if(string.IsNullOrEmpty(line.Trim(' '))) continue;
            var row = new Row(line, separator);
            if (!row.HasError)
                rows.Add(row);
        }
        this.rows = rows.ToArray();
    }

    public void AddRow(Row row)
    {
        if(rows != null && rows.Length > 0)
        {
            if (rows[0].Collumns.Length != row.Collumns.Length)
                throw new System.Exception("Length of Columns array must be equal existed");
        }
        if(rows == null) rows = new Row[1];
        else System.Array.Resize(ref rows, rows.Length + 1);
#if NET_STANDARD_2_1
        rows[^1] = col; 
#else
        rows[rows.Length - 1] = row;
#endif
    }

    public string ToCSVString(char separator = ',')
    {
        return string.Join("\r\n", System.Array.ConvertAll(rows, row => row.ToCSVString(separator)));
    }

    [System.Serializable]
    public class Row
    {
        [SerializeField] private Collumn[] collumns;
        public readonly bool HasError;
        public Collumn[] Collumns { get => collumns; }

        public Row()
        {
            collumns = new Collumn[0];
        }
        internal Row(string line, char separator = ',')
        {
            var cols = line.Split(separator);
            List<Collumn> columns = new List<Collumn>();
            for (int i = 0; i < cols.Length; i++)
            {
                try
                {
                    if (cols[i].Contains('"'))
                    {
                        int start = i;
                        var count = cols[i].Count(c => c == '"');
                        var r = count % 2;
                        while (i < cols.Length - 1 && r != 0)
                        {
                            i++;
                            count += cols[i].Count(c => c == '"');
                            r = count % 2;
                        }
                        int len = i - start;
                        if (len != 0)
                        {
#if NET_STANDARD_2_1
                            var col = string.Join(separator, cols, start, (i - start) + 1).TrimStart('\"').TrimEnd('\"');
#else
                            var col = string.Join(separator.ToString(), cols, start, (i - start) + 1).TrimStart('\"').TrimEnd('\"');
#endif
                            columns.Add(new Collumn(col, columns.Count));
                        }
                        else
                        {
                            if(cols[i].StartsWith("\""))
                            {
                                var col = cols[i].After("\"").BeforeLast("\"");
                                columns.Add(new Collumn(col, columns.Count));
                            }
                            else columns.Add(new Collumn(cols[i], columns.Count));
                        }
                    }
                    else
                    {
                        columns.Add(new Collumn(cols[i], columns.Count));
                    }
                }
                catch (System.Exception ex)
                {
                    HasError = true;
                    Debug.LogException(ex);
                }
            }
            collumns = columns.ToArray();
        }

        public void AddColumn(string value)
        {
            AddColumn(new Collumn(value));
        }
        public void AddColumn(Collumn col)
        {
            if (collumns == null) collumns = new Collumn[1];
            else System.Array.Resize(ref collumns, collumns.Length + 1);
            col.Index = collumns.Length - 1;
#if NET_STANDARD_2_1
            collumns[^1] = col; 
#else
            collumns[collumns.Length - 1] = col;
#endif
        }

        public string ToCSVString(char separator = ',')
        {
#if NET_STANDARD_2_1
            return string.Join(separator, System.Array.ConvertAll(collumns, col => col.ToCSVString(separator)));
#else
            return string.Join(separator.ToString(), System.Array.ConvertAll(collumns, col => col.ToCSVString(separator)));
#endif
        }
    }
    [System.Serializable]
    public class Collumn
    {
        [SerializeField] string value;
        public string Value { get => value; set => this.value = value; }
        public bool IsEmpty => string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value);
        public int Index { get; internal set; }

        public Collumn(string value)
        {
            this.value = value.Replace("\r", "\n");
        }
        internal Collumn(string value, int index)
        {
            this.value = value.Replace("\r", "\n");
            Index = index;
        }

        public string ToCSVString(char separator = ',')
        {
            if (value.Contains(separator) || value.Contains("\n"))
                value = $"\"{value}\"";
            return value;
        }

        public override string ToString()
        {
            return value;
        }
    }
}
