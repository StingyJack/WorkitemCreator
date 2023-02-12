namespace WorkitemCreator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Shapes;

    /// <summary>
    /// Interaction logic for WorkitemTemplateViewControl.xaml
    /// </summary>
    public partial class WorkitemTemplateViewControl : UserControl
    {
        private readonly WorkitemTemplate _workitemTemplate;

        internal WorkitemTemplateViewControl()
        {
            InitializeComponent();
        }


        internal WorkitemTemplateViewControl(WorkitemTemplate workitemTemplate) : this()
        {
            _workitemTemplate = workitemTemplate ?? throw new ArgumentNullException(nameof(workitemTemplate));
            TemplateName.Text = _workitemTemplate.Name;
            Title.Text = _workitemTemplate.Title;
            Description.Text = _workitemTemplate.Description;
            WorkitemType.ItemsSource = Enum.GetValues<WorkitemType>();
            WorkitemType.SelectedItem = workitemTemplate.WorkitemType;

            WorkItemChildren.Items.Clear();
            foreach (var child in workitemTemplate.Children ?? new List<WorkitemTemplate>())
            {
                var childControl = new WorkitemTemplateViewControl(child);
                var childTab = new TabItem
                {
                    Header = child.Name,
                    Content = childControl
                };
                WorkItemChildren.Items.Add(childTab);
            }
        }


        public WorkitemTemplate AsTemplateDefinition()
        {
            var returnValue = new WorkitemTemplate
            {
                Name = TemplateName.Text.Trim(),
                Title = Title.Text.Trim(),
                Description = Description.Text.Trim()
            };
            foreach (TabItem ti in WorkItemChildren.Items)
            {
                var witvc = (WorkitemTemplateViewControl)ti.Content;

                var child = witvc.AsTemplateDefinition();
                returnValue.Children.Add(child);
            }

            return returnValue;
        }
    }
}
