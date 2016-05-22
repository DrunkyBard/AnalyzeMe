using AnalyzeMe.Design.Analyzers.xUnit;
using AnalyzeMe.Tests.TestFixtures;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace AnalyzeMe.Tests
{
	public sealed class MemberDataParameterMismatchTests : CodeFixVerifier
	{
		[Theory, MemberData("OuterCorrectTestFixture", MemberType = typeof(MemberDataParameterMismatchTestFixtures))]
		public void Test(string src)
		{
			VerifyCSharpDiagnostic(src);
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
		{
			return new MemberDataParameterMismatchAnalyzer();
		}
	}
}
