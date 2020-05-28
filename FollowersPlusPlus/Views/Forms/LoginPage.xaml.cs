using Acr.UserDialogs;
using FollowersPlusPlus.Views.Profile;
using InstagramApiSharp.API;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Logger;
using System;
using System.IO;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace FollowersPlusPlus.Views.Forms
{
    /// <summary>
    /// Page to login with user name and password
    /// </summary>
    [Preserve(AllMembers = true)]
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage
    {

        private static IInstaApi InstaApi = InstaApiBuilder.CreateBuilder().Build();
        const string StateFile = "state.bin";

        public LoginPage()
        {
            InitializeComponent();
            content.IsVisible = false;
            ActivateBusyIndicator();
        }

        protected async override void OnAppearing()
        {
            var session_exists = LoadSession();

            if (session_exists)
            {
                //Move to HomePage
                DeactivateBusyIndicator();
                var home_page_instance = new InstagramProfilePage();
                home_page_instance.InstaApi = InstaApi;
                Navigation.InsertPageBefore(home_page_instance, this);
                await Navigation.PopAsync();
            }
            else
            {
                content.IsVisible = true;
                DeactivateBusyIndicator();
                return;
            }
        }

        private async void Login_pressed(object sender, EventArgs e)
        {

            if (string.IsNullOrWhiteSpace(username_field.Text) || string.IsNullOrWhiteSpace(PasswordEntry.Text))
            {
                await DisplayAlert("Error", "Please type username and password", "Ok");
                return;
            }

            ActivateBusyIndicator();

            //get data from fields
            var userSession = new UserSessionData
            {
                UserName = username_field.Text,
                Password = PasswordEntry.Text
            };

            InstaApi = InstaApiBuilder.CreateBuilder()
                .SetUser(userSession)
                .UseLogger(new DebugLogger(LogLevel.All))
                .SetRequestDelay(RequestDelay.FromSeconds(0, 1))
                .Build();

            if (!InstaApi.IsUserAuthenticated)
            {
                var logInResult = await InstaApi.LoginAsync();

                if (logInResult.Succeeded)
                {
                    //save user session
                    SaveSession();
                    //Move to HomePage
                    DeactivateBusyIndicator();
                    var home_page_instance = new InstagramProfilePage();
                    home_page_instance.InstaApi = InstaApi;
                    Navigation.InsertPageBefore(home_page_instance, this);
                    await Navigation.PopAsync();
                }
                else
                {
                    // two factor is required
                    if (logInResult.Value == InstaLoginResult.TwoFactorRequired)
                    {
                        PromptResult pResult = await UserDialogs.Instance.PromptAsync(new PromptConfig
                        {
                            InputType = InputType.Number,
                            OkText = "Login",
                            Title = "Two Factor Authentication",
                        });
                        if (pResult.Ok && !string.IsNullOrWhiteSpace(pResult.Text))
                        {
                            if (InstaApi == null)
                                return;
                            var twoFactorLogin = await InstaApi.TwoFactorLoginAsync(pResult.Text);
                            if (twoFactorLogin.Succeeded)
                            {
                                // connected
                                // save session
                                SaveSession();
                                //Move to HomePage
                                DeactivateBusyIndicator();
                                var home_page_instance = new InstagramProfilePage();
                                home_page_instance.InstaApi = InstaApi;
                                Navigation.InsertPageBefore(home_page_instance, this);
                                await Navigation.PopAsync();
                            }
                            else
                            {
                                await DisplayAlert("Error", "There has been an error in login", "Ok");
                            }
                        }
                    }
                    else
                    {
                        await DisplayAlert("Error", logInResult.Info.Message, "Ok");
                    }
                }
            }
            else
            {
                await DisplayAlert("Info", "Already Connected", "Ok");
            }
        }

        void ActivateBusyIndicator()
        {
            busyindicator.IsVisible = true;
            busyindicator.IsBusy = true;
            content.IsVisible = false;
        }

        void DeactivateBusyIndicator()
        {
            busyindicator.IsBusy = false;
            busyindicator.IsVisible = false;
            content.IsVisible = true;
        }

        void SaveSession()
        {
            if (InstaApi == null)
                return;
            var state = InstaApi.GetStateDataAsStream();
            string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), StateFile);
            using (var fileStream = File.Create(fileName))
            {
                state.Seek(0, SeekOrigin.Begin);
                state.CopyTo(fileStream);
            }
            //DisplayAlert("Info", fileName, "Ok");
        }

        private bool LoadSession()
        {
            try
            {
                string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), StateFile);
                if (File.Exists(fileName))
                {

                    using (var fs = File.OpenRead(fileName))
                    {
                        InstaApi.LoadStateDataFromStream(fs);
                        //DisplayAlert("Info", "State file found", "Ok");
                        return true;
                    }
                }
                //DisplayAlert("Info", "State file not found", "Ok");
                return false;
            }
            catch (Exception ex)
            {
                DisplayAlert("Info", ex.ToString(), "Ok");
                return false;
            }
        }
    }
}