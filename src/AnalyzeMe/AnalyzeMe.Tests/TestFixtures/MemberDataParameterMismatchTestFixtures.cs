using System.Text;
using AnalyzeMe.Tests.Helpers;
using Xunit;

namespace AnalyzeMe.Tests.TestFixtures
{
	public sealed class MemberDataParameterMismatchTestFixtures
	{
		private const string MemberDataDeclaration = @"
using Xunit;

namespace Xunit
{
	public class MemberDataAttribute : System.Attribute
	{
		public System.Type MemberType { get; set; }

		public MemberDataAttribute(string memberName)
		{
		}
	}
}";

		private static string TestFixtureClass = @"
public static class TestFixture
{
	public static System.Collections.Generic.IEnumerable<object[]> CorrectTestFixture()
	{
		yield return new object[]{ 1, ""A"", false};
		yield return new object[]{ 2, ""B"", true};
	}

	public static System.Collections.Generic.IEnumerable<object[]> WrongTestFixture()
	{
		yield return new object[]{ false, 1, ""A""};
		yield return new object[]{ 2, ""B""};
	}
}";

		private const string InnerCorrectMemberData = @"[Xunit.MemberData(""CorrectTestFixture"")]";

		private const string InnerWrongMemberData = @"[Xunit.MemberData(""WrongTestFixture"")]";

		private const string OuterCorrectMemberData = @"[MemberData(""CorrectTestFixture"", MemberType = typeof(TestFixture))]";

		private const string OuterWrongMemberData = @"[Xunit.MemberData(""WrongTestFixture"", , MemberType = typeof(TestFixture))]";

		private const string TestClass = @"
public class TestClass
{
	@MemberDataPlaceHolder@
	public void TestMethod(int a, string b, bool c)
	{
	}

	public static System.Collections.Generic.IEnumerable<object[]> CorrectTestFixture()
	{
		yield return new object[]{ 1, ""A"", false};
		yield return new object[]{ 2, ""B"", true};
	}
	
	public static System.Collections.Generic.IEnumerable<object[]> WrongTestFixture()
	{
		yield return new object[]{ false, 1, ""A""};
		yield return new object[]{ 2, ""B""};
	}
}";

		public static TheoryData<string> InnerCorrectTestFixture()
		{
			var sb = new StringBuilder();
			var src = sb
				.AppendWithLine(MemberDataDeclaration)
				.AppendWithLine(TestClass.Replace("@MemberDataPlaceHolder@", InnerCorrectMemberData))
				.ToString();
			
			return new TheoryData<string> {src};
		}

		public static string InnerWrongTestFixture()
		{
			var sb = new StringBuilder();
			return sb
				.AppendWithLine(MemberDataDeclaration)
				.AppendWithLine(TestClass.Replace("@MemberDataPlaceHolder@", InnerWrongMemberData))
				.ToString();
		}

		public static TheoryData<string> OuterCorrectTestFixture()
		{
			var sb = new StringBuilder();
			var src = sb
				.AppendWithLine(MemberDataDeclaration)
				.AppendWithLine(TestFixtureClass)
				.AppendWithLine(TestClass.Replace("@MemberDataPlaceHolder@", OuterCorrectMemberData))
				.ToString();

			return new TheoryData<string> {src};
		}

		public static string OuterWrongTestFixture()
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





