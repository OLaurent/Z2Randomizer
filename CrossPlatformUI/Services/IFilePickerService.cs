using System.Threading.Tasks;
using Avalonia.Controls;

public interface IFilePickerService
{
    Task<string?> PickFileAsync(string title, string[] allowedExtensions, Window? owner = null);
}