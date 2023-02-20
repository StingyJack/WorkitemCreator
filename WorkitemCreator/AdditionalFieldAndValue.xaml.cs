namespace WorkitemCreator
{
    using System;

    /// <summary>
    /// Interaction logic for AdditionalFieldAndValue.xaml
    /// </summary>
    public partial class AdditionalFieldAndValue
    {

        
        public AdditionalFieldAndValueViewModel ViewModel { get; }

        private AdditionalFieldAndValue()
        {
            InitializeComponent();
        }

        public AdditionalFieldAndValue(AdditionalFieldAndValueViewModel fieldAndValueViewModel) : this()
        {
            
            ViewModel = fieldAndValueViewModel ?? throw new ArgumentNullException(nameof(fieldAndValueViewModel));
            DataContext = fieldAndValueViewModel;
            //FieldName.Content = fieldAndValueViewModel.FieldName;
            //FieldValue.Text = fieldAndValueViewModel.Value;
            //FieldIsEnabled.IsChecked = fieldAndValueViewModel.IsEnabled;
            //FieldIsEligible.IsChecked = fieldAndValueViewModel.IsEligible; 
        }

        public FieldAndValue GetFieldAndValueFromDisplay()
        {
            //var fieldAndValueViewModel = new ViewModel
            //{
            //    FieldName = FieldName.Content.ToString(),
            //    Value = FieldValue.Text.Trim(),
            //    IsEnabled = FieldIsEnabled.IsChecked.GetValueOrDefault(false),
            //    IsEligible = FieldIsEligible.IsChecked.GetValueOrDefault(false)
            //};
            //return fieldAndValueViewModel;

            return ViewModel;
        }

        public void SetAvailableFields(List<WorkItemTypeFieldInstance> availableFields)
        {
            ViewModel.AvailableFields = availableFields;
        }

        public void SetIsEligible(bool isEligible)
        {
            //This control is hidden from the user
            //FieldIsEligible.IsChecked = isEligible;
            ViewModel.IsEligible = isEligible;
        }
    }
}