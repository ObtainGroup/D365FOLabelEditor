using LabelEditor.ViewModel;

using LabelLibrary;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;


namespace LabelEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string LabelColumnConfigFile = "LabelEditor.Columns.json";
        private static CollectionViewSource collectionViewSource;

        public LabelFileViewModel labelFileViewModel { get; set; } = new LabelFileViewModel();

        public bool IsAdministrator => new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

        public MainWindow()
        {
            InitializeComponent();
            if (Settings.Filename != null)
            {
                this.Load(Settings.Filename);
            }
            this.LoadSettings();

            if (this.IsAdministrator == false)
            {
                MessageBox.Show("Saving labels require Obtain Label Editor to be run as Administrator", "Run As Adminstrator", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
     
        private void LoadSettings()
        {
            try
            {
                string pos = Settings.WindowPosition;
                if (pos != null)
                {
                    this.Left = Convert.ToInt32(pos.Split(',')[0]);
                    this.Top = Convert.ToInt32(pos.Split(',')[1]);
                }
            }
            catch 
            { 
                // Don't do anything
            }
            
            try
            {
                string size = Settings.WindowSize;
                if (size != null)
                {
                    this.Width = Convert.ToInt32(size.Split(',')[0]);
                    this.Height = Convert.ToInt32(size.Split(',')[1]);
                }
            }
            catch
            {
                // Don't do anything
            }
        }

        private void Load(string filename)
        {
            if (System.IO.File.Exists(filename))
            {
                labelFileViewModel = LabelFileViewModel.Create(filename);
                this.DataContext = labelFileViewModel;
                this.Title = $"Obtain Label Editor - {labelFileViewModel.LabelHeaderName}";

                LabelDataGrid.Columns.Clear();

                DataGridTextColumn labelIdColumn = new DataGridTextColumn();
                labelIdColumn.Header = "Label ID";
                labelIdColumn.Binding = new Binding
                {
                    Path = new PropertyPath("LabelId"),
                    Mode = BindingMode.TwoWay,
                    ValidatesOnExceptions = true
                };

                List<Column> columnsDef = new List<Column>();

                LabelDataGrid.Columns.Add(labelIdColumn);
                if (System.IO.File.Exists(LabelColumnConfigFile))
                {
                    string json = System.IO.File.ReadAllText(LabelColumnConfigFile);

                    try
                    {
                        columnsDef = JsonConvert.DeserializeObject<List<Column>>(json);
                    }
                    catch { }
                }

                foreach (LabelFile labelFile in labelFileViewModel.LabelFiles)
                {
                    DataGridTextColumn dataGridTextColumn = new DataGridTextColumn();
                    dataGridTextColumn.Header = labelFile.Language;

                    dataGridTextColumn.Binding = new Binding
                    {
                        Path = new PropertyPath($"LanguageLabel[{labelFile.Language}].Text"),
                        Mode = BindingMode.TwoWay
                    };
                    dataGridTextColumn.CanUserSort = true;

                    if (columnsDef.Count > 0)
                    {
                        var cdef = columnsDef.Find( c => c.Name == $"{labelFileViewModel.LabelHeaderName}:{labelFile.Language}");
                        if (cdef?.Width>0)
                        { 
                            dataGridTextColumn.Width = cdef.Width;
                        }
                    }
                    LabelDataGrid.Columns.Add(dataGridTextColumn);
                    //LabelDataGrid.ContextMenu = new ContextMenu { it}
                }
            }
        }

        //protected ContextMenu BuildContextMenu()
        //{
        //    ContextMenu menu = new ContextMenu();
        //    menu.Items.Add  
        //}

        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            if (labelFileViewModel.Save() == false)
            {
                if (this.IsAdministrator == false)
                {
                    MessageBox.Show("Label files could not be saved. Try running Obtain Label Editor as Administrator.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show("Label files could not be saved.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void LoadButtonClick(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = "*.xml";
            dlg.Filter = "Label files (*.xml)|*.xml";
            if (dlg.ShowDialog() == true)
            {
                this.Load(dlg.FileName);
                Settings.Filename = dlg.FileName;
            }
        }

        private void LabelDataGrid_InitializingNewItem(object sender, InitializingNewItemEventArgs e)
        {
            DataGrid datagrid = sender as DataGrid;
            if (datagrid != null)
            {
                labelFileViewModel.NewLabelRow((LabelRow)e.NewItem);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            Settings.WindowSize = $"{this.Width},{this.Height}";
            Settings.WindowPosition = $"{this.Left},{this.Top}";

            List<Column> columns = new List<Column>();
            foreach (DataGridTextColumn gridColumn in LabelDataGrid.Columns)
            {
                columns.Add(new Column { Name = $"{labelFileViewModel.LabelHeaderName}:{gridColumn.Header}", Width = (int)gridColumn.Width.Value});
            }

            if (columns.Count > 0)
            {
                string json = JsonConvert.SerializeObject(columns);
                System.IO.File.WriteAllText(LabelColumnConfigFile, json);
            }
            
        }

        private void LabelDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            labelFileViewModel.CurrentLabelRow = (sender as DataGrid).CurrentItem as LabelRow;
        }

        private void FilterBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (collectionViewSource == null)
                {
                    collectionViewSource = new CollectionViewSource() { Source = labelFileViewModel.LabelRows };
                }
                ICollectionView itemList = collectionViewSource.View;

                var filter = new Predicate<object>(l => ((LabelRow)l).SearchTarget.ToLower().Contains(FilterBox.Text.ToLower()));
                itemList.Filter = filter;

                LabelDataGrid.ItemsSource = itemList;
            }
        }
    }
}
