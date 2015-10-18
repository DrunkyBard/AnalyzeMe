using System.Diagnostics.Contracts;

namespace AnalyzeMe.Tests.TestFixtures
{
    public sealed class SourceFixture
    {
        public readonly string Actual;
        public readonly string Expected;

        public SourceFixture(string actual, string expected)
        {
            Contract.Requires(actual != null);
            Contract.Requires(expected != null);

            Actual = actual;
            Expected = expected;
        }
    }
}
