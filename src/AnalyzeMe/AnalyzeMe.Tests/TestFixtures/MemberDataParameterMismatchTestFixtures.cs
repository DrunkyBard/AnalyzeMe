using System.Text;
using AnalyzeMe.Tests.Helpers;

namespace AnalyzeMe.Tests.TestFixtures
{
	public sealed class MemberDataParameterMismatchTestFixtures
	{
		private const string MemberDataDeclaration = @"
namespace Xunit
{
	public class MemberDataAttribute : Attribute
	{
		public Type MemberType { get; set; }

		public MemberDataAttribute(string memberName)
		{
		}
	}
}";

		private static string TestFixtureClass = @"
public static class TestFixture
{
	public static IEnumerable<object[]> CorrectTestFixture()
	{
		yield return new object[]{ 1, ""A"", false};
		yield return new object[]{ 2, ""B"", true};
	}

	public static IEnumerable<object[]> WrongTestFixture()
	{
		yield return new object[]{ false, 1, ""A""};
		yield return new object[]{ 2, ""B""};
	}
}";

		private const string InnerCorrectMemberData = @"[MemberData(""CorrectTestFixture"")]";

		private const string InnerWrongMemberData = @"[MemberData(""WrongTestFixture"")]";

		private const string OuterCorrectMemberData = @"[MemberData(""CorrectTestFixture"", MemberType = typeof(TestFixture))]";

		private const string OuterWrongMemberData = @"[MemberData(""WrongTestFixture"", , MemberType = typeof(TestFixture))]";

		private const string TestClass = @"
public class TestClass
{
	@MemberDataPlaceHolder@
	public void TestMethod(int a, string b, bool c)
	{
	}

	public static IEnumerable<object[]> CorrectTestFixture()
	{
		yield return new object[]{ 1, ""A"", false};
		yield return new object[]{ 2, ""B"", true};
	}
	
	public static IEnumerable<object[]> WrongTestFixture()
	{
		yield return new object[]{ false, 1, ""A""};
		yield return new object[]{ 2, ""B""};
	}
}";

		private static string BuildInnerCorrectTestFixture()
		{
			var sb = new StringBuilder();
			return sb
				.AppendWithLine(MemberDataDeclaration)
				.AppendWithLine(TestClass.Replace("@MemberDataPlaceHolder@", InnerCorrectMemberData))
				.ToString();
		}

		private static string BuildInnerWrongTestFixture()
		{
			var sb = new StringBuilder();
			return sb
				.AppendWithLine(MemberDataDeclaration)
				.AppendWithLine(TestClass.Replace("@MemberDataPlaceHolder@", InnerWrongMemberData))
				.ToString();
		}

		private static string BuildOuterCorrectTestFixture()
		{
			var sb = new StringBuilder();
			return sb
				.AppendWithLine(MemberDataDeclaration)
				.AppendWithLine(TestFixtureClass)
				.AppendWithLine(TestClass.Replace("@MemberDataPlaceHolder@", OuterCorrectMemberData))
				.ToString();
		}

		private static string BuildOuterWrongTestFixture()
		{
			var sb = new StringBuilder();
			return sb
				.AppendWithLine(MemberDataDeclaration)
				.AppendWithLine(TestFixtureClass)
				.AppendWithLine(TestClass.Replace("@MemberDataPlaceHolder@", OuterWrongMemberData))
				.ToString();
		}
	}
}





