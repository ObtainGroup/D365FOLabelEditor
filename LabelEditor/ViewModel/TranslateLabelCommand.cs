using LabelLibrary;

using System;
using System.Collections.Generic;
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
            if (parameter is DataGrid grid)
            {
                var item = (LabelLibrary.LabelRow)grid.CurrentCell.Item;
                var column = grid.CurrentCell.Column;

                var labels = item.LanguageLabel;
                string currentLanguage = column.Header.ToString();

                if (labels.TryGetValue(currentLanguage, out LabelLibrary.Label currentLabel))
                {
                    Translator.Translator translator = new Translator.Translator
                    {
                        TextToTranslate = currentLabel.Text,
                        From = currentLanguage
                    };

                    List<string> languages = new List<string>(labels.Keys);
                    foreach (var language in languages)
                    {
                        if (language != currentLanguage)
                        {
                            translator.To.Add(language);
                        }
                    }

                    if (translator.TextToTranslate != string.Empty && translator.To.Count > 0)
                    {

                        try
                        {
                            var selectedCells = grid.SelectedCells;
                            var translations = translator.Translate().Result;

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
                        catch
                        {
                            if (MessageBox.Show("An error occurred when trying to translate text\nDid you add the translation keys to the config file?\n" +
                                "Click OK to see documentation", "Error tranlating", MessageBoxButton.OKCancel, MessageBoxImage.Error) == MessageBoxResult.OK)
                            {
                                System.Diagnostics.Process.Start("https://github.com/ObtainGroup/D365FOLabelEditor/discussions/3");
                            }
                        }
                    }
                }
            }
        }
    }
}
