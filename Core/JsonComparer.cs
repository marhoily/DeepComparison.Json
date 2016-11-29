using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Linq;

namespace DeepComparison.Json
{
    public class JsonComparer
    {
        public ComparisonResult Compare(JToken j, object a)
        {
            return CompareToken(j, a, "$root");
        }
        private ComparisonResult CompareToken(JToken j, object a, string context)
        {
            switch (j.Type)
            {
                case JTokenType.None:
                case JTokenType.Property:
                case JTokenType.Constructor:
                    throw new ArgumentOutOfRangeException(nameof(j),
                        "comparison is not supported for this token type: " + j.Type);
                case JTokenType.Object:
                    return CompareObject(j.Value<JObject>(), a, context);
                case JTokenType.Array:
                    if (!(a is IEnumerable)) return new ComparisonResult("not an array");
                    return CompareArray(j.Value<JArray>(), (IEnumerable)a, context);
                case JTokenType.Comment:
                    break;
                case JTokenType.Null:
                    if (a == null) break;
                    return new ComparisonResult($"{context}: <null> != {JToken.FromObject(a)}");
                case JTokenType.Undefined:
                case JTokenType.Raw:
                case JTokenType.Bytes:
                    throw new NotImplementedException();
                default:
                    var left = j.ToString();
                    if (a == null)
                        return new ComparisonResult($"{context}: {left} != <null>");
                    var right = JToken.FromObject(a).ToString();
                    if (left == right) break;
                    return new ComparisonResult($"{context}: {left} != {right}");
            }
            return ComparisonResult.True;
        }

        private ComparisonResult CompareArray(JArray j, IEnumerable a, string context)
        {
            var counter = 0;
            return j.SequenceEqual(a, (jj, aa) =>
                CompareToken((JToken)jj, aa, $"{context}[{counter++}]"));
        }

        private ComparisonResult CompareObject(JObject j, object a, string context)
        {
            var properties = a.GetType().GetProperties();
            foreach (var property in j.Properties())
            {
                var match = properties.FirstOrDefault(p => p.Name == property.Name);
                if (match == null)
                {
                    var subject = CheckIfAnonymousType(a.GetType())
                        ? "properties of an anonymous object:\r\n"
                        : $"properties of type {a.GetType().FullName}:\r\n";
                    return new ComparisonResult(
                        $"property {property.Name} is not found among {subject}" +
                        string.Join(", ", properties.Select(p => p.Name)));
                }
                var result = CompareToken(property.Value,
                    match.GetValue(a), context + "." + property.Name);
                if (result != ComparisonResult.True) return result;
            }
            return ComparisonResult.True;
        }

        private static bool CheckIfAnonymousType(Type type)
        {
            return Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false)
                   && type.IsGenericType && type.Name.Contains("AnonymousType")
                   && (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$"))
                   && (type.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic;
        }
    }
}