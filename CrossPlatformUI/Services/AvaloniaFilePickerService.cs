using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using System.Threading.Tasks;
using System.Linq;

namespace CrossPlatformUI.Services;
public class AvaloniaFilePickerService : IFilePickerService
{
    public async Task<string?> PickFileAsync(string title, string[] allowedExtensions, Window? owner = null)
    {
        var dialog = new OpenFileDialog
        {
            Title = title,
            AllowMultiple = false,
            Filters = { new FileDialogFilter { Name = "ROM", Extensions = allowedExtensions.ToList() } }
        };
        var mainWindow = (Avalonia.Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        var result = await dialog.ShowAsync( owner ?? mainWindow);
        return result?.FirstOrDefault();
    }
}