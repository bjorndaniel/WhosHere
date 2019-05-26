using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Microsoft.Extensions.Options;
using MvvmHelpers;
using WhosHere.Common;

namespace WhosHere.Wpf
{
    public class MainViewModel : BaseViewModel
    {
        private ConfigValues _secrets;
        private ObservableCollection<WHUser> _avatars = new ObservableCollection<WHUser>();
        private bool _visible;
        private string _statusText = "No token aquired";
        private string _loginUrl;
        private string _selectedImagename;
        private ImageSource _selectedImage;
        private byte[] _selectedImageBytes;
        private string _imageStatus;
        private ObservableCollection<Person> _identifiedPersons;

        public MainViewModel(IOptions<ConfigValues> secrets)
        {
            _secrets = secrets.Value;
            UploadCommand = new Command(async (e) => await LoadAvatars());
            AnalyzeCommand = new Command(async (e) => await AnalyzeImage());
            TrainCommand = new Command(async (e) => await TrainModel());
        }

        public ICommand UploadCommand { get; set; }

        public ICommand AnalyzeCommand { get; set; }

        public ICommand TrainCommand { get; set; }

        public ObservableCollection<WHUser> Avatars
        {
            get => _avatars;
            set => SetProperty(ref _avatars, value);
        }

        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        public bool Visible
        {
            get => _visible;
            set => SetProperty(ref _visible, value);
        }

        public string LoginUrl
        {
            get => _loginUrl;
            set => SetProperty(ref _loginUrl, value);
        }


        public ImageSource SelectedImage
        {
            get => _selectedImage;
            set => SetProperty(ref _selectedImage, value);
        }

        public string SelectedImagename
        {
            get => _selectedImagename;
            set => SetProperty(ref _selectedImagename, value);
        }


        public byte[] SelectedImageBytes
        {
            get => _selectedImageBytes;
            set => SetProperty(ref _selectedImageBytes, value);
        }

        public string ImageStatus
        {
            get => _imageStatus;
            set => SetProperty(ref _imageStatus, value);
        }


        public ObservableCollection<Person> IdentifiedPersons
        {
            get => _identifiedPersons;
            set => SetProperty(ref _identifiedPersons, value);
        }

        private async Task LoadAvatars()
        {

            var result = await GraphConnector.AquireTokenAsync(_secrets, (loginResult) =>
            {
                Visible = true;
                LoginUrl = loginResult.VerificationUrl;
                StatusText = $"{loginResult.Message} The code is in the clipboard";
                var thread = new Thread(() => Clipboard.SetText(loginResult.UserCode));
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join();
                return Task.FromResult(true);
            });
            if (!string.IsNullOrEmpty(result))
            {
                StatusText = "Authenticated. Starting photo sync";
                foreach (var users in await GraphConnector.GetUsersAsync(result))
                {
                    var foundUsers = users.CurrentPage.Where(_ => !string.IsNullOrWhiteSpace(_.GivenName) && !string.IsNullOrWhiteSpace(_.Surname)).ToList();
                    foreach (var u in foundUsers)
                    {
                        var image = await GraphConnector.GetUserImageAsync(u.Id, result);
                        if (image != null)
                        {
                            var whUser = new WHUser { Mail = u.Mail, Image = image };
                            Avatars.Add(whUser);
                            StatusText = $"Total {Avatars.Count}";
                            await StorageConnector.AddUserToStorageAsync(whUser, _secrets);
                            await FaceConnector.AddUserToFaceApiAsync(whUser, _secrets);
                        }
                    }
                }
                StatusText = "Finished loading photos";
            }
        }

        private async Task AnalyzeImage()
        {
            ImageStatus = "Analyzing image";
            IdentifiedPersons = new ObservableCollection<Person>();
            IdentifiedPersons = new ObservableCollection<Person>(await FaceConnector.AnalyzeImageAsync(SelectedImageBytes, _secrets));
            ImageStatus = "Done analyzing...";
        }

        private async Task TrainModel()
        {
            StatusText = "Training model....";
            await FaceConnector.TrainModelAsync(_secrets);
            StatusText = "Finished training model";
        }
    }
}


