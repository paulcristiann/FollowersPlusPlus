using System.ComponentModel;
using Xamarin.Forms.Internals;

namespace FollowersPlusPlus.Models
{
    [Preserve(AllMembers = true)]
    public class AppFeature : INotifyPropertyChanged
    {
        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Property

        public string Title { get; set; }

        public string Preview { get; set; }

        public string ImagePath { get; set; }

        #endregion

        #region Methods

        protected void OnPropertyChanged(string property)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        #endregion
    }
}