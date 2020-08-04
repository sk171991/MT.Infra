using System.Text;

namespace MT.Infra.Common
{
    #region ExtensionMethods
    public static class ExtensionMethods
    {
        public static StringBuilder TrimEnd(this StringBuilder stringBuilder)
        {
            stringBuilder.Length = stringBuilder.ToString().TrimEnd().Length;

            return stringBuilder;
        }
        public static StringBuilder TrimStart(this StringBuilder stringBuilder)
        {
            stringBuilder.Replace(stringBuilder.ToString(), stringBuilder.ToString().TrimStart());

            return stringBuilder;
        }
        public static string Truncate(this string originalString, int maxLength)
        {
            return originalString.Substring(0, maxLength);
        }

    }
    #endregion
}
