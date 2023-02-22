namespace WorkitemCreator
{
    using System;
    using System.Collections.Generic;
    using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

    /// <summary>
    /// Interaction logic for AdditionalFieldAndValue.xaml
    /// </summary>
    public partial class AdditionalFieldAndValue
    {
        public AdditionalFieldAndValueViewModel ViewModel { get; }

        private AdditionalFieldAndValue() => InitializeComponent();

        public AdditionalFieldAndValue(AdditionalFieldAndValueViewModel fieldAndValueViewModel) : this()
        {
            ViewModel = fieldAndValueViewModel ?? throw new ArgumentNullException(nameof(fieldAndValueViewModel));
            DataContext = fieldAndValueViewModel;

            if (ViewModel.AllFields.Count == 0)
            {
                ViewModel.AllFields.Add(new WorkItemTypeFieldInstance(){Name = ViewModel.FieldName, ReferenceName = ViewModel.FieldReferenceName});
                FieldName.SelectedIndex = 0;
            }
            //FieldName.Content = fieldAndValueViewModel.FieldName;
            //FieldValue.Text = fieldAndValueViewModel.Value;
            //FieldIsEnabled.IsChecked = fieldAndValueViewModel.IncludeWhenCreating;
            //FieldIsEligible.IsChecked = fieldAndValueViewModel.IsEligible; 
        }
        
        public void SetAvailableFields(List<WorkItemTypeFieldInstance> availableFields) => ViewModel.AllFields = availableFields;

    
    }
}