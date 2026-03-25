using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Nintenlord.Hacking.Core;

namespace Nintenlord.UPSpatcher.AvaloniaApp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void BrowsePatchTargetFile_Click(object sender, RoutedEventArgs e) => PatchTargetFileTextBox.Text = await PickFilePathAsync("Select a file", false);

    private async void BrowsePatchFile_Click(object sender, RoutedEventArgs e) => PatchFileTextBox.Text = await PickFilePathAsync("Select a patch", true);

    private async void BrowseOriginalFile_Click(object sender, RoutedEventArgs e) => OriginalFileTextBox.Text = await PickFilePathAsync("Select the original file", false);

    private async void BrowseModifiedFile_Click(object sender, RoutedEventArgs e) => ModifiedFileTextBox.Text = await PickFilePathAsync("Select the modified file", false);

    private async void BrowseOutputPatchFile_Click(object sender, RoutedEventArgs e) => OutputPatchFileTextBox.Text = await SaveFilePathAsync("Select where to save patch", "ups");

    private async void BrowseInspectPatchFile_Click(object sender, RoutedEventArgs e) => InspectPatchFileTextBox.Text = await PickFilePathAsync("Select a patch", true);

    private async void ApplyPatch_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(PatchTargetFileTextBox.Text) || !File.Exists(PatchTargetFileTextBox.Text))
        {
            await ShowMessageAsync("Target file does not exist.");
            return;
        }

        if (string.IsNullOrWhiteSpace(PatchFileTextBox.Text) || !File.Exists(PatchFileTextBox.Text))
        {
            await ShowMessageAsync("Patch file does not exist.");
            return;
        }

        UPSfile upsFile;
        byte[] file;

        try
        {
            upsFile = new UPSfile(PatchFileTextBox.Text);
            file = await File.ReadAllBytesAsync(PatchTargetFileTextBox.Text);
        }
        catch
        {
            await ShowMessageAsync("Error opening file. Verify selected file paths.");
            return;
        }

        if (!upsFile.ValidPatch)
        {
            await ShowMessageAsync("The patch is corrupt.");
            return;
        }

        var validToApply = upsFile.ValidToApply(file);
        if (OnMismatchCancelRadio.IsChecked == true && !validToApply)
        {
            await ShowMessageAsync("The patch does not match the file. Patching canceled.");
            return;
        }

        if (OnMismatchAskRadio.IsChecked == true && !validToApply)
        {
            var shouldContinue = await ConfirmAsync("The patch does not match the file. Patch anyway?");
            if (!shouldContinue)
            {
                return;
            }
        }

        if (OnMismatchWarnRadio.IsChecked == true && !validToApply)
        {
            await ShowMessageAsync("The patch does not match the file. Patching anyway.");
        }

        if (CreateBackupCheckBox.IsChecked == true)
        {
            CreateBackup(PatchTargetFileTextBox.Text);
        }

        var newFile = upsFile.Apply(file);
        await File.WriteAllBytesAsync(PatchTargetFileTextBox.Text, newFile);
        await ShowMessageAsync("Patching has been done.");
    }

    private async void CreatePatch_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(OriginalFileTextBox.Text) || !File.Exists(OriginalFileTextBox.Text))
        {
            await ShowMessageAsync("Original file does not exist.");
            return;
        }

        if (string.IsNullOrWhiteSpace(ModifiedFileTextBox.Text) || !File.Exists(ModifiedFileTextBox.Text))
        {
            await ShowMessageAsync("Modified file does not exist.");
            return;
        }

        if (string.IsNullOrWhiteSpace(OutputPatchFileTextBox.Text))
        {
            await ShowMessageAsync("Output patch file is missing.");
            return;
        }

        try
        {
            var original = await File.ReadAllBytesAsync(OriginalFileTextBox.Text);
            var modified = await File.ReadAllBytesAsync(ModifiedFileTextBox.Text);
            var upsFile = new UPSfile(original, modified);
            upsFile.WriteToFile(OutputPatchFileTextBox.Text);
        }
        catch
        {
            await ShowMessageAsync("Error creating patch. Verify selected file paths.");
            return;
        }

        await ShowMessageAsync("Patch has been created.");
    }

    private async void ReadPatchData_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(InspectPatchFileTextBox.Text) || !File.Exists(InspectPatchFileTextBox.Text))
        {
            await ShowMessageAsync("Patch does not exist.");
            return;
        }

        try
        {
            var details = await Task.Run(() =>
            {
                var upsFile = new UPSfile(InspectPatchFileTextBox.Text);
                return upsFile.GetData();
            });

            InspectOutputTextBox.Text = BuildPatchData(details);
        }
        catch
        {
            await ShowMessageAsync("Error reading patch data.");
        }
    }

    private void ClearPatchTab_Click(object sender, RoutedEventArgs e)
    {
        PatchTargetFileTextBox.Text = string.Empty;
        PatchFileTextBox.Text = string.Empty;
        CreateBackupCheckBox.IsChecked = true;
        OnMismatchCancelRadio.IsChecked = true;
    }

    private void ClearCreateTab_Click(object sender, RoutedEventArgs e)
    {
        OriginalFileTextBox.Text = string.Empty;
        ModifiedFileTextBox.Text = string.Empty;
        OutputPatchFileTextBox.Text = string.Empty;
    }

    private void ClearInspectTab_Click(object sender, RoutedEventArgs e)
    {
        InspectPatchFileTextBox.Text = string.Empty;
        InspectOutputTextBox.Text = string.Empty;
    }

    private static string BuildPatchData(int[,] details)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Offsets\tLengths");
        for (var i = 0; i < details.GetLength(0); i++)
        {
            sb.AppendLine($"{Convert.ToString(details[i, 0], 16).ToUpper()}\t{Convert.ToString(details[i, 1], 16).ToUpper()}");
        }

        return sb.ToString();
    }

    private static void CreateBackup(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath) ?? string.Empty;
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var backupPath = Path.Combine(directory, fileName + ".bak");

        var index = 1;
        while (File.Exists(backupPath))
        {
            backupPath = Path.Combine(directory, fileName + index + ".bak");
            index++;
        }

        File.Copy(filePath, backupPath, false);
    }

    private async Task<string> PickFilePathAsync(string title, bool upsOnly)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = false,
            FileTypeFilter = upsOnly
                ? new[] { new FilePickerFileType("UPS files") { Patterns = new[] { "*.ups" } } }
                : null
        });

        return files.FirstOrDefault()?.TryGetLocalPath() ?? string.Empty;
    }

    private async Task<string> SaveFilePathAsync(string title, string extension)
    {
        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = title,
            DefaultExtension = extension,
            FileTypeChoices = new[] { new FilePickerFileType("UPS file") { Patterns = new[] { "*.ups" } } }
        });

        return file?.TryGetLocalPath() ?? string.Empty;
    }

    private async Task ShowMessageAsync(string message)
    {
        var okButton = new Button
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            Content = "OK",
            Width = 84
        };

        var dialog = new Window
        {
            Width = 460,
            Height = 180,
            MinWidth = 360,
            MinHeight = 150,
            Title = "Nintenlord UPS patcher",
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content = new StackPanel
            {
                Margin = new Avalonia.Thickness(16),
                Spacing = 12,
                Children =
                {
                    new TextBlock { Text = message, TextWrapping = Avalonia.Media.TextWrapping.Wrap },
                    okButton
                }
            }
        };

        okButton.Click += (_, _) => dialog.Close();
        await dialog.ShowDialog(this);
    }

    private async Task<bool> ConfirmAsync(string message)
    {
        var result = false;
        var dialog = new Window
        {
            Width = 500,
            Height = 190,
            MinWidth = 390,
            MinHeight = 160,
            Title = "Confirm",
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        var okButton = new Button { Content = "Patch", Width = 90 };
        var cancelButton = new Button { Content = "Cancel", Width = 90 };

        okButton.Click += (_, _) =>
        {
            result = true;
            dialog.Close();
        };

        cancelButton.Click += (_, _) => dialog.Close();

        dialog.Content = new StackPanel
        {
            Margin = new Avalonia.Thickness(16),
            Spacing = 14,
            Children =
            {
                new TextBlock { Text = message, TextWrapping = Avalonia.Media.TextWrapping.Wrap },
                new StackPanel
                {
                    Orientation = Avalonia.Layout.Orientation.Horizontal,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                    Spacing = 8,
                    Children = { okButton, cancelButton }
                }
            }
        };

        await dialog.ShowDialog(this);
        return result;
    }
}


