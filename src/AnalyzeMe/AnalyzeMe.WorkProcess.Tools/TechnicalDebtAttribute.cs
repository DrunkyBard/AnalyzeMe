using System;

namespace AnalyzeMe.WorkProcess.Tools
{
    /// <summary>
    /// Attribute designed to display the technical debt in your code.
    /// TechnicalDebtAnalyzer analyzes your code on the presence of this attribute, and if the term of technical debt has expired, the analyzer will notify you about this issue.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
    //Resharper disable All
    public class TechnicalDebtAttribute : Attribute
    {
        /// <summary>
        /// Initialize attribute.
        /// </summary>
        /// <param name="year">Year expiration of technical debt.</param>
        /// <param name="month">Month expiration of technical debt.</param>
        /// <param name="day">Day expiration of technical debt.</param>
        /// <param name="reason">Reason of pay back technical debt.</param>
        /// <remarks>If the <paramref name="year"/>, <paramref name="month"/> and <paramref name="day"/> do not match the correct date,
        /// then analyzer notify you about this issue. Also, <paramref name="reason"/> parameter should not be null or empty, otherwise, because of what the debt arose?
        /// </remarks>
        public TechnicalDebtAttribute(int year, Month month, int day, string reason)
        {}
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
