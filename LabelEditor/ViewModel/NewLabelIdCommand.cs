using System;
using System.Windows.Input;

namespace LabelEditor.ViewModel
{
    /// <summary>
    /// Command to create a new label
    /// </summary>
    public class NewLabelIdCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            LabelEditor.MainWindow mainWindow = (LabelEditor.MainWindow)parameter;
            System.Windows.Controls.DataGrid datagrid = mainWindow.LabelDataGrid;

            var items = mainWindow.LabelDataGrid.Items;

            datagrid.SelectedIndex = items.Count - 1;
            datagrid.ScrollIntoView(datagrid.SelectedItem);
            datagrid.Focus();
            datagrid.CurrentCell = new System.Windows.Controls.DataGridCellInfo(datagrid.SelectedItem, datagrid.Columns[0]);
        }
    }
}
