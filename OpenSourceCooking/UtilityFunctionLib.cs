using System;

namespace OpenSourceCooking
{
    public static class UtilityFunctionLib
    {
        public static double FractionToDouble(string fraction)
        {
            if (fraction == null)
                throw new ArgumentNullException("fraction");
            if (double.TryParse(fraction, out double result))
                return result;
            string[] split = fraction.Split(new char[] { ' ', '/' });
            if (split.Length == 2 || split.Length == 3)
            {
                if (int.TryParse(split[0], out int a) && int.TryParse(split[1], out int b))
                {
                    if (split.Length == 2)
                        return (double)a / b;
                    if (int.TryParse(split[2], out int c))
                        return a + (double)b / c;
                }
            }
            throw new FormatException("Not a valid fraction");
        }
    }
    public static class StringExt
    {
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }
}