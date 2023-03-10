namespace WorkitemCreator
{
    using System;
    using System.Windows.Controls;

    /// <summary>
    ///     Interaction logic for AdditionalFieldAndValue.xaml
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
        }

        private void FieldValue_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel.IncludeWhenCreating = string.IsNullOrWhiteSpace(FieldValue.Text) == false;
        }
    }
}