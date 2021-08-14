using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Diagnostics;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Pour plus d'informations sur le modèle d'élément Page vierge, consultez la page https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace RDSLinker
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        String consoleLogs;
        bool _configComplete;
        public MainPage()
        {
            this.InitializeComponent();
            ApplicationView.PreferredLaunchViewSize = new Size(300, 500);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;

            consoleLogs = "";

            checkParameters();
        }
       
        public void checkParameters()
        {
            _configComplete = false;

            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;


            // Read data from a simple setting.
            String ip = (String)localSettings.Values["ip"];
            String port = (String)localSettings.Values["port"];
            String file = (String)localSettings.Values["file"];
            String filetoken = (String)localSettings.Values["filetoken"];

            if (file == null || ip == null || port == null || filetoken == null
                || file == "" || ip == "" || port == "")
            {
                consoleLog("Veuillez configurer le serveur RDS");
                return;
            }



            if (!StorageApplicationPermissions.FutureAccessList.ContainsItem(filetoken))
            {
                consoleLog("Impossible d'acceder au fichier, réessayez dans les parametres");
                return;
            }
            consoleLog("Prêt, cliquez sur \"Lancer\" pour lancer l'application");

            _configComplete = true;
        }

        public bool isConfigurationComplete()
        {
            return _configComplete;
        }

        private void NavigationView_Loaded(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(typeof(GlobalPage), this);
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            NavigationViewItem item = args.SelectedItem as NavigationViewItem;

            switch(item.Tag.ToString())
            {
                case "Home":
                    ContentFrame.Navigate(typeof(GlobalPage), this);
                    break;
                case "Options":
                    ContentFrame.Navigate(typeof(OptionsPage), this);
                    break;
            }
        }
        public String getLogs()
        {
            return consoleLogs;
        }
        public async void launchWatchLoop(GlobalPage childPointer)
        {
            String prevText = "";
            while(true)
            {
                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

                String filetoken = (String)localSettings.Values["filetoken"];
                StorageFile titlesFile = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(filetoken);

                try
                {
                    // Thaks a LOT, UWP developers 
                    String text = await Windows.Storage.FileIO.ReadTextAsync(titlesFile);
                    if (text != prevText)
                    {
                        consoleLog("Le fichier à été modifié");
                        childPointer.addParagraph("Le fichier à été modifié");

                        SendFileData(childPointer);
                        prevText = text;
                    }

                }
                catch (Exception)
                {
                    consoleLog("Le fichier que vous avez selectionné contient des caractères étranges");
                    childPointer.addParagraph("Le fichier que vous avez selectionné contient des caractères étranges");
                }
                await Task.Delay(10000);
            }
        }
        private async void SendFileData(GlobalPage childPointer)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            try
            {

                String filetoken = (String)localSettings.Values["filetoken"];

                StorageFile titlesFile = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(filetoken);

                string text = await Windows.Storage.FileIO.ReadTextAsync(titlesFile);

                text = text.Replace("\r\n", "");

                string textNews = "NEWS=" + text;
                string textSport = "SPORT=\u00A0";

                if (text.Length > 30)
                {
                    int unicodecut = 30;
                    textNews = "NEWS=" + text.Substring(0, unicodecut);
                    int textNewsByteLength = Encoding.UTF8.GetByteCount(textNews);

                    Debug.WriteLine(textNewsByteLength);
                    Debug.WriteLine(textNews.Length);

                    while (textNewsByteLength > 35)
                    {
                        unicodecut--;
                        textNews = "NEWS=" + text.Substring(0, unicodecut);
                        textNewsByteLength = Encoding.UTF8.GetByteCount(textNews);
                    }

                    textSport = "SPORT=" + text.Substring(unicodecut);
                }
                if (textSport.Length > 35)
                {
                    int unicodecut = 35;
                    textSport = textSport.Substring(0, unicodecut);
                    int textSportByteLength = Encoding.UTF8.GetByteCount(textSport);

                    while (textSportByteLength > 35)
                    {
                        unicodecut--;
                        textSport = textSport.Substring(0, unicodecut);
                        textSportByteLength = Encoding.UTF8.GetByteCount(textSport);
                    }
                }


                consoleLog("Envoi des données :");
                consoleLog(textNews + "\r\n" + textSport);
                childPointer.addParagraph("Envoi des données :");
                childPointer.addParagraph(textNews + "\r\n" + textSport);


                sendData(childPointer, textNews);
                sendData(childPointer, textSport);
            }
            catch (Exception)
            {
                consoleLog("Le fichier que vous avez selectionné contient des caractères étranges");
                childPointer.addParagraph("Le fichier que vous avez selectionné contient des caractères étranges");
                sendData(childPointer, "NEWS=-");
                sendData(childPointer, "SPORT=-");
            }
        }
        private async void sendData(GlobalPage childPointer, string data)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            // Read data from a simple setting.
            String ip = (String)localSettings.Values["ip"];
            String port = (String)localSettings.Values["port"];
            String file = (String)localSettings.Values["file"];
            try
            {
                // Create the StreamSocket and establish a connection to the echo server.
                using (var streamSocket = new Windows.Networking.Sockets.StreamSocket())
                {
                    // The server hostname that we will be establishing a connection to. In this example, the server and client are in the same process.
                    var hostName = new Windows.Networking.HostName(ip);

                    consoleLog("Trying to connect to TCP Server...");
                    childPointer.addParagraph("Trying to connect to TCP Server...");


                    await streamSocket.ConnectAsync(hostName, port);

                    consoleLog("Connected to " + ip + ":" + port + ".");
                    childPointer.addParagraph("Connected to " + ip + ":" + port + ".");


                    using (Stream outputStream = streamSocket.OutputStream.AsStreamForWrite())
                    {
                        using (var streamWriter = new StreamWriter(outputStream))
                        {
                            await streamWriter.WriteLineAsync(data);
                            await streamWriter.FlushAsync();
                        }
                    }

                    // Read data from the echo server.
                    string response;
                    using (Stream inputStream = streamSocket.InputStream.AsStreamForRead())
                    {
                        using (StreamReader streamReader = new StreamReader(inputStream))
                        {
                            response = await streamReader.ReadLineAsync();
                        }
                    }

                    consoleLog(string.Format("client received the response: \"{0}\" ", response));
                    childPointer.addParagraph(string.Format("client received the response: \"{0}\" ", response));
                }

                consoleLog("ClosingSocket");
                childPointer.addParagraph("ClosingSocket");
            }
            catch (Exception ex)
            {
                Windows.Networking.Sockets.SocketErrorStatus webErrorStatus = Windows.Networking.Sockets.SocketError.GetStatus(ex.GetBaseException().HResult);
                consoleLog(webErrorStatus.ToString() != "Unknown" ? webErrorStatus.ToString() : ex.Message);
                childPointer.addParagraph(webErrorStatus.ToString() != "Unknown" ? webErrorStatus.ToString() : ex.Message);
            }
        }
        private void consoleLog(string text)
        {

            consoleLogs = consoleLogs + "\r\n" + text;

        }
    }
}
