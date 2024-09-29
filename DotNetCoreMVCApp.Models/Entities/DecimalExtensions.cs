using System.Globalization;

namespace DotNetCoreMVCApp.Extensions
{
    public static class DecimalExtensions
    {
        public static string ToFormattedString(this decimal value)
        {
            return value.ToString("N2", CultureInfo.CurrentCulture);
        }
    }
}