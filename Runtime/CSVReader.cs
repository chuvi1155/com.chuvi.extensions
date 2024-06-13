using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class CSVReader
{
    [SerializeField] Row[] rows;
    public Row[] Rows { get => rows; }
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

    [System.Serializable]
    public class Row
    {
        [SerializeField] private Collumn[] collumns;
        public readonly bool HasError;
        public Collumn[] Collumns { get => collumns; }

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
                            var col = string.Join(separator, cols, start, (i - start) + 1).TrimStart('\"').TrimEnd('\"');
                            columns.Add(new Collumn(col, columns.Count));
                        }
                        else columns.Add(new Collumn(cols[i], columns.Count));
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
    }
    [System.Serializable]
    public class Collumn
    {
        [SerializeField] string value;
        public string Value { get => value; set => this.value = value; }
        public readonly bool IsEmpty;
        public readonly int Index;

        internal Collumn(string value, int index)
        {
            this.value = value.Replace("\r", "\n");
            IsEmpty = string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value);
            Index = index;
        }

        public override string ToString()
        {
            return value;
        }
    }
}