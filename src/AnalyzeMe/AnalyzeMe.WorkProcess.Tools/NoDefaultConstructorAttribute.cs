using System;
using AnalyzeMe.WorkProcess.Tools;

namespace AnalyzeMe.WorkProcess.Tools
{
    [AttributeUsage(AttributeTargets.Struct)]
    public sealed class NoDefaultConstructorAttribute : Attribute
    { }
}

namespace Test
{
    [NoDefaultConstructor]
    public struct S
    {
        public S(int i) { }
    }

    public class A
    {
        public void T()
        {
            var t = new S(2);
        }
    }
}