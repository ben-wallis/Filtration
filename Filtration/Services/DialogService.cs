using Microsoft.WindowsAPICodePack.Dialogs;

namespace Filtration.Services
{
    internal interface IDialogService
    {
        CommonFileDialogResult ShowFolderPickerDialog(string dialogTitle, out string folderName);
    }

    internal sealed class DialogService : IDialogService
    {
        public CommonFileDialogResult ShowFolderPickerDialog(string dialogTitle, out string folderName)
        {
            using (var dialog = new CommonOpenFileDialog(dialogTitle))
            {
                dialog.IsFolderPicker = true;
                var result = dialog.ShowDialog();

                folderName = result == CommonFileDialogResult.Ok ? dialog.FileName : string.Empty;

                return result;
            }
        }
    }
}
