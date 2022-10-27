using LabelLibrary;

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace LabelEditor.ViewModel
{
    /// <summary>
    /// Translates the focused cell
    /// </summary>
    public class TranslateLabelCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        /// <summary>
        /// Look for child element
        /// </summary>
        /// <typeparam name="childItem">Child Item</typeparam>
        /// <param name="obj">Dependency Object</param>
        /// <returns>The child or null</returns>
        private childItem FindVisualChild<childItem>(DependencyObject obj) where childItem : DependencyObject
        {
            if (obj == null)
            {
                return null;
            }

            int childCount = VisualTreeHelper.GetChildrenCount(obj);

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);

                if (child != null && child is childItem)
                {
                    return (childItem)child;
                }
                else
                {
                    childItem childOfChild = this.FindVisualChild<childItem>(child);

                    if (childOfChild != null)
                    {
                        return childOfChild;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Get the cell of the datagrid.
        /// </summary>
        /// <param name="dataGrid">The data grid in question</param>
        /// <param name="cellInfo">The cell information for a row of that datagrid</param>
        /// <param name="cellIndex">The row index of the cell to find. </param>
        /// <returns>The cell or null</returns>
        private DataGridCell TryToFindGridCell(DataGrid dataGrid, DataGridCellInfo cellInfo, int cellIndex = -1)
        {
            DataGridRow row;
            DataGridCell result = null;

            if (cellIndex < 0)
            {
                row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromItem(cellInfo.Item);
            }
            else
            {
                row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(cellIndex);
            }

            if (row != null)
            {
                int columnIndex = dataGrid.Columns.IndexOf(cellInfo.Column);

                if (columnIndex > -1)
                {
                    DataGridCellsPresenter presenter = this.FindVisualChild<DataGridCellsPresenter>(row);

                    if (presenter != null)
                    {
                        result = presenter.ItemContainerGenerator.ContainerFromIndex(columnIndex) as DataGridCell;
                    }
                    else
                    {
                        result = null;
                    }
                }
            }
            
            return result;
        }

        public void Execute(object parameter)
        {
            // https://stackoverflow.com/questions/22606700/find-the-focused-cell-from-a-datagrid
            DataGrid grid = parameter as DataGrid;

            if (grid != null)
            {

                Translator.Translator translator = new Translator.Translator();
                var selectedCells = grid.SelectedCells;

                // Collect columns to translate
                foreach (var cellInfo in selectedCells)
                {
                    if (cellInfo.Column.SortMemberPath != "LabelId")
                    { 
                        var cell = this.TryToFindGridCell(grid, cellInfo);
                        if (cell.IsFocused)
                        {
                            translator.TextToTranslate = ((TextBlock)cell.Content).Text;
                            translator.From = cellInfo.Column.Header.ToString();
                        }
                        else if (cellInfo.Column.SortMemberPath != "LabelId")
                        {
                            if (cell.Content is TextBlock)
                            {
                                if (((TextBlock)cell.Content).Text.Trim().Length == 0)
                                {
                                    translator.To.Add(cellInfo.Column.Header.ToString());
                                }
                            }
                            else if (cell.Content is TextBox)
                            {
                                if (((TextBox)cell.Content).Text.Trim().Length == 0)
                                {
                                    translator.To.Add(cellInfo.Column.Header.ToString());
                                }
                            }
                        }
                    }
                }
                
                if (translator.TextToTranslate != string.Empty && translator.To.Count > 0)
                {
                    try
                    {
                        var translations = translator.Translate().Result;
                        LabelRow labelRow = (LabelRow)selectedCells[0].Item;

                        foreach (var cellInfo in selectedCells)
                        {
                            if (cellInfo.Column.SortMemberPath != "LabelId")
                            {
                                string colId = cellInfo.Column.Header.ToString();
                                string cid;
                                // Language ID mapping as the D365FO ids do not always match the ones returned by the Azure Translator
                                switch (colId.ToLower())
                                {
                                    case "no":
                                        cid = "nb";
                                        break;
                                    case "en-us":
                                        cid = "en";
                                        break;
                                    default:
                                        cid = colId.ToLower();
                                        break;
                                }

                                if (translations.ContainsKey(cid))
                                {
                                    var setCell = this.TryToFindGridCell(grid, cellInfo);
                                    ((TextBlock)setCell.Content).Text = translations[cid];
                                    ((LabelRow)setCell.DataContext).LanguageLabel[colId].Text = translations[cid];
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        if (MessageBox.Show($"An error occurred when trying to translate text\nDid you add the translation keys to the config file?\n\nMessage:\n{(e.InnerException != null ? e.InnerException.Message : e.Message)}" +
                            "\n\nClick OK to see documentation", "Error tranlating", MessageBoxButton.OKCancel, MessageBoxImage.Error) == MessageBoxResult.OK)
                        {
                            System.Diagnostics.Process.Start("https://github.com/ObtainGroup/D365FOLabelEditor/discussions/3");
                        }
                    }
                }
            }
        }
    }
}
