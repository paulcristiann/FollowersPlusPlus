using InstagramApiSharp.API;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace FollowersPlusPlus.Views.Lists
{
    /// <summary>
    /// Page to show the transaction history.
    /// </summary>
    [Preserve(AllMembers = true)]
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NotFollowingMePage : ContentPage
    {

        public IInstaApi InstaApi;

        public bool Updated = false;

        public IResult<InstaCurrentUser> CurrentUserObject;

        public ObservableCollection<InstagramUser> not_following_back;

        public NotFollowingMePage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            //listView.ItemsSource = not_following_back;
        }

        async void OnListViewItemTapped(object sender, ItemTappedEventArgs e)
        {
            InstagramUser tappedItem = e.Item as InstagramUser;

            bool answer = await DisplayAlert("Warning", "Do you want to unfollow " + tappedItem.Username + "?", "Yes", "No");

            if (answer == true)
            {

                var response = await InstaApi.UserProcessor.UnFollowUserAsync(tappedItem.UserId);

                if (response.Succeeded)
                {
                    await DisplayAlert("Info", "Unfollowed " + tappedItem.Username, "Ok");
                    //Refresh list

                    not_following_back.Remove(tappedItem);
                    Updated = true;

                }
                else
                {
                    await DisplayAlert("Error", response.Info.Message, "Ok");
                }


            }

            //listView.SelectedItem = null;
        }

        protected override async void OnDisappearing()
        {
            base.OnDisappearing();
            if (Updated)
            {
                await DisplayAlert("Information", "Please refresh using the button", "Ok");
            }
        }
    }
}