using System.Collections.Generic;
using System.Linq;
using System;
using System.Reflection;
using System.Text;
using System.Linq.Expressions;

namespace SKD.Common {


    public class FlatFileLine<T> where T : new() {

        public class Field {
            public string Name { get; set; }
            public int Length { get; set; }
        }

        public class FieldValue {

            public FieldValue() { }
            public string Name { get; set; }
            public string Value { get; set; }
        }

        public int LineLength { get; init; }

        public List<Field> Fields { get; set; } = new List<Field>();

        ///<param name="schemaObject">A type with int properties representing character fields</param>
        public FlatFileLine() {
            Fields = GetSchemaFields();
            LineLength = Fields.Select(t => t.Length).Aggregate((a, b) => a + b);
        }

        public FieldValue CreateFieldValue(Expression<Func<T, object>> expr, string value) {
            var member = expr.GetAccessedMemberInfo();
            var fieldValue = new FieldValue {
                Name = member.Name,
                Value = value
            };
            return fieldValue;

        }

        private List<Field> GetSchemaFields() {
            var schemaObject = (T)Activator.CreateInstance(typeof(T));

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

        public string GetFieldValue(string lineText, Expression<Func<T, object>> prop) {
            var member = prop.GetAccessedMemberInfo();

            var pos = 0;
            foreach (var field in Fields) {
                if (field.Name == member.Name) {
                    var value = lineText.Substring(pos, field.Length);
                    return value;
                }
                pos += field.Length;
            }
            throw new Exception($"field '{prop.Name}' not found in layout");
        }

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

            var line = builder.ToString();
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