﻿using System.Collections;
using System.Collections.Generic;
using System.Text;
using AnalyzeMe.Tests.Helpers;
using Xunit;

namespace AnalyzeMe.Tests.TestFixtures
{
	public sealed partial class MemberDataParameterMismatchTestFixtures
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

	public class Q{}
}";

		private static string TestFixtureClass = @"
public partial class TestFixture {
	public void A(){}
}

public partial class TestFixture
{
	public static System.Collections.Generic.IEnumerable<object[]> CorrectTestFixture()
	{
		yield return GetData();
		yield return GetListData();
		yield return new object[]{ (byte)1, ""A"", GetBool()};
		yield return new object[]{ 2, ""B"", true};
	}

	private static object[] GetData()
	{
		return null;
	}

	private static System.Collections.Generic.List<object> GetListData()
	{
		return null;
	}

	private static bool GetBool() => false;

	public static System.Collections.Generic.IEnumerable<object[]> WrongTestFixture()
	{
		yield return new object[]{ false, 1, ""A""};
		yield return new object[]{ 2, ""B""};
	}
}";

		private const string InnerCorrectMemberData = @"[Xunit.MemberData(""CorrectTestFixture"")]";

		private const string InnerWrongMemberData = @"[Xunit.MemberData(""WrongTestFixture"")]";

		//private const string OuterCorrectMemberData = @"[MemberData(""CorrectTestFixture"", MemberType = typeof(TestFixture))]";
		private const string OuterCorrectMemberData = @"[MemberData(nameof(CorrectTestFixture), MemberType = typeof(TestFixture))]";

		private const string OuterWrongMemberData = @"[Xunit.MemberData(""WrongTestFixture"", , MemberType = typeof(TestFixture))]";

		private const string TestClass = @"
public class TestClass
{
	@MemberDataPlaceHolder@
	public void TestMethod(int a, string b, bool c, Xunit.Q u, System.IO.Stream stream)
	{
		var a = TestFixture.CorrectTestFixture();
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

		public static IEnumerable<IA[]> TestA()
		{
			yield return Fixtures();
			//return new CustomCollection<IA[]> {Fixtures()};
		}

		private static IA[] Fixtures()
		{
			return new IA[] {new A(), new B()};
		}
	}

	public class CustomCollection<T> : IEnumerable<T>
	{
		private readonly List<T> _list = new List<T>();

		public IEnumerator<T> GetEnumerator()
		{
			return _list.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(T item)
		{
			_list.Add(item);
		}
	}

	public interface IA
	{
	}

	public class A : IA
	{
	}

	public class B : IA
	{
	}
}





