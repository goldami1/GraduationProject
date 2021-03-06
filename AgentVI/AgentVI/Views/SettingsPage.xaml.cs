﻿#if DPROXY
using DummyProxy;
#else
using InnoviApiProxy;
#endif
using System;
using System.Collections.Generic;
using AgentVI.Services;
using Xamarin.Forms;
using AgentVI.Interfaces;
using AgentVI.ViewModels;

namespace AgentVI.Views
{
    public partial class SettingsPage : ContentPage, IBindable
    {
        public IBindableVM BindableViewModel => SettingsVM;
        public ContentPage ContentPage => this;
        public SettingsViewModel SettingsVM { get; private set; }
        string numOfCameras = "<nums>";
		string siteName = "<Site name>";
		string networkDatacomSolutions = "<Network Datacom Solutions>";
		                                                                                                           
        public SettingsPage()
        {
            InitializeComponent();
            
			numOfCameras = "<123>";
            siteName = "<xyz>";
            networkDatacomSolutions = "<abc>";

            ArmDisarmSwitch.IsToggled = ServiceManager.Instance.LoginService.ArmCamersSettings;
            NotificationsSwitch.IsToggled = ServiceManager.Instance.LoginService.PushNotificationsSettings;

            if (ArmDisarmSwitch.IsToggled)
            {
				DescriptionArmDisarm.Text = numOfCameras + " cameras of " + networkDatacomSolutions + ", " + siteName + " are Armed.";
            }
            else
            {
                DescriptionArmDisarm.Text = "Disarmed.";
            }

            if (NotificationsSwitch.IsToggled)
            {
				DescriptionNotifications.Text = "You will receive push notifications for " + networkDatacomSolutions + ", " + siteName + ".";
            }
            else
            {
                DescriptionNotifications.Text = "Push notifications is off.";
            }
        }

        private void OnLogoutButtonClicked(object sender, EventArgs e)
        {
            ServiceManager.Instance.LoginService.DeleteCredentials();
			InnoviApiService.Logout();
            Device.BeginInvokeOnMainThread(() => Application.Current.MainPage = new NavigationPage(new LoadingPage()));
        }

        void arm_Toggled(object sender, EventArgs e)
        {
            ServiceManager.Instance.LoginService.ArmCamersSettings = ArmDisarmSwitch.IsToggled;
            if (ArmDisarmSwitch.IsToggled)
            {
				DescriptionArmDisarm.Text = numOfCameras + " cameras of " + networkDatacomSolutions + ", " + siteName + " are Armed.";
            }
            else
            {
                DescriptionArmDisarm.Text = "Disarmed.";
            }
        }

        void Notifications_Toggled(object sender, EventArgs e)
        {
            ServiceManager.Instance.LoginService.PushNotificationsSettings = NotificationsSwitch.IsToggled;
            if (NotificationsSwitch.IsToggled)
            {
				DescriptionNotifications.Text = "You will receive push notifications for " + networkDatacomSolutions + ", " + siteName + ".";
            }
            else
            {
                DescriptionNotifications.Text = "Push notifications is off.";
            }
        }
    }
}