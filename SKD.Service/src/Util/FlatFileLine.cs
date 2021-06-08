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

            public FieldValue() {}
            public FieldValue(string name, string value) {
                this.Name = name;
                this.Value = value;
            }
            public string Name { get; set; }
            public string Value { get; set; }
        }

        public int LineLength { get; init; }

        public List<Field> Fields { get; set; } = new List<Field>();

        ///<param name="schemaObject">A type with int properties representing character fields</param>
        public FlatFileLine(Object schemaObject) {
            Fields = GetSchemaFields(schemaObject);
            LineLength = Fields.Select(t => t.Length).Aggregate((a, b) => a + b);
        }

        private List<Field> GetSchemaFields(Object schemaObject) {
        
            var fields = schemaObject.GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.Public)
                .Where(t => t.FieldType.Name == "Int32").ToList();

            return fields.ToList().Select(f => {
                var val = f.GetValue(schemaObject);
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

        // public FieldValue NewFieldValue(string name, string value) {
        //     var field = this.Fields.First(t => t.Name == name);
        //     value = value.Length < field.Length 
        //         ? value.PadRight(field.Length, ' ') 
        //         : value.Substring(0, field.Length);

        //     return new FieldValue {
        //         Name= field.Name,
        //         Value = value
        //     };
        // }

        public string Build(List<FieldValue> values) {
            var builder = new StringBuilder();

            foreach (var field in Fields) {
                var value = values.Where(t => t.Name == field.Name)
                    .Select(t => t.Value)
                    .FirstOrDefault();

                if (value == null) {
                    Console.WriteLine("Null " + field.Name);
                }

                value = value.Length < field.Length 
                    ? value.PadRight(field.Length)
                    : value.Substring(0, field.Length);                

                if (value.Length != field.Length) {
                    throw new Exception($"field {field.Name} length {field.Length} != value length {value.Length}");
                }
                builder.Append(value);
            }

            var line= builder.ToString();
            if (line.Length != LineLength) {
                throw new Exception($"Line length {this.LineLength} != generated line lenght {line.Length} ");
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