using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Pour plus d'informations sur le modèle d'élément Page vierge, consultez la page https://go.microsoft.com/fwlink/?LinkId=234238

namespace RDSLinker
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class GlobalPage : Page
    {
        MainPage parentPointer;
        public GlobalPage()
        {
            this.InitializeComponent();


        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            parentPointer = e.Parameter as MainPage;

            Paragraph paragraph = new Paragraph();
            Run run = new Run();

            run.Text = parentPointer.getLogs();

            paragraph.Inlines.Add(run);
            console.Blocks.Add(paragraph);

            launchbutton.IsEnabled = parentPointer.isConfigurationComplete();
        }
        private async void LaunchButton_Click(object sender, RoutedEventArgs e)
        {
            parentPointer.launchWatchLoop(this);
        }

        public void addParagraph(string text)
        {
            Paragraph paragraph = new Paragraph();
            Run run = new Run();

            run.Text = text;

            paragraph.Inlines.Add(run);
            console.Blocks.Add(paragraph);
            rtcontainer.ChangeView(0, double.MaxValue, 1);
        }
    }
}
