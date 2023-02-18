namespace WorkitemCreator
{
    using System;

    /// <summary>
    /// Interaction logic for AdditionalFieldAndValue.xaml
    /// </summary>
    public partial class AdditionalFieldAndValue
    {
        private AdditionalFieldAndValue()
        {
            InitializeComponent();
        }

        public AdditionalFieldAndValue(FieldAndValue fieldAndValue) : this()
        {
            _ = fieldAndValue ?? throw new ArgumentNullException(nameof(fieldAndValue));
            FieldName.Content = fieldAndValue.FieldName;
            FieldValue.Text = fieldAndValue.Value;
            FieldIsEnabled.IsChecked = fieldAndValue.IsEnabled;
            FieldIsEligible.IsChecked = fieldAndValue.IsEligible;
        }

        public FieldAndValue GetFieldAndValueFromDisplay()
        {
            var fieldAndValue = new FieldAndValue
            {
                FieldName = FieldName.Content.ToString(),
                Value = FieldValue.Text.Trim(),
                IsEnabled = FieldIsEnabled.IsChecked.GetValueOrDefault(false),
                IsEligible = FieldIsEligible.IsChecked.GetValueOrDefault(false)
            };
            return fieldAndValue;
        }

        public void SetIsEligible(bool isEligible)
        {
            FieldIsEligible.IsChecked = isEligible;
        }
    }
}