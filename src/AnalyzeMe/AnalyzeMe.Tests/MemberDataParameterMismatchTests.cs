using System.Reflection;
using AnalyzeMe.Design.Analyzers.xUnit;
using AnalyzeMe.Tests.TestFixtures;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace AnalyzeMe.Tests
{
	public sealed class MemberDataParameterMismatchTests : CodeFixVerifier
	{
		[Theory]
        [MemberData(nameof(MemberDataParameterMismatchTestFixtures.OuterCorrectTestFixture), MemberType = typeof(MemberDataParameterMismatchTestFixtures))]
		public void Test(string src)
		{
			VerifyCSharpDiagnostic(src);
		}

		[Theory]
		[MemberData(nameof(MemberDataParameterMismatchTestFixtures.TestA), MemberType = typeof(MemberDataParameterMismatchTestFixtures))]
		public void A(IA a)
		{
			var a1 = 1;
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
		{
			return new MemberDataParameterMismatchAnalyzer();
		}
	}
}
