namespace WorkitemCreator
{
    public static class Extensions
    {
        /// <summary>
        ///     Returns null if the string is null or whitespace
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string NullIfWhitespace(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return value;
        }
    }
}