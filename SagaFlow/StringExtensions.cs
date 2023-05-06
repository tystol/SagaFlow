using System;
using System.Text;

namespace SagaFlow
{
    internal static class StringExtensions
    {
        /// <summary>
        /// Converts a PascalCaseString into a camelCaseString
        /// </summary>
        public static string ToCamelCase(this string source)
        {
            if (String.IsNullOrEmpty(source) || Char.IsLower(source, 0))
                return source;

            return Char.ToLowerInvariant(source[0]) + source.Substring(1);
        }

        /// <summary>
        /// Converts a PascalCaseString or a 'normally spaced string' into a kebab-case-string
        /// </summary>
        /// <remarks>
        /// Adapted from: https://stackoverflow.com/a/54012346/505457
        /// </remarks>
        public static string ToKebabCase(this string source)
        {
            if (source is null) return null;

            if (source.Length == 0) return string.Empty;

            StringBuilder builder = new StringBuilder();

            for (var i = 0; i < source.Length; i++)
            {
                // TODO: some tests around these 'normally spaced string' conversions.
                if (source[i] == ' ')
                {
                    builder.Append('-');
                }
                else if (char.IsLower(source[i])) // if current char is already lowercase
                {
                    builder.Append(source[i]);
                }
                else if (i == 0) // if current char is the first char
                {
                    builder.Append(char.ToLower(source[i]));
                }
                else if (char.IsDigit(source[i]) && !char.IsDigit(source[i - 1])) // if current char is a number and the previous is not
                {
                    builder.Append('-');
                    builder.Append(source[i]);
                }
                else if (char.IsDigit(source[i])) // if current char is a number and previous is
                {
                    builder.Append(source[i]);
                }
                else if (source[i - 1] == ' ') // if current char is upper and previous char is a space
                {
                    // prev space char already replaced with a hyphen so just emit the character 
                    builder.Append(char.ToLower(source[i]));
                }
                else if (char.IsLower(source[i - 1])) // if current char is upper and previous char is lower
                {
                    builder.Append('-');
                    builder.Append(char.ToLower(source[i]));
                }
                else if (i + 1 == source.Length || char.IsUpper(source[i + 1])) // if current char is upper and next char doesn't exist or is upper
                {
                    builder.Append(char.ToLower(source[i]));
                }
                else // if current char is upper and next char is lower
                {
                    builder.Append('-');
                    builder.Append(char.ToLower(source[i]));
                }
            }
            return builder.ToString();
        }
    }
}

