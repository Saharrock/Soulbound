using System.Globalization;
using System.IO;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using Soulbound.Models;

namespace Soulbound.Converters
{
    
    public sealed class GoalPhotoPathConverter : IValueConverter
    {
        
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            string? relative = value switch
            {
                GoalPhotoRef r => r.RelativePath,
                string s => s,
                _ => null
            };

            if (string.IsNullOrWhiteSpace(relative))
            {
                return null;
            }

            string full = Path.GetFullPath(Path.Combine(FileSystem.AppDataDirectory,
                relative.Replace('/', Path.DirectorySeparatorChar)));
            string root = Path.GetFullPath(FileSystem.AppDataDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            if (!full.StartsWith(root, StringComparison.OrdinalIgnoreCase) || !File.Exists(full))
            {
                return null;
            }

            return ImageSource.FromFile(full);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}
