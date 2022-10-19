using LabelEditor.ViewModel;

using LabelLibrary;

using System.Collections.Generic;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace LabelEditor
{
    public class LabelRowValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            LabelRow labelRow = (value as BindingGroup).Items[0] as LabelRow;

            if (labelRow != null)
            {
                if (string.IsNullOrWhiteSpace(labelRow.LabelId))
                {
                    return new ValidationResult(false, "Label ID must be specified");
                }

                List<LabelRow> labelRowList = new List<LabelRow>(((LabelFileViewModel)labelRow.Owner).LabelRows);
                int duplicates = labelRowList.FindAll(l => l.LabelId == labelRow.LabelId).Count;
                if (duplicates > 1)
                {
                    return new ValidationResult(false, "Label ID already exist");
                }
            }
            return ValidationResult.ValidResult;
        }
    }
}
