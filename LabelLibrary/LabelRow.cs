using LabelLibrary;
using System;
using System.Collections.Generic;
using System.Text;

namespace LabelLibrary
{
    /// <summary>
    /// A row of labels in the editor
    /// </summary>
    public class LabelRow
    {
        public object Owner { get; set; } = null;
        public event EventHandler<String> OnSettingLabelId;
        private string labelId = String.Empty;

        public string LabelId 
        { 
            get => labelId;
            set
            {
                labelId = value;
                OnSettingLabelId?.Invoke(this, labelId);
            }
        }
        
        public Dictionary<string, Label> LanguageLabel { get; set; } = new Dictionary<string, LabelLibrary.Label>();

        /// <summary>
        /// Builds the target in which searches are made against
        /// </summary>
        public string SearchTarget
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(labelId);
                foreach (var item in LanguageLabel)
                {
                    sb.Append(item.Value.Text);
                }
                return sb.ToString();
            }
        }

        public LabelRow() : this(null)
        {

        }

        public LabelRow(object owner)
        {
            Owner = owner;
        }
        
    }
}
