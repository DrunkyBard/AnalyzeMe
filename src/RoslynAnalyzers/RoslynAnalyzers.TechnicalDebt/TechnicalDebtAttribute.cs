using System;

namespace RoslynAnalyzers.TechnicalDebt
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
    //Resharper disable All
    public class TechnicalDebtAttribute : Attribute
    {
        public TechnicalDebtAttribute(int year, Month month, int day, string reason)
        { }
    }

    public enum Month
    {
        January = 1,
        February = 2,
        March = 3,
        April = 4,
        May = 5,
        June = 6,
        July = 7,
        August = 8,
        September = 9,
        October = 10,
        November = 11,
        December = 12
    }
}
