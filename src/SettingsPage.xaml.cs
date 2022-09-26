using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace Centilia
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        private Settings _settings;
        
        public SettingsPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            dynamic parameter = e.Parameter;
            _settings = parameter.Settings;

            txtHomepage.Text = _settings.Homepage;
            switchHideControls.IsOn = _settings.HideControlls;
            switchAllowHttp.IsOn = _settings.AllowHTTP;
            switchUseWhitelist.IsOn = _settings.IsWhitelistEnabled;
            switchUseBlacklist.IsOn = _settings.IsBlacklistEnabled;

            foreach(string entry in _settings.Whitelist)
            {
                listWhitelist.Items.Add(entry);
            }

            foreach (string entry in _settings.Blacklist)
            {
                listBlacklist.Items.Add(entry);
            }
        }
        
        private void txtHomepage_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool isValidUri(string uri)
            {
                Uri result;
                return Uri.TryCreate(uri, UriKind.RelativeOrAbsolute, out result);
            }

            string input = txtHomepage.Text.Trim().ToLower();

            txtHompageWarning.Visibility = (isValidUri(input)) ? Visibility.Collapsed : Visibility.Visible; 
        }

        private async void Button_ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            ChangePasswordDialog dialog = new ChangePasswordDialog(_settings.MasterPassword);
            await dialog.ShowAsync();

            if (dialog.Result == ChangePasswordDialog.DialogResult.OK)
            {
                try
                {
                    ApplicationData.Current.LocalSettings.Values["MasterPassword"] = dialog.NewPassword;
                    _settings.MasterPassword = dialog.NewPassword;
                }
                catch (Exception ex)
                {
                    MessageDialog errorMessage = new MessageDialog("Beim speichern des neuen Passworts auf der Festplatte ist ein Fehler aufgetreten!", "Fehler!");
                    await errorMessage.ShowAsync();
                }
            }
            else if(dialog.Result == ChangePasswordDialog.DialogResult.OldPasswordWrong)
            {
                MessageDialog errorMessage = new MessageDialog("Das Kennwort ist falsch! Stellen sie sicher, dass die Feststelltaste nicht aktiviert ist oder ob der Nummernblock auf der Tastatur deaktiviert ist, falls Sie diesen benutzt haben sollten.", "Fehler!");
                await errorMessage.ShowAsync();
            }
            else if (dialog.Result == ChangePasswordDialog.DialogResult.DifferentNewPasswords)
            {
                MessageDialog errorMessage = new MessageDialog("Das neue Kennwort stimmt nicht mit der wiederholten Eingabe überein!", "Fehler!");
                await errorMessage.ShowAsync();
            }
        }

        private async void Button_Save_Click(object sender, RoutedEventArgs e)
        {
            object[] whitelist = new string[listWhitelist.Items.Count];
            listWhitelist.Items.CopyTo(whitelist, 0);

            object[] blacklist = new string[listBlacklist.Items.Count];
            listBlacklist.Items.CopyTo(blacklist, 0);

            Settings settings = new Settings()
            {
                HideControlls = switchHideControls.IsOn,
                Homepage = txtHomepage.Text.Trim().ToLower(),
                AllowHTTP = switchAllowHttp.IsOn,
                IsWhitelistEnabled = switchUseWhitelist.IsOn,
                IsBlacklistEnabled = switchUseBlacklist.IsOn,
                MasterPassword = _settings.MasterPassword,
                Whitelist = whitelist as string[],
                Blacklist = blacklist as string[]
            };

            try
            {
                var localSettings = ApplicationData.Current.LocalSettings;

                localSettings.Values["Homepage"] = settings.Homepage;
                localSettings.Values["AllowHTTP"] = settings.AllowHTTP;
                localSettings.Values["IsBlacklistEnabled"] = settings.IsBlacklistEnabled;
                localSettings.Values["IsWhitelistEnabled"] = settings.IsWhitelistEnabled;

                string setting = string.Empty;

                foreach(string entry in settings.Blacklist)
                {
                    setting += entry + ';';
                }

                if (setting.Length > 0)
                    setting = setting.Substring(0, setting.Length - 1);

                localSettings.Values["Whitelist"] = setting;

                setting = string.Empty;

                foreach (string entry in settings.Blacklist)
                {
                    setting += entry + ';';
                }

                if(setting.Length > 0)
                    setting = setting.Substring(0, setting.Length - 1);

                localSettings.Values["Blacklist"] = setting;

            }
            catch (Exception ex)
            {
                MessageDialog errorMessage = new MessageDialog("Die neuen Einstellungen konnten nicht gespeichert werden!", "Fehler!");
                await errorMessage.ShowAsync();

                await App.LogAsync("Unable to save new Settings!", ex);

                return;
            }
            
            Frame.Navigate(typeof(MainPage), new { Arguments = null as string, Settings = settings });
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage), new { Arguments = null as string, Settings = _settings });
        }

        private void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(pivot.SelectedIndex == 0)
            {
                Separator.Visibility = Visibility.Collapsed;
                Button_Add.Visibility = Visibility.Collapsed;
                Button_Remove.Visibility = Visibility.Collapsed;
            }
            else
            {
                Separator.Visibility = Visibility.Visible;
                Button_Add.Visibility = Visibility.Visible;
                Button_Remove.Visibility = Visibility.Visible;
            }
        }

        private async void Button_Add_Click(object sender, RoutedEventArgs e)
        {
            if(pivot.SelectedIndex == 0)
            {
                Separator.Visibility = Visibility.Collapsed;
                Button_Add.Visibility = Visibility.Collapsed;
                Button_Remove.Visibility = Visibility.Collapsed;
            }
            else
            {
                AddUriDialog dialog = new AddUriDialog();
                await dialog.ShowAsync();

                if(dialog.Result == AddUriDialog.DialogResult.OK)
                {
                    if(pivot.SelectedIndex == 1) //Whitelist
                    {
                        if (listWhitelist.Items.Contains(dialog.RawURI))
                        {
                            MessageDialog errorMessage = new MessageDialog("Die Whitelist enthält bereits einen Eintrag mit diesem Wert!", "Fehler!");
                            await errorMessage.ShowAsync();
                        }
                        else
                        {
                            listWhitelist.Items.Add(dialog.RawURI);
                        }
                    }
                    else if(pivot.SelectedIndex == 2) //Blacklist
                    {
                        if(listBlacklist.Items.Contains(dialog.RawURI))
                        {
                            MessageDialog errorMessage = new MessageDialog("Die Blacklist enthält bereits einen Eintrag mit diesem Wert!", "Fehler!");
                            await errorMessage.ShowAsync();
                        }
                        else
                        {
                            listBlacklist.Items.Add(dialog.RawURI);
                        }   
                    }
                }
            }
        }

        private async void Button_Remove_Click(object sender, RoutedEventArgs e)
        {
            if (pivot.SelectedIndex == 0)
            {
                Separator.Visibility = Visibility.Collapsed;
                Button_Add.Visibility = Visibility.Collapsed;
                Button_Remove.Visibility = Visibility.Collapsed;
            }    
            else if (pivot.SelectedIndex == 1) //Whitelist
            {
                if(listWhitelist.SelectedItem == null)
                {
                    MessageDialog errorMessage = new MessageDialog("Es ist kein Eintrag in der Whitelist ausgewält, der entfernt werden kann!", "Fehler!");
                    await errorMessage.ShowAsync();
                }
                else
                {
                    listWhitelist.Items.Remove(listWhitelist.SelectedItem);
                }
            }
            else if (pivot.SelectedIndex == 2) //Blacklist
            {
                if (listBlacklist.SelectedItem == null)
                {
                    MessageDialog errorMessage = new MessageDialog("Es ist kein Eintrag in der Blacklist ausgewält, der entfernt werden kann!", "Fehler!");
                    await errorMessage.ShowAsync();
                }
                else
                {
                    listBlacklist.Items.Remove(listBlacklist.SelectedItem);
                }
            }   
        }
    }
}
