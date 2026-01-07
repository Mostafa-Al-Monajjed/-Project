using System.Text.RegularExpressions;
using StoreManagement.Exceptions;

namespace StoreManagement.Utils
{
    public static class FilePathValidator
    {
        public static bool IsValidJsonFile(string path)
        {
            return Regex.IsMatch(path, @"\.json$", RegexOptions.IgnoreCase);
        }

        public static void ValidateFileExtension(string path, string expectedExtension)
        {
            if (!path.EndsWith(expectedExtension, System.StringComparison.OrdinalIgnoreCase))
            {
                throw new FileFormatException($"File must be a {expectedExtension} file");
            }
        }
    }
}
