using System.Collections.Generic;
using System.Linq;
using System;
using System.Reflection;
using System.Text;

namespace SKD.Service.Util {

    public class FlatFileLine {

        public class Field {
            public string Name { get; set; }
            public int Length { get; set; }
        }

        public class FieldValue {
            public string Name { get; set; }
            public string Value { get; set; }
        }

        public int LineLength { get; init; }


        public List<Field> Fields { get; set; } = new List<Field>();

        ///<param name="schema">A type with int properties representing character fields</param>
        public FlatFileLine(Type schema) {
            Fields = GetSchemaFields(schema);
            LineLength = Fields.Select(t => t.Length).Aggregate((a, b) => a + b);
        }

        private List<Field> GetSchemaFields(Type layoutType) {
            var fields = layoutType
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static)
                .Where(t => t.FieldType.Name == "Int32").ToList();

            return fields.ToList().Select(f => {
                var val = f.GetValue(layoutType);
                return new Field {
                    Name = f.Name,
                    Length = (int)val
                };
            }).ToList();
        }

        public string GetFieldValue(string lineText, string fieldName) {
            var pos = 0;
            foreach (var field in Fields) {
                if (field.Name == fieldName) {
                    var value = lineText.Substring(pos, field.Length);
                    return value;
                }
                pos += field.Length;
            }
            throw new Exception($"field '{fieldName}' not found in layout");
        }

        public string Build(List<FieldValue> values) {
            var builder = new StringBuilder();

            foreach (var field in Fields) {
                var value = values.Where(t => t.Name == field.Name)
                    .Select(t => t.Value)
                    .FirstOrDefault();

                value = value.PadRight(field.Length);
                builder.Append(value);
            }

            return builder.ToString();
        }

        public List<FieldValue> Parse(string text) {
            var fieldValues = new List<FieldValue>();
            var pos = 0;
            foreach (var field in Fields) {
                var fieldValue = new FieldValue {
                    Name = field.Name,
                    Value = text.Substring(pos, field.Length)
                };
                fieldValues.Add(fieldValue);

                pos += field.Length;
            }
            return fieldValues;
        }
    }
}