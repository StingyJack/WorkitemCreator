namespace WorkitemCreator
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for WorkitemTemplateViewControl.xaml
    /// </summary>
    public partial class WorkitemTemplateViewControl
    {
        internal WorkitemTemplateViewControl()
        {
            InitializeComponent();
        }


        internal WorkitemTemplateViewControl(WorkitemTemplate workitemTemplate) : this()
        {
            var workitemTemplate1 = workitemTemplate ?? throw new ArgumentNullException(nameof(workitemTemplate));
            TemplateName.Text = workitemTemplate1.Name;
            Title.Text = workitemTemplate1.Title;
            Description.Text = workitemTemplate1.Description;
            WorkitemType.ItemsSource = Enum.GetValues(typeof(WorkitemType));
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
