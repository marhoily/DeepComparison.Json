using System;
using DeepComparison;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Tests
{
    public sealed class JsonCases
    {
        private readonly JsonComparer _comparer;

        public JsonCases()
        {
            _comparer = new JsonComparerBuilder().Build();
        }

        [Fact] public void Uri() => Check(new Uri("http://blah.com"), new Uri("x", UriKind.Relative));
        [Fact] public void Double() => Check(3.14f, 0.0f);
        [Fact] public void Float() => Check(3.14, 0.0);
        [Fact] public void Boolean() => Check(true, false);
        [Fact] public void Int32() => Check(569, 0);
        [Fact] public void Int64() => Check(569L, 0L);
        [Fact] public void String() => Check("blah", "");
        [Fact] public void DateTime() => Check(new DateTime(2000,1,1), new DateTime());
        [Fact] public void Duration() => Check(TimeSpan.FromSeconds(2), TimeSpan.Zero);
        [Fact] public void Guid_Ranom() => Check(Guid.NewGuid(), Guid.Empty);

        private void Check<T>(T random, T empty)
        {
            var jo = JToken.FromObject(new { X = random });
            _comparer
                .Compare(jo, new { X = random })
                .Should().Be(ComparisonResult.True);
            _comparer
                .Compare(jo, new { X = empty })
                .Should().NotBe(ComparisonResult.True);
            _comparer
                .Compare(JToken.FromObject(
                    new {X = default(T)}), new {X = default(T)})
                .Should().Be(ComparisonResult.True);
        }
    }
}