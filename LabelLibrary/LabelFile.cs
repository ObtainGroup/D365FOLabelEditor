using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace LabelLibrary
{
    /// <summary>
    /// Represents a physical label file
    /// </summary>
    public class LabelFile
    {
        public string Filename { get; set; } = string.Empty;
        public List<Label> Labels { get; set; } = new List<Label>();
        public string Language
        {
            get
            {
                return this.Filename.Split('.')[1];
            }
        }

        /// <summary>
        /// Load labe file content
        /// </summary>
        /// <param name="filename">Name if the label file</param>
        /// <returns>Label file object instance</returns>
        public static LabelFile Load(string filename)
        {
            LabelFile file = new LabelFile() { Filename = filename };
            Label label = new Label();

            foreach (string line in File.ReadAllLines(filename))
            {
                if (string.IsNullOrWhiteSpace(line) == true)
                    continue;

                if (line.TrimStart()[0] != ';') // A line starting with a ; is a comment belonging to the previous line
                {
                    if (string.IsNullOrEmpty(label.Id) == false)
                    {
                        file.Labels.Add(label);
                        label = new Label();
                    }
                    int eqSign = line.IndexOf('=');
                    label.Id = line.Substring(0, eqSign);
                    label.Text = line.Substring(eqSign + 1);
                }
                else
                {
                    label.Comment = line.Substring(line.IndexOf(';') + 1);
                }
            }
            file.Labels.Add(label);

            return file;
        }

        /// <summary>
        /// Build the label file contents
        /// </summary>
        /// <returns>Label file contents</returns>
        protected string LabelFileContent()
        {
            StringBuilder labelFileContent = new StringBuilder();
            foreach (Label label in this.Labels)
            {
                labelFileContent.AppendLine($"{label.Id}={label.Text}");
                if (string.IsNullOrWhiteSpace(label.Comment) == false)
                {
                    labelFileContent.AppendLine($" ;{label.Comment}");
                }
            }
            return labelFileContent.ToString();
        }

        /// <summary>
        /// Checks if the label file has changed
        /// </summary>
        /// <returns>true or false</returns>
        public bool HasChanged()
        {
            string fileContent = System.IO.File.ReadAllText(this.Filename);
            string newContent = this.LabelFileContent();

            bool isEqual = fileContent.Equals(newContent);
            return isEqual == false;
        }

        /// <summary>
        /// Saves the label file
        /// </summary>
        /// <returns>true, if saved</returns>
        public bool Save()
        {
            if (this.HasChanged())
            {
                try
                {
                    System.IO.File.WriteAllText(this.Filename, this.LabelFileContent());
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Updates the labels from a label rows collection
        /// </summary>
        /// <param name="labelRows">LabelRow collection</param>
        public void UpdateLabels(ObservableCollection<LabelRow> labelRows)
        {
            // Get list from the UI
            List<Label> uiLabels = new List<Label>();
            foreach (LabelRow labelRow in labelRows)
            {
                Label uiLabel = labelRow.LanguageLabel[this.Language];
                if (uiLabel?.Text.Trim().Length > 0)
                {
                    uiLabels.Add(new Label
                    {
                        Id = labelRow.LabelId,
                        Text = uiLabel.Text,
                        Comment = uiLabel.Comment
                    });

                }
            }

            List<Label> newLabels = new List<Label>();

            foreach (Label label in Labels)
            {
                Label labelRowLabel = uiLabels.Find(uil => uil.Id == label.Id);
                if (labelRowLabel != null)
                {
                    if (labelRowLabel.Text.Trim().Length > 0)
                    {
                        newLabels.Add(labelRowLabel);
                    }
                }
            }

            var newUiLabels = uiLabels.Except(newLabels).ToList();
            if (newUiLabels != null)
            {
                newLabels = newLabels.Concat(newUiLabels).ToList();
            }

            this.Labels = newLabels;
        }
    }
}
