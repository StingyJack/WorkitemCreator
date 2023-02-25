﻿namespace WorkitemCreator
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

    /// <summary>
    /// Interaction logic for WorkitemTemplateViewControl.xaml
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

        public void UpdateWorkitemTypeList(List<WorkItemType> workitemTypes)
        {
            _workItemTypes = workitemTypes;

            var workitemTypeNames = workitemTypes.Select(s => s.Name);

            WorkitemType.ItemsSource = workitemTypeNames;
            foreach (TabItem ti in WorkItemChildren.Items)
            {
                var witvc = (WorkitemTemplateViewControl)ti.Content;
                witvc.UpdateWorkitemTypeList(workitemTypes);
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
                //we dont want to show things like "Area Path 1", "IterationPath 2", etc
                var lastChar = wif.Name.Substring(wif.Name.Length - 1, 1);
                if (int.TryParse(lastChar, out _))
                {
                    continue;
                }

                //also dont want to show fields in this area that already have specific inputs
                if (wif.ReferenceName.Equals("System.Title", StringComparison.OrdinalIgnoreCase)
                    || wif.ReferenceName.Equals("System.Description", StringComparison.OrdinalIgnoreCase)
                    || wif.ReferenceName.Equals("System.History", StringComparison.OrdinalIgnoreCase))
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

        #endregion //#region "Data and View helpers"
    }
}