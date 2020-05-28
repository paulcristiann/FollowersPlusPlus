using InstagramApiSharp.API;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace FollowersPlusPlus
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class YouDontFollowPage : ContentPage
    {
        public IInstaApi InstaApi;

        public List<InstagramUser> i_dont_follow_back = new List<InstagramUser>();

        public YouDontFollowPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            listView.ItemsSource = i_dont_follow_back;
        }
    }
}