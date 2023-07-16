using System.Diagnostics;
using JTIS.Console;

namespace JTIS.Extensions
{

    public enum StringCompareType
    {
        scEquals = 0, 
        scStartsWith = 1,
        scEndsWith = 2, 
        scContains = 3

    }

    // public class IsMatch : Property<IsMatch>
    // {
    //     public IsMatch(string name, Type type) : base(name, type)
    //     {
            
    //     }

    // }

    public static class StringExt
    {

        public static string Scrub(this string item)
        {
            return ConsoleUtil.Scrub(item);
        }

        // converts any two inputs to strings and compares
        public static bool StringsMatch<T,T2>(this T sourceData, T2 compareTo, StringCompareType compareType = StringCompareType.scEquals)
        {            
            bool result = false;
            string s1= sourceData.ToString() ?? string.Empty;
            string s2=compareTo.ToString() ?? string.Empty;
            switch (compareType)
            {
                case StringCompareType.scEquals:
                    result=s1.Equals(s2,StringComparison.OrdinalIgnoreCase);
                    break;
                case StringCompareType.scStartsWith:
                    result=s1.StartsWith(s2,StringComparison.OrdinalIgnoreCase);
                    break;
                case StringCompareType.scEndsWith:
                    result=s1.EndsWith(s2,StringComparison.OrdinalIgnoreCase);
                    break;
                case StringCompareType.scContains:
                    result=s1.Contains(s2,StringComparison.OrdinalIgnoreCase);
                    break;
            }
            return result;            
        }

        public static void StringsMatchTest()
        {
            string s1 = "15";
            double d1 = 15;
            int i1 = 15;
            short sh1 = 15;

            

            Debug.Assert(s1.StringsMatch(i1));
            Debug.Assert(s1.StringsMatch(d1));
            Debug.Assert(s1.StringsMatch(i1));
            Debug.Assert(s1.StringsMatch(sh1));

            s1="0.1";
            d1 = 0.1;
            System.Console.WriteLine($"Compare String: {s1} to double: {d1}");
            Debug.Assert("0.1".StringsMatch(0.1));

            string? nullableS = null;
            int? nullableI = null;
            Debug.Assert(nullableI.StringsMatch(nullableS));
            

        }

    }

}