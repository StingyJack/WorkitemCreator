namespace WorkitemCreator
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

    /// <summary>
    ///     Interaction logic for WorkitemTemplateViewControl.xaml
    /// </summary>
    public partial class WorkitemTemplateViewControl
    {
        #region "fields and ctors"

        private List<WorkItemType> _workItemTypes = new List<WorkItemType>();

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
            WorkitemType.ItemsSource = new List<string> { workitemTemplate.WorkitemType };
            WorkitemType.SelectedItem = workitemTemplate.WorkitemType;

            AdditionalFields.Children.Clear();
            foreach (var af in workitemTemplate.AdditionalFields.OrderBy(k => k.Key))
            {
                var afavm = new AdditionalFieldAndValueViewModel
                {
                    FieldName = af.Key,
                    Value = af.Value,
                    IncludeWhenCreating = false
                };
                var afUserControl = new AdditionalFieldAndValue(afavm);
                AdditionalFields.Children.Add(afUserControl);
            }

            WorkItemChildren.Items.Clear();
            WorkItemChildren.Visibility = Visibility.Collapsed;
            foreach (var child in workitemTemplate.Children ?? new List<WorkitemTemplate>())
            {
                var childControl = new WorkitemTemplateViewControl(child);
                var childTab = new TabItem
                {
                    Header = child.Name,
                    Content = childControl
                };
                WorkItemChildren.Items.Add(childTab);
                WorkItemChildren.Visibility = Visibility.Visible;
            }
        }

        #endregion //#region "fields and ctors"

        #region "event handlers"

        private void WorkitemType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var workItemType = _workItemTypes.FirstOrDefault(w => string.Equals(w.Name, WorkitemType.SelectedItem.ToString(), StringComparison.OrdinalIgnoreCase));

            if (workItemType == null)
            {
                return;
            }

            SetEligibleAdditionalFields(workItemType.Fields.ToList());
        }

        #endregion //#region "event handlers"

        #region "Data and View helpers"

        public WorkitemTemplate AsTemplateDefinition(bool forSavingOnly)
        {
            var returnValue = new WorkitemTemplate
            {
                Name = TemplateName.Text.Trim(),
                Title = Title.Text.Trim(),
                Description = Description.Text.Trim(),
                WorkitemType = WorkitemType.Text.Trim()
            };

            foreach (AdditionalFieldAndValue afav in AdditionalFields.Children)
            {
                var viewModel = afav.ViewModel;
                if (viewModel.IsEligible == false)
                {
                    continue;
                }

                if (forSavingOnly == false && viewModel.IncludeWhenCreating == false)
                {
                    continue;
                }


                returnValue.AdditionalFields.Add(viewModel.FieldName, viewModel.Value);
            }

            foreach (TabItem ti in WorkItemChildren.Items)
            {
                var witvc = (WorkitemTemplateViewControl)ti.Content;

                var child = witvc.AsTemplateDefinition(forSavingOnly);
                returnValue.Children.Add(child);
            }

            return returnValue;
        }

        public void UpdateWithProjectConfiguration(List<WorkItemType> workitemTypes, List<WorkItemClassificationNode> iterationNodes, List<WorkItemClassificationNode> areaPathNodes)
        {
            _workItemTypes = workitemTypes;

            var workitemTypeNames = workitemTypes.Select(s => s.Name);

            WorkitemType.ItemsSource = workitemTypeNames;
            var iterations = new Dictionary<int, string>();
            IterationPath.ItemsSource = FlattenClassificationNodes(iterationNodes, ref iterations);

            var areaPaths = new Dictionary<int, string>();
            AreaPath.ItemsSource = FlattenClassificationNodes(areaPathNodes, ref areaPaths);


            foreach (TabItem ti in WorkItemChildren.Items)
            {
                var witvc = (WorkitemTemplateViewControl)ti.Content;
                witvc.UpdateWithProjectConfiguration(workitemTypes, iterationNodes, areaPathNodes);
            }

            if (WorkitemType.SelectedIndex < 0 && WorkitemType.Items.Count > 0)
            {
                WorkitemType.SelectedIndex = 0;
            }
            else
            {
                var workitemType = workitemTypes.FirstOrDefault(w => w.Name.Equals(WorkitemType.SelectedItem.ToString().Trim(), StringComparison.OrdinalIgnoreCase));
                if (workitemType == null)
                {
                    Trace.TraceWarning($"The selected workitem type {WorkitemType.SelectedItem} is invalid for this project");
                    return;
                }

                SetEligibleAdditionalFields(workitemType.Fields.ToList());
            }
        }

        private static Dictionary<int, string> FlattenClassificationNodes(List<WorkItemClassificationNode> treeNodes, ref Dictionary<int, string> collectedNodes)
        {
            foreach (var tn in treeNodes)
            {
                collectedNodes.Add(tn.Id, tn.Path);
                if (tn.HasChildren.HasValue && tn.HasChildren.Value)
                {
                    //collectedNodes.AddRange(
                    FlattenClassificationNodes(tn.Children.ToList(), ref collectedNodes);
                    //);
                }
            }

            return collectedNodes;
        }

        public void SetEligibleAdditionalFields(List<WorkItemTypeFieldInstance> workItemFields)
        {
            var fieldsAlreadyAdded = new List<AdditionalFieldAndValueViewModel>();

            foreach (AdditionalFieldAndValue afav in AdditionalFields.Children)
            {
                var viewModel = afav.ViewModel;
                var wiField = workItemFields.FirstOrDefault(f => f.Name.Equals(viewModel.FieldName, StringComparison.OrdinalIgnoreCase));
                if (wiField == null)
                {
                    viewModel.IsEligible = false;
                    continue;
                }

                viewModel.FieldReferenceName = wiField.ReferenceName;
                viewModel.IncludeWhenCreating = true;
                viewModel.IsEligible = true;
                fieldsAlreadyAdded.Add(viewModel);
            }

            foreach (var wif in workItemFields.OrderBy(f => f.Name))
            {
                if (IsFieldPreventedFromBeingShownInAdditionalFields(wif))
                {
                    continue;
                }

                if (fieldsAlreadyAdded.Any(f => string.Equals(f.FieldReferenceName, wif.ReferenceName, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                var afavm = new AdditionalFieldAndValueViewModel
                {
                    FieldName = wif.Name,
                    FieldReferenceName = wif.ReferenceName,
                    IncludeWhenCreating = false,
                    IsEligible = true
                };
                var afUserControl = new AdditionalFieldAndValue(afavm);
                AdditionalFields.Children.Add(afUserControl);
                fieldsAlreadyAdded.Add(afavm);
            }
        }

        private static bool IsFieldPreventedFromBeingShownInAdditionalFields(WorkItemFieldReference wif)
        {
            //TODO: this can be moved to configuration.

            //we dont want to show things like "Area Path 1", "IterationPath 2", etc
            var lastChar = wif.Name.Substring(wif.Name.Length - 1, 1);
            if (int.TryParse(lastChar, out _))
            {
                return true;
            }

            //also dont want to show fields in this area that already have specific inputs from this program
            // or that are not appropriate when creating a workitem
            if (wif.ReferenceName.Equals("System.Title", StringComparison.OrdinalIgnoreCase)
                || wif.ReferenceName.Equals("System.Description", StringComparison.OrdinalIgnoreCase)
                || wif.ReferenceName.Equals("System.History", StringComparison.OrdinalIgnoreCase)
                || wif.ReferenceName.Equals("System.IterationPath", StringComparison.OrdinalIgnoreCase)
                || wif.ReferenceName.Equals("System.AreaPath", StringComparison.OrdinalIgnoreCase)
                || wif.ReferenceName.Equals("System.TeamProject", StringComparison.OrdinalIgnoreCase)
                || wif.ReferenceName.Equals("System.Resolution", StringComparison.OrdinalIgnoreCase)
                || wif.ReferenceName.Equals("System.IntegrationBuild", StringComparison.OrdinalIgnoreCase)
                || wif.ReferenceName.Equals("System.Parent", StringComparison.OrdinalIgnoreCase)
                || wif.ReferenceName.Equals("System.State", StringComparison.OrdinalIgnoreCase)
                || wif.ReferenceName.Equals("System.Reason", StringComparison.OrdinalIgnoreCase)
                || wif.ReferenceName.Equals("System.ResolvedReason", StringComparison.OrdinalIgnoreCase)
                || wif.ReferenceName.Equals("System.AuthorizedAs", StringComparison.OrdinalIgnoreCase)
                || wif.ReferenceName.Equals("System.Rev", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            //also dont need to show change tracking fields or calculated fields like "Count"s or system assigned fields like IDs
            var splits = wif.Name.Split(' ');
            var lastWord = splits[splits.Length - 1];
            if ("Date".Equals(lastWord, StringComparison.OrdinalIgnoreCase)
                || "By".Equals(lastWord, StringComparison.OrdinalIgnoreCase)
                || "Count".Equals(lastWord, StringComparison.OrdinalIgnoreCase)
                || "ID".Equals(lastWord, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            //"Board Column", "Board Column Done", "Board Lane"
            if (splits.Any(s => "Board".Equals(s, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            //"Node Name"
            if (splits.Any(s => "Node".Equals(s, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            return false;
        }

        #endregion //#region "Data and View helpers"
    }
}