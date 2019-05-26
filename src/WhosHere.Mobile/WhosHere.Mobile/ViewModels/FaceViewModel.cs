using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace WhosHere.Mobile.ViewModels
{
    public class FaceViewModel : BaseViewModel
    {

        private ImageSource _imageSource;

        public FaceViewModel()
        {
            TakePhotoCommand = new Command(async () => await TakePhoto());
        }


        public ICommand TakePhotoCommand { get; set; }

        public ImageSource CameraImage
        {
            get => _imageSource;
            set => SetProperty(ref _imageSource, value);
        }

        private async Task TakePhoto()
        {
            var photo = await Plugin.Media.CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions() { });
            if (photo != null)
            {
                var stream = photo.GetStream();
                var bytes = new byte[stream.Length];
                CameraImage = ImageSource.FromStream(() => new MemoryStream(bytes));
                stream.Read(bytes, 0, bytes.Length);
                MessagingCenter.Send(this, string.Empty);
                var client = new HttpClient();
                await client.PostAsync($"http://{App.FunctionUrl}/api/AnalyzeImage", new ByteArrayContent(bytes));
            }
        }
    }
}
