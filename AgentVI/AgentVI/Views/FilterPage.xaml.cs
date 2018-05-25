﻿using AgentVI.Models;
using AgentVI.Services;
using AgentVI.ViewModels;
using CommonServiceLocator;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
//using DummyProxy;
using InnoviApiProxy;

namespace AgentVI.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class FilterPage : CarouselPage
	{
        private FilterViewModel FilterViewModel;
        private FilterIndicatorViewModel filterIndicatorViewModel;

        public FilterPage(FilterIndicatorViewModel i_filterIndicatorViewModel)
        {
            InitializeComponent ();
            filterIndicatorViewModel = i_filterIndicatorViewModel;
            FilterViewModel = new FilterViewModel(ServiceManager.Instance.FilterService);
            BindingContext = FilterViewModel;
        }

        protected override bool OnBackButtonPressed()
        {
            filterIndicatorViewModel.SelectedFoldersNamesCache = ServiceManager.Instance.FilterService.getSelectedFoldersHirearchy();
            return base.OnBackButtonPressed();
        }

        private void Handle_FilterListItemSelected(object i_sender, SelectedItemChangedEventArgs i_itemEventArgs)
        {
            int filterDepthLabelValue = -1;
            Folder selectedFolder = i_itemEventArgs.SelectedItem as Folder;
            Label filterDepthLabel = ((ListView)i_sender).Parent.FindByName<Label>("filterNumLabel");
            if(Int32.TryParse(filterDepthLabel.Text, out filterDepthLabelValue))
            {
                using (Converters.FilterPageIDConverter a = new Converters.FilterPageIDConverter())
                {
                    filterDepthLabelValue = (int)a.ConvertBack(filterDepthLabelValue, null, null, null);
                }
            }

            FilterViewModel.fetchNextFilteringDepth(selectedFolder, ++filterDepthLabelValue);
        }
    }
}