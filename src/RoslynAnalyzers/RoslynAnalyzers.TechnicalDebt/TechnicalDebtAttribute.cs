using System;

namespace RoslynAnalyzers.TechnicalDebt
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
    //Resharper disable All
    public class TechnicalDebtAttribute : Attribute
    {
        public TechnicalDebtAttribute(int year, int month, int day, string reason)
        { }
    }
}
