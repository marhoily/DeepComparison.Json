using System.Collections.Generic;

namespace DeepComparison.Json
{
    /// <summary>Combines result of comparison with the description of what did not compare </summary>
    public sealed class ComparisonResult
    {
        /// <summary>True if objects are equal</summary>
        public bool AreEqual { get; }
        /// <summary>Path to the property that does not compare</summary>
        public List<string> Path { get; } = new List<string>();
        /// <summary>Comparer that was used and the compared values</summary>
        public string Message { get; }
        /// <summary>ctor</summary>
        private ComparisonResult(bool areEqual) { AreEqual = areEqual; }
        /// <summary>ctor</summary>
        private ComparisonResult(string message) { Message = message; }

        public static readonly ComparisonResult True = new ComparisonResult(true);
        /// <summary>False</summary>
        public static implicit operator ComparisonResult (string s) => new ComparisonResult(s);

        /// <summary>pretty prints the result</summary>
        public override string ToString()
            => $"{string.Join(".", Path)}: {Message}";
    }
}