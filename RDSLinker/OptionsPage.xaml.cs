using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Pour plus d'informations sur le modèle d'élément Page vierge, consultez la page https://go.microsoft.com/fwlink/?LinkId=234238

namespace RDSLinker
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class OptionsPage : Page
    {
        MainPage parentPointer;
        string _token;
        public OptionsPage()
        {
            this.InitializeComponent();
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            // Read data from a simple setting.
            Object ip = localSettings.Values["ip"];
            Object port = localSettings.Values["port"];
            Object file = localSettings.Values["file"];
            _token = (string)localSettings.Values["filetoken"];

            if (ip != null)
            {
                ipfield.Text = (string)ip;
            }
            if (port != null)
            {
                portfield.Text = (string)port;
            }
            if (file != null)
            {
                sendfilefield.Text = (string)file;
            }
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            parentPointer = e.Parameter as MainPage;
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            // Create a simple setting.
            localSettings.Values["ip"] = ipfield.Text;
            localSettings.Values["port"] = portfield.Text;
            localSettings.Values["file"] = sendfilefield.Text;
            localSettings.Values["filetoken"] = _token;

            parentPointer.checkParameters();

        }
        private async void SelectFile_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            openPicker.FileTypeFilter.Add("*");
            StorageFile file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

                sendfilefield.Text = file.Path;
                _token = Guid.NewGuid().ToString();
                StorageApplicationPermissions.FutureAccessList.AddOrReplace(_token, file);

            }
        }
    }
}
