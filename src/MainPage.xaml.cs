using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x407 dokumentiert.

namespace Centilia
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const string InfoText = "Centilia Version 1.01.1031 Städtische Werke Magdeburg Edition\n© 2017 Dominik Viererbe";

        private Uri _homePage;

        private Settings _settings;
        private HashSet<string> _whitelist;
        private HashSet<string> _blacklist;

        public MainPage()
        {
            this.InitializeComponent();
        }
        
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ApplicationView view = ApplicationView.GetForCurrentView();
            bool isInFullScreenMode = view.IsFullScreenMode;

            if(!isInFullScreenMode)
                view.TryEnterFullScreenMode();
            
            dynamic parameter = e.Parameter;
            _settings = parameter.Settings;

            if(_settings == null)
            {
                _settings = new Settings()
                {
                    HideControlls = true,
                    Homepage = App.DefaultHomepage,
                    AllowHTTP = false,
                    IsWhitelistEnabled = false,
                    Whitelist = new string[0],
                    IsBlacklistEnabled = false,
                    Blacklist = new string[0],
                    MasterPassword = App.HashString("icrAYH2k16peluL")
                };
            }

            if(_settings.HideControlls)
            {
                Controls.Visibility = Visibility.Collapsed;
                columnControls.Width = new GridLength(0);
                txtAddressBar.Visibility = Visibility.Collapsed;
            }

            _whitelist = new HashSet<string>(_settings.Whitelist);
            _blacklist = new HashSet<string>(_settings.Blacklist);

            if (!Uri.TryCreate(_settings.Homepage, UriKind.RelativeOrAbsolute, out _homePage))
            {
                webFrame.NavigateToString("<html><body><h1>Fehler!</h1>\n<h2>Die Adresse der Startseite ist keine zulässige URI!</h2></body></html>");
            }
            else
            {
                webFrame.Navigate(_homePage);
            }

            webFrame.NavigationStarting += WebFrame_NavigationStarting;
            webFrame.NavigationCompleted += WebFrame_NavigationCompleted;
        }

        private void WebFrame_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            txtAddressBar.Text = args.Uri?.AbsoluteUri ?? "";
            
            button_GoBack.IsEnabled = webFrame.CanGoBack;
            button_GoForward.IsEnabled = webFrame.CanGoForward;
        }

        private async void WebFrame_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            if (args.Uri != null && args.Uri.Equals(_homePage))
                return;

            if (args.Uri != null && args.Uri.Scheme.Equals("http", StringComparison.OrdinalIgnoreCase) && !_settings.AllowHTTP)
            {
                args.Cancel = true;

                MessageDialog errorMessage = new MessageDialog($"Die Navigation zu \"{args.Uri.AbsoluteUri}\" wurde verhindert, da diese Seite kein HTTPS unterstützt.\nBitte wenden Sie sich an den Systemadministrator, wenn Sie Zugriff auf diese Seite benötigen.", "HINWEIS");
                await errorMessage.ShowAsync();

                return;
            }

            if(args.Uri != null && (_settings.IsBlacklistEnabled &&  _blacklist.Contains(args.Uri.Authority)))
            {
                args.Cancel = true;

                MessageDialog errorMessage = new MessageDialog($"Die Navigation zu \"{args.Uri.AbsoluteUri}\" wurde verhindert, da \"{args.Uri.Authority}\" auf der Blacklist indiziert ist.\nBitte wenden Sie sich an den Systemadministrator, wenn Sie Zugriff auf diese Seite benötigen.", "HINWEIS");
                await errorMessage.ShowAsync();

                return;
            }

            if (args.Uri != null && (_settings.IsWhitelistEnabled && !_whitelist.Contains(args.Uri.Authority)))
            {
                args.Cancel = true;

                MessageDialog errorMessage = new MessageDialog($"Die Navigation zu \"{args.Uri.AbsoluteUri}\" wurde verhindert, da \"{args.Uri.Authority}\" nicht auf der Whitelist indiziert ist.\nBitte wenden Sie sich an den Systemadministrator, wenn Sie Zugriff auf diese Seite benötigen.", "HINWEIS");
                await errorMessage.ShowAsync();

                return;
            }

            //TODO: Log this stuff...

            button_GoBack.IsEnabled = webFrame.CanGoBack;
            button_GoForward.IsEnabled = webFrame.CanGoForward;
        }

        private async void Button_Info_Clicked(object sender, RoutedEventArgs e)
        {
            MessageDialog dialog = new MessageDialog(InfoText, "Info");
            await dialog.ShowAsync();
        }

        private async void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            PasswordDialog dialog = new PasswordDialog();
            await dialog.ShowAsync();

            if(dialog.Result == PasswordDialog.DialogResult.OK)
            {
                if(App.HashString(dialog.Password).Equals(_settings.MasterPassword))
                {
                    Frame.Navigate(typeof(SettingsPage), new { Settings = _settings });
                }
                else
                {
                    MessageDialog errorMessage = new MessageDialog("Das Kennwort ist falsch! Stellen sie sicher, dass die Feststelltaste nicht aktiviert ist oder ob der Nummernblock auf der Tastatur deaktiviert ist, falls Sie diesen benutzt haben sollten.", "Fehler!");
                    await errorMessage.ShowAsync();
                }
            }
        }

        private void Button_Back_Click(object sender, RoutedEventArgs e)
        {
            if(webFrame.CanGoBack)
                webFrame.GoBack();

            button_GoBack.IsEnabled = webFrame.CanGoBack;
            button_GoForward.IsEnabled = webFrame.CanGoForward;
        }

        private void Button_Forward_Click(object sender, RoutedEventArgs e)
        {
            if (webFrame.CanGoForward)
                webFrame.GoForward();
            
            button_GoBack.IsEnabled = webFrame.CanGoBack;
            button_GoForward.IsEnabled = webFrame.CanGoForward;
        }

        private void Button_Refresh_Click(object sender, RoutedEventArgs e)
        {
            webFrame.Refresh();
        }

        private void Button_Home_Click(object sender, RoutedEventArgs e)
        {
            //Die Adresse der Startseite konnte nicht in eine uri konvertiert werden!
            if(_homePage == null)
                webFrame.NavigateToString("<html><body><h1>Fehler!</h1>\n<h2>Die Adresse der Startseite ist keine zulässige URI!</h2></body></html>");
            else
                webFrame.Navigate(_homePage);
        }
    }
}
