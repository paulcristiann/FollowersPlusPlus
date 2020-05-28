using FollowersPlusPlus.Models;
using System.Collections.ObjectModel;
using Xamarin.Forms.Internals;

namespace FollowersPlusPlus.ViewModels.ToDelete
{
    [Preserve(AllMembers = true)]
    public class ProfileViewModel : BaseViewModel
    {
        #region Fields

        private ObservableCollection<AppFeature> cardItems;

        #endregion

        #region Constructor

        public ProfileViewModel()
        {
            cardItems = new ObservableCollection<AppFeature>()
            {
                new AppFeature()
                {
                    Title = "Users not following you back",
                    Preview = "0",
                    ImagePath = ""
                },
                new AppFeature()
                {
                    Title = "Users you don't follow back",
                    Preview = "0",
                    ImagePath = ""
                },
                //new AppFeature()
                //{
                //    Title = "Feature #3",
                //    Preview = "0",
                //    ImagePath = "men.svg"
                //},
                //new AppFeature()
                //{
                //    Title = "Feature #4",
                //    Preview = "0",
                //    ImagePath = "men.svg"
                //}
            };
        }

        #endregion

        #region Properties

        public ObservableCollection<AppFeature> CardItems
        {
            get
            {
                return this.cardItems;
            }

            set
            {
                this.cardItems = value;
                this.NotifyPropertyChanged();
            }
        }
        #endregion
    }
}