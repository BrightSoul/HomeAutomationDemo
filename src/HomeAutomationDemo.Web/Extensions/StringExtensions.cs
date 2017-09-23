using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeAutomationDemo.Web.Extensions
{
    public static class StringExtensions
    {
        public static string[] AsCommandArguments(this string input)
        {
            return (input ?? string.Empty).Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
