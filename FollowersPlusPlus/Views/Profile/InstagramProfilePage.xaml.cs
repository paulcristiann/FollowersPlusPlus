using FollowersPlusPlus.Models;
using FollowersPlusPlus.ViewModels.ToDelete;
using FollowersPlusPlus.Views.Forms;
using FollowersPlusPlus.Views.Lists;
using InstagramApiSharp;
using InstagramApiSharp.API;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using Syncfusion.DataSource.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace FollowersPlusPlus.Views.Profile
{
    [Preserve(AllMembers = true)]
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class InstagramProfilePage : ContentPage
    {

        public IInstaApi InstaApi;

        private IResult<InstaCurrentUser> CurrentUserObject;

        List<InstaUserShort> not_following_back = new List<InstaUserShort>();
        List<InstaUserShort> i_dont_follow_back = new List<InstaUserShort>();

        const string StateFile = "state.bin";

        const int _downloadImageTimeoutInSeconds = 15;
        readonly HttpClient _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(_downloadImageTimeoutInSeconds) };

        public InstagramProfilePage()
        {
            InitializeComponent();
            ActivateBusyIndicator();
        }

        protected override void OnAppearing()
        {
            var Page = Navigation.NavigationStack.Last();

            if (Page.GetType() == typeof(InstagramProfilePage))
            {
                Refresh_Data(this, null);
            }
        }
        async Task<byte[]> DownloadImageAsync(string imageUrl)
        {
            try
            {
                using (var httpResponse = await _httpClient.GetAsync(imageUrl))
                {
                    if (httpResponse.StatusCode == HttpStatusCode.OK)
                    {
                        return await httpResponse.Content.ReadAsByteArrayAsync();
                    }
                    else
                    {
                        //Url is Invalid
                        return null;
                    }
                }
            }
            catch (Exception e)
            {
                //Handle Exception
                return null;
            }
        }

        void ActivateBusyIndicator()
        {
            busyindicator.IsVisible = true;
            busyindicator.IsBusy = true;
            page_content.IsVisible = false;
        }

        void DeactivateBusyIndicator()
        {
            busyindicator.IsBusy = false;
            busyindicator.IsVisible = false;
            page_content.IsVisible = true;
        }

        private async void dont_follow_you_button_Clicked(object sender, EventArgs e)
        {
            var not_followin_back_page_instance = new NotFollowingMePage();
            not_followin_back_page_instance.InstaApi = InstaApi;
            not_followin_back_page_instance.CurrentUserObject = CurrentUserObject;

            ObservableCollection<InstagramUser> usernames = new ObservableCollection<InstagramUser>();

            foreach (var u in not_following_back)
            {
                var added_user = new InstagramUser
                {
                    FullName = u.FullName,
                    Username = u.UserName,
                    ImageUrl = u.ProfilePicUrl,
                    UserId = u.Pk
                };
                usernames.Add(added_user);
            }

            not_followin_back_page_instance.not_following_back = usernames;

            await Navigation.PushAsync(not_followin_back_page_instance);
        }

        private async void you_dont_follow_button_Clicked(object sender, EventArgs e)
        {
            var you_dont_follow_page_instance = new YouDontFollowPage();
            you_dont_follow_page_instance.InstaApi = InstaApi;

            var usernames = new List<InstagramUser>();

            foreach (var u in i_dont_follow_back)
            {
                var added_user = new InstagramUser
                {
                    FullName = u.FullName,
                    Username = u.UserName,
                    ImageUrl = u.ProfilePicUrl
                };
                usernames.Add(added_user);
            }

            you_dont_follow_page_instance.i_dont_follow_back = usernames;
            await Navigation.PushAsync(you_dont_follow_page_instance);
        }

        private async void Logout_Clicked(object sender, EventArgs e)
        {
            try
            {
                string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), StateFile);
                if (File.Exists(fileName))
                {
                    //delete file and go back to login
                    File.Delete(fileName);
                    //Move to Login Page
                    var login_page_instance = new LoginPage();
                    Navigation.InsertPageBefore(login_page_instance, this);
                    _ = await Navigation.PopAsync();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Info", ex.ToString(), "Ok");
            }
        }

        private async void Refresh_Data(object sender, EventArgs e)
        {
            try
            {
                ActivateBusyIndicator();

                var pvm = new ProfileViewModel();

                i_dont_follow_back.Clear();
                not_following_back.Clear();

                //Get all user Information
                CurrentUserObject = await InstaApi.GetCurrentUserAsync();

                //Set fields
                var user_photo_byte = await DownloadImageAsync(CurrentUserObject.Value.ProfilePicture);
                string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "usr_photo.jpg");
                if (user_photo_byte != null)
                {
                    File.WriteAllBytes(fileName, user_photo_byte);
                    userAvatar.Source = ImageSource.FromFile(fileName);
                }
                else
                {
                    //userAvatar.Source = Syncfusion.XForms.AvatarView.AvatarCharacter.Avatar2;
                }

                full_name_label.Text = CurrentUserObject.Value.FullName;
                username_label.Text = CurrentUserObject.Value.UserName;

                var followers = await InstaApi.UserProcessor.GetUserFollowersAsync(CurrentUserObject.Value.UserName,
                PaginationParameters.MaxPagesToLoad(5));

                var following = await InstaApi.UserProcessor.GetUserFollowingAsync(CurrentUserObject.Value.UserName,
                PaginationParameters.MaxPagesToLoad(5));

                //Build not following back
                var followers_array = followers.Value.ToArray();
                var following_array = following.Value.ToArray();

                float ratio = (float)followers_array.Count() / (float)following_array.Count();

                followers_ratio.Text = ratio.ToString("0.00");
                user_followers.Text = followers_array.Count().ToString();
                user_following.Text = following_array.Count().ToString();

                foreach (var user in following_array)
                {
                    if (!followers_array.Contains(user))
                    {
                        //await DisplayAlert("Info", user.FullName, "Ok");
                        not_following_back.Add(user);
                    }
                }

                foreach (var user in followers_array)
                {
                    if (!following_array.Contains(user))
                    {
                        //await DisplayAlert("Info", user.FullName, "Ok");
                        i_dont_follow_back.Add(user);
                    }
                }

                pvm.CardItems.ElementAt(0).Preview = not_following_back.Count.ToString();
                pvm.CardItems.ElementAt(1).Preview = i_dont_follow_back.Count.ToString();

                menu_list.BindingContext = pvm;
                menu_list.ItemsSource = pvm.CardItems;

                DeactivateBusyIndicator();
            }


            catch (Exception ex)
            {
                await DisplayAlert("Error", "Can not connect", "Ok");
                await DisplayAlert("Error", e.ToString(), "Ok");
                //redirect to trap page
            }
        }

        private async void menu_list_ItemTapped(object sender, Syncfusion.ListView.XForms.ItemTappedEventArgs e)
        {
            AppFeature tappedItem = e.ItemData as AppFeature;

            switch (tappedItem.Title)
            {
                case "Users not following you back":
                    var not_followin_back_page_instance = new NotFollowingMePage();
                    not_followin_back_page_instance.InstaApi = InstaApi;
                    not_followin_back_page_instance.CurrentUserObject = CurrentUserObject;

                    ObservableCollection<InstagramUser> usernames = new ObservableCollection<InstagramUser>();

                    foreach (var u in not_following_back)
                    {
                        var added_user = new InstagramUser
                        {
                            FullName = u.FullName,
                            Username = u.UserName,
                            ImageUrl = u.ProfilePicUrl,
                            UserId = u.Pk
                        };
                        usernames.Add(added_user);
                    }

                    not_followin_back_page_instance.not_following_back = usernames;
                    await Navigation.PushAsync(not_followin_back_page_instance);
                    break;
                case "Users you don't follow back":
                    var you_dont_follow_page_instance = new YouDontFollowPage();
                    you_dont_follow_page_instance.InstaApi = InstaApi;

                    var usernames1 = new List<InstagramUser>();

                    foreach (var u in i_dont_follow_back)
                    {
                        var added_user = new InstagramUser
                        {
                            FullName = u.FullName,
                            Username = u.UserName,
                            ImageUrl = u.ProfilePicUrl
                        };
                        usernames1.Add(added_user);
                    }

                    you_dont_follow_page_instance.i_dont_follow_back = usernames1;
                    await Navigation.PushAsync(you_dont_follow_page_instance);
                    break;
            }
        }
    }
}