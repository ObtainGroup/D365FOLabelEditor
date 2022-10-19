using LabelLibrary;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace LabelEditor.ViewModel
{
    /// <summary>
    /// A class holding the different label files for each language
    /// </summary>
    public class LabelFileViewModel
    {
        public LabelRow CurrentLabelRow { get; set; }
        public string LabelHeaderName { get; private set; } = string.Empty;
        public List<LabelFile> LabelFiles { get; private set; } = new List<LabelFile>();
        public ObservableCollection<LabelRow> LabelRows { get; set; } = new ObservableCollection<LabelRow>();

        public LabelRow NewLabelRow(LabelRow labelRow = null)
        {
            LabelRow row = labelRow ?? new LabelRow();
            row.Owner = this;

            foreach (LabelFile labelFile in LabelFiles) 
            {
                row.LanguageLabel.Add(labelFile.Language, new Label());
            }
            return row;
        }

        /// <summary>
        /// Combines the found label files (languages) and builds the LabelRow
        /// </summary>
        protected void BuildLabelRows()
        {
            IEnumerable<string> allLabelIds = new List<string>();
            foreach (LabelFile labelFile in LabelFiles)
            {
                var labelIds = from label in labelFile.Labels select label.Id;

                allLabelIds = allLabelIds.Union(labelIds);
            }

            foreach (string labelId in allLabelIds)
            {
                LabelRow labelRow = new LabelRow(this);
                labelRow.LabelId = labelId;
                labelRow.LanguageLabel = new Dictionary<string, Label>();

                foreach (LabelFile labelFile in LabelFiles)
                {
                    Label label = labelFile.Labels.Find(x => x.Id == labelId);

                    if (label == null)
                    {
                        label = new Label() { Id = labelId };
                    }

                    labelRow.LanguageLabel.Add(labelFile.Language, label);
                }

                LabelRows.Add(labelRow);
            }

        }

        /// <summary>
        /// Creates the view model initializing it with the contents of the label filename
        /// </summary>
        /// <param name="filename">xml filename as base file</param>
        /// <param name="labelRessourcesFolderName">Subfolder of the txt files</param>
        /// <returns></returns>
        public static LabelFileViewModel Create(string filename, string labelRessourcesFolderName = "LabelResources")
        {
            string labelFilePath = filename.Substring(0, filename.LastIndexOf('\\'));
            string resourceFolderPath;

            LabelFileViewModel labelFileSet = new LabelFileViewModel();
            if (System.IO.Path.GetExtension(filename).ToLower() == ".xml")
            {
                // Get the name part without the language part
                string nameWithoutExt = System.IO.Path.GetFileNameWithoutExtension(filename);
                int languageSep = nameWithoutExt.LastIndexOf('_');
                labelFileSet.LabelHeaderName = nameWithoutExt.Substring(0, languageSep);

                resourceFolderPath = System.IO.Path.Combine(labelFilePath, labelRessourcesFolderName);

                foreach (string path in System.IO.Directory.GetDirectories(resourceFolderPath))
                {
                    string language = path.Substring(path.LastIndexOf('\\') + 1);
                    string labelfilename = System.IO.Path.Combine(path, $"{labelFileSet.LabelHeaderName}.{language}.label.txt");

                    if (System.IO.File.Exists(labelfilename))
                    {
                        LabelFile labelFile = LabelFile.Load(labelfilename);
                        labelFileSet.LabelFiles.Add(labelFile);
                    }
                }

                labelFileSet.BuildLabelRows();
            }

            return labelFileSet;
        }

        /// <summary>
        /// Save the label file
        /// </summary>
        /// <returns>False, if it fails, otherwise true</returns>
        public bool Save()
        {
            bool ok = true;

            foreach (LabelFile labelFile in LabelFiles)
            {
                labelFile.UpdateLabels(LabelRows);
                ok = labelFile.Save() && ok;
            }

            return ok;
        }

        public ICommand CopyLabelIdCommand => new CopyLabelIdCommand();
        public ICommand NewLabelIdCommand => new NewLabelIdCommand();
        public ICommand TranslateLabelCommand => new TranslateLabelCommand();
    }
}
