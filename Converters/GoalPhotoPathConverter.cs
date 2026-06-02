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
                GoalPhotoRef r => r.RelativePath, //достаем из обьекта RelativePath
                string s => s, //если получили строчку, работаем с ней
                _ => null //сли пришло что-то другое мы записываем в переменную relative значение null
            };

            if (string.IsNullOrWhiteSpace(relative)) //проверка на пустоту 
            {
                return null;
            }

            string full = Path.GetFullPath(Path.Combine(FileSystem.AppDataDirectory, //достаем картинку из папки
                relative.Replace('/', Path.DirectorySeparatorChar))); // изменение направления слеша под определенную операционную систему

            string root = Path.GetFullPath(FileSystem.AppDataDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            if (!full.StartsWith(root, StringComparison.OrdinalIgnoreCase) || !File.Exists(full)) //Правда ли, что итоговый путь файла всё еще начинается с нашей разрешенной папки?
            {
                return null;
            }

            return ImageSource.FromFile(full); //это строчка /Net MAUI выведет если все в порядке 
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotSupportedException(); //IValueConverter обязан иметь перевод в обратную сторону, но у нас Операция не поддерживается
    }
}
