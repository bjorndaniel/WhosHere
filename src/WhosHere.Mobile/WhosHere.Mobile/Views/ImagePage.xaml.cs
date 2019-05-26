using WhosHere.Mobile.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WhosHere.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ImagePage : ContentPage
    {
        private FaceViewModel _vm;
        public ImagePage()
        {
            InitializeComponent();
            BindingContext = _vm = new FaceViewModel();
            MessagingCenter.Subscribe<FaceViewModel>(this, "", async (e) =>
            {
                await DisplayAlert("Analyzing image", "Your image is being analyzed", "Ok");
            });
        }
    }
}