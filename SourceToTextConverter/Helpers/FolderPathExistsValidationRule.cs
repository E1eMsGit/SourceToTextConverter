using System.Globalization;
using System.IO;
using System.Windows.Controls;

namespace SrcToTextConverter.Helpers
{
    class FolderPathExistsValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string path = value as string;
            
            return path.Length < 3 || !Directory.Exists(path)? new ValidationResult(false, "Path is not exists") : ValidationResult.ValidResult;
        }
    }
}
