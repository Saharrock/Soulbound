using Microsoft.Maui.Controls;

namespace Soulbound.Models
{
    /// <summary>
    /// Normalizes pet image file names for Android (lowercase) and provides a safe fallback.
    /// </summary>
    public static class PetImageHelper
    {
        public const string DefaultPetImageFile = "dotnet_bot.png";

        /// <summary>
        /// Trims and lowercases the file name (Android drawable names are usually lowercase).
        /// </summary>
        public static string NormalizePetImageFileName(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return DefaultPetImageFile;
            }

            return fileName.Trim().ToLowerInvariant();
        }

        /// <summary>
        /// Returns the default image if the extension is not .svg or .png.
        /// </summary>
        public static string GetSafePetImageFileName(string? fileName)
        {
            string normalized = NormalizePetImageFileName(fileName);

            if (normalized.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
            {
                return normalized;
            }

            if (normalized.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            {
                return normalized;
            }

            return DefaultPetImageFile;
        }

        /// <summary>
        /// Builds an ImageSource; on failure returns the default pet image.
        /// </summary>
        public static ImageSource CreateSafeImageSource(string? fileName)
        {
            string name = GetSafePetImageFileName(fileName);

            try
            {
                return ImageSource.FromFile(name);
            }
            catch
            {
                return ImageSource.FromFile(DefaultPetImageFile);
            }
        }
    }
}
