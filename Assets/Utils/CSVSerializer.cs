using UnityEngine;
using System;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Utils
{
    public static class CsvSerializer
    {
        public static T[] Deserialize<T>(string text)
        {
            return (T[]) CreateArray(typeof(T), ParseCsv(text));
        }

        private static object CreateArray(Type type, List<string[]> rows)
        {
            var arrayValue = Array.CreateInstance(type, rows.Count - 1);
            var table = new Dictionary<string, int>();

            for (var i = 0; i < rows[0].Length; i++)
            {
                var id = rows[0][i];
                var id2 = "";
                foreach (var t in id)
                {
                    switch (t)
                    {
                        case >= 'a' and <= 'z':
                        case >= '0' and <= '9':
                            id2 += t.ToString();
                            break;
                        case >= 'A' and <= 'Z':
                            id2 += ((char) (t - 'A' + 'a')).ToString();
                            break;
                    }
                }

                table.Add(id, i);
                if (!table.ContainsKey(id2))
                {
                    table.Add(id2, i);
                }
            }

            for (var i = 1; i < rows.Count; i++)
            {
                var rowData = Create(rows[i], table, type);
                arrayValue.SetValue(rowData, i - 1);
            }

            return arrayValue;
        }

        private static object Create(string[] cols, Dictionary<string, int> table, Type type)
        {
            var v = Activator.CreateInstance(type);
            var fieldInfo = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var tmp in fieldInfo)
            {
                if (table.ContainsKey(tmp.Name))
                {
                    var idx = table[tmp.Name];
                    if (idx < cols.Length)
                    {
                        SetValue(v, tmp, cols[idx]);
                    }
                }
            }

            return v;
        }

        private static void SetValue(object v, FieldInfo fieldInfo, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            if (fieldInfo.FieldType.IsArray)
            {
                var elementType = fieldInfo.FieldType.GetElementType();
                if (elementType != null)
                {
                    var elem = value.Split(',');
                    var arrayValue = Array.CreateInstance(elementType, elem.Length);
                    for (var i = 0; i < elem.Length; i++)
                    {
                        arrayValue.SetValue(
                            elementType == typeof(string) ? elem[i] : Convert.ChangeType(elem[i], elementType), 
                            i);
                    }

                    fieldInfo.SetValue(v, arrayValue);
                }
            }
            else if (fieldInfo.FieldType.IsEnum)
            {
                fieldInfo.SetValue(v, Enum.Parse(fieldInfo.FieldType, value));
            }
            else if (value.IndexOf('.') != -1 &&
                     (fieldInfo.FieldType == typeof(int) || 
                      fieldInfo.FieldType == typeof(long) ||
                      fieldInfo.FieldType == typeof(short)))
            {
                var f = (float) Convert.ChangeType(value, typeof(float));
                fieldInfo.SetValue(v, Convert.ChangeType(f, fieldInfo.FieldType));
            }
#if UNITY_EDITOR
        else if (fieldInfo.FieldType == typeof(Sprite))
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(value);
            fieldInfo.SetValue(v, sprite);
        }
#endif
            else if (fieldInfo.FieldType == typeof(string))
            {
                fieldInfo.SetValue(v, value);
            }
            else
            {
                fieldInfo.SetValue(v, Convert.ChangeType(value, fieldInfo.FieldType));
            }
        }

        private static List<string[]> ParseCsv(string text, char separator = ',')
        {
            var lines = new List<string[]>();
            var line = new List<string>();
            var token = new StringBuilder();
            var quotes = false;

            for (var i = 0; i < text.Length; i++)
            {
                if (quotes)
                {
                    if ((text[i] == '\\' && i + 1 < text.Length && text[i + 1] == '\"') ||
                        (text[i] == '\"' && i + 1 < text.Length && text[i + 1] == '\"'))
                    {
                        token.Append('\"');
                        i++;
                    }
                    else if (text[i] == '\\' && i + 1 < text.Length && text[i + 1] == 'n')
                    {
                        token.Append('\n');
                        i++;
                    }
                    else if (text[i] == '\"')
                    {
                        line.Add(token.ToString());
                        token = new StringBuilder();
                        quotes = false;
                        if (i + 1 < text.Length && text[i + 1] == separator)
                            i++;
                    }
                    else
                    {
                        token.Append(text[i]);
                    }
                }
                else if (text[i] == '\r' || text[i] == '\n')
                {
                    if (token.Length > 0)
                    {
                        line.Add(token.ToString());
                        token = new StringBuilder();
                    }

                    if (line.Count > 0)
                    {
                        lines.Add(line.ToArray());
                        line.Clear();
                    }
                }
                else if (text[i] == separator)
                {
                    line.Add(token.ToString());
                    token = new StringBuilder();
                }
                else if (text[i] == '\"')
                {
                    quotes = true;
                }
                else
                {
                    token.Append(text[i]);
                }
            }

            if (token.Length > 0)
            {
                line.Add(token.ToString());
            }

            if (line.Count > 0)
            {
                lines.Add(line.ToArray());
            }

            return lines;
        }
    }
}