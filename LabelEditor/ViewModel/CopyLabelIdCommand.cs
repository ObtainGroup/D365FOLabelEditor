using LabelLibrary;

using System;
using System.Windows;
using System.Windows.Input;

namespace LabelEditor.ViewModel
{
    /// <summary>
    /// Copy label into clipboard
    /// </summary>
    public class CopyLabelIdCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter != null)
            {
                LabelRow labelRow = parameter as LabelRow;
                if (labelRow != null)
                {
                    LabelFileViewModel labelFileViewModel = (LabelFileViewModel)labelRow.Owner;
                    Clipboard.SetText($"@{labelFileViewModel.LabelHeaderName}:{labelRow.LabelId}");
                }
            }
        }
    }
}
