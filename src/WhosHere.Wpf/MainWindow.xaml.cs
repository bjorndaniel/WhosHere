using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using WhosHere.Common;

namespace WhosHere.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ConfigValues _secrets;
        private MainViewModel _viewModel;
        public MainWindow(IOptions<ConfigValues> secrets)
        {
            InitializeComponent();
            DataContext = _viewModel = new MainViewModel(secrets);
            _secrets = secrets.Value;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _viewModel.SelectedImagename = openFileDialog.FileName;
                _viewModel.SelectedImageBytes = File.ReadAllBytes(openFileDialog.FileName);
                _viewModel.SelectedImage = new BitmapImage(new Uri(openFileDialog.FileName));
            }
        }
    }
}