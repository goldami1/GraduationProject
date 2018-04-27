﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using InnoviApiProxy;
using AgentVI.ViewModels;

namespace AgentVI.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class LoginPage : ContentPage
	{
        private LoginResult m_loginResult = null;
        private LoginPageViewModel m_ViewModel = null;

        public LoginPage ()
		{
            InitializeComponent ();
            NavigationPage.SetHasNavigationBar(this, false);
        }

        void loginButton_Clicked(object sender, EventArgs e)
        {
            string username = usernameEntry.Text;
            string password = passwordEntry.Text;
            bool isUsernameEmpty = string.IsNullOrEmpty(username)||string.IsNullOrWhiteSpace(username);
            bool isPasswordEmpty = string.IsNullOrEmpty(password) || string.IsNullOrWhiteSpace(password);

            if(isUsernameEmpty || isPasswordEmpty)
            {
                DisplayAlert("Login Error", "Please enter your username and password.", "Retry");
            }
            else
            {
                if (m_loginResult == null)
                {
                    try
                    {
                        m_loginResult = User.Login(usernameEntry.Text, passwordEntry.Text);
                    }
                    catch (Exception ex)
                    {
                        DisplayAlert("Exception", ex.Message, "Close");
                    }
                }
                if( m_loginResult != null)
                {
                    if (m_loginResult.ErrorMessage == LoginResult.eErrorMessage.Empty)
                    {
                        m_ViewModel = new LoginPageViewModel();
                        m_ViewModel.InitializeFields(m_loginResult.User);
                        Navigation.PushModalAsync(new MainPage());
                    }
                    else
                    {
                        DisplayAlert("Error Message", m_loginResult.ErrorMessage.toString(), "Retry");
                        m_loginResult = null;
                    }
                }
            }            
        }

        async void forgotPwdButton_Clicked(object sender, EventArgs e)
        {
            var response = await DisplayActionSheet("Title", "Cancel", "Delete", "Copy Link", "Duplicate Link");
            await DisplayAlert("Response", response, "OK");
        }
    }
}