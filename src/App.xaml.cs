using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Centilia
{
    /// <summary>
    /// Stellt das anwendungsspezifische Verhalten bereit, um die Standardanwendungsklasse zu ergänzen.
    /// </summary>
    sealed partial class App : Application
    {
        public const string DefaultHomepage = "https://www.sw-magdeburg.de";

        /// <summary>
        /// Initialisiert das Singletonanwendungsobjekt. Dies ist die erste Zeile von erstelltem Code
        /// und daher das logische Äquivalent von main() bzw. WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        public static string HashString(string value)
        {
            using (SHA512 algorithm = SHA512.Create())
            {
                byte[] buffer = Encoding.UTF32.GetBytes(value);
                buffer = algorithm.ComputeHash(buffer);

                StringBuilder hash = new StringBuilder();

                foreach(byte b in buffer)
                {
                    hash.Append(Convert.ToString(b, 16));
                }

                return hash.ToString();
            }
        }

        private static async Task<StorageFile> GetLogFileAsync()
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;

            try
            {
                return await localFolder.GetFileAsync("centilia.log");
            }
            catch (FileNotFoundException)
            {
#if DEBUG
                Debug.WriteLine("Log file not found. Creating new log file...");
#endif
                try
                {
                    return await localFolder.CreateFileAsync("centilia.log");
                }
#if DEBUG
                catch (UnauthorizedAccessException ex)
                {
                    Debug.WriteLine("Application is not authorized to create log file.", ex);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Creating new log file failed!", ex);
                }
#else
                catch { /* there is nothing what i can do :( */ }
#endif
            }
#if DEBUG
            catch (UnauthorizedAccessException ex)
            {
                Debug.WriteLine("Application is not authorized to access log file.", ex);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Accessing log file failed!", ex);
            }
#else
            catch { /* there is nothing what i can do :( */ }
#endif
            return null;
        }

        private static async Task<StorageFile> GetHistoryFileAsync()
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;

            try
            {
                return await localFolder.GetFileAsync("centilia.history");
            }
            catch (FileNotFoundException)
            {
#if DEBUG
                Debug.WriteLine("History file not found. Creating new history file...");
#endif
                try
                {
                    return await localFolder.CreateFileAsync("centilia.history");
                }
#if DEBUG
                catch (UnauthorizedAccessException ex)
                {
                    Debug.WriteLine("Application is not authorized to create history file.", ex);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Creating new history file failed!", ex);
                }
#else
                catch { /* there is nothing what i can do :( */ }
#endif
            }
#if DEBUG
            catch (UnauthorizedAccessException ex)
            {
                Debug.WriteLine("Application is not authorized to access history file.", ex);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Accessing history file failed!", ex);
            }
#else
            catch { /* there is nothing what i can do :( */ }
#endif
            return null;
        }

        public static async Task LogAsync(string message, Exception exception = null)
        {
            DateTime now = DateTime.Now;

            Task<StorageFile> fileTask = GetLogFileAsync();

#if DEBUG
            if (exception == null)
            {
                Debug.WriteLine(message);
            }
            else
            {
                Debug.WriteLine(message, exception);
            }
#endif
            string prefix = $"{now.Year}/{now.Month.ToString().PadLeft(2, '0')}/{now.Day.ToString().PadLeft(2, '0')} {now.Hour.ToString().PadLeft(2, '0')}:{now.Minute.ToString().PadLeft(2, '0')}:{now.Second.ToString().PadLeft(2, '0')}: ";
            string logMessage = prefix + message;

            if (exception != null)
            {
                //TODO: SERIALIZE EXCEPTION!
                string serializedException = "TODO: SERIALIZE EXCEPTION!!!";

                logMessage += Environment.NewLine + new string(' ', prefix.Length) + serializedException;
            }

            StorageFile logFile = await fileTask;

            if(logFile != null)
            {
                try
                {
                    await FileIO.AppendTextAsync(logFile, logMessage);
                }
#if DEBUG
                catch (Exception ex)
                {
                    Debug.WriteLine("Unable to append log message to log file!", ex);
                }
#else
                catch { /* there is nothing what i can do :( */ }
#endif
            }
        }

        public static async Task LogUrlAsync(string url)
        {
            DateTime now = DateTime.Now;

            StorageFile logFile = await GetHistoryFileAsync();
            
            if (logFile != null)
            {
                string prefix = $"{now.Year}/{now.Month.ToString().PadLeft(2, '0')}/{now.Day.ToString().PadLeft(2, '0')} {now.Hour.ToString().PadLeft(2, '0')}:{now.Minute.ToString().PadLeft(2, '0')}:{now.Second.ToString().PadLeft(2, '0')}: ";
                string logMessage = prefix + "Navigating to " + url;

                try
                {
                    await FileIO.AppendTextAsync(logFile, logMessage);
                }
#if DEBUG
                catch (Exception ex)
                {
                    Debug.WriteLine("Unable to append history log to history file!", ex);
                }
#else
                catch { /* there is nothing what i can do :( */ }
#endif
            }
        }

        private Settings LoadSettings()
        {
            try
            {
                var localSettings = ApplicationData.Current.LocalSettings;

                string localString;

                Settings result = new Settings();
                if (!localSettings.Values.ContainsKey("HideControlls"))
                    localSettings.Values["HideControlls"] = true;

                if (!localSettings.Values.ContainsKey("Homepage"))
                    localSettings.Values["Homepage"] = DefaultHomepage;

                if (!localSettings.Values.ContainsKey("AllowHTTP"))
                    localSettings.Values["AllowHTTP"] = false;

                if (!localSettings.Values.ContainsKey("IsBlacklistEnabled"))
                    localSettings.Values["IsBlacklistEnabled"] = false;

                if (!localSettings.Values.ContainsKey("IsWhitelistEnabled"))
                    localSettings.Values["IsWhitelistEnabled"] = false;

                if (!localSettings.Values.ContainsKey("Blacklist"))
                    localSettings.Values["Blacklist"] = "";

                if (!localSettings.Values.ContainsKey("Whitelist"))
                    localSettings.Values["Whitelist"] = "";

                if (!localSettings.Values.ContainsKey("MasterPassword"))
                    localSettings.Values["MasterPassword"] = HashString("icrAYH2k16peluL");

                result.HideControlls = (bool)localSettings.Values["HideControlls"];
                result.Homepage = localSettings.Values["Homepage"] as string;
                result.AllowHTTP = (bool)localSettings.Values["AllowHTTP"];
                result.IsBlacklistEnabled = (bool)localSettings.Values["IsBlacklistEnabled"];
                result.IsWhitelistEnabled = (bool)localSettings.Values["IsWhitelistEnabled"];

                localString = localSettings.Values["Blacklist"] as string;
                result.Blacklist = string.IsNullOrEmpty(localString) ? new string[0] : localString.Split(';');

                localString = localSettings.Values["Whitelist"] as string;
                result.Whitelist = string.IsNullOrEmpty(localString) ? new string[0] : localString.Split(';');

                result.MasterPassword = localSettings.Values["MasterPassword"] as string;

#if DEBUG
                Debug.WriteLine("Loaded Settings:");
                Debug.WriteLine("Homepage: " + result.Homepage);
                Debug.WriteLine("AllowHTTP: " + result.AllowHTTP);
                Debug.WriteLine("IsWhitelistEnabled: " + result.IsWhitelistEnabled);
                Debug.Write("Whitelist: ");
                Debug.WriteLine(result.Whitelist);
                Debug.WriteLine("IsBlacklistEnabled: " + result.IsBlacklistEnabled);
                Debug.Write("Blacklist: ");
                Debug.WriteLine(result.Blacklist);
                Debug.WriteLine("MasterPassword: " + result.MasterPassword);
#endif

                return result;
            }
            catch (Exception ex)
            {
                LogAsync("Failed to load Settings!", ex).Wait();

                return null;
            }
        }

        /// <summary>
        /// Wird aufgerufen, wenn die Anwendung durch den Endbenutzer normal gestartet wird. Weitere Einstiegspunkte
        /// werden z. B. verwendet, wenn die Anwendung gestartet wird, um eine bestimmte Datei zu öffnen.
        /// </summary>
        /// <param name="e">Details über Startanforderung und -prozess.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // App-Initialisierung nicht wiederholen, wenn das Fenster bereits Inhalte enthält.
            // Nur sicherstellen, dass das Fenster aktiv ist.
            if (rootFrame == null)
            {
                // Frame erstellen, der als Navigationskontext fungiert und zum Parameter der ersten Seite navigieren
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Zustand von zuvor angehaltener Anwendung laden
                }

                // Den Frame im aktuellen Fenster platzieren
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // Wenn der Navigationsstapel nicht wiederhergestellt wird, zur ersten Seite navigieren
                    // und die neue Seite konfigurieren, indem die erforderlichen Informationen als Navigationsparameter
                    // übergeben werden
                    rootFrame.Navigate(typeof(MainPage), new { Arguments = e.Arguments, Settings = LoadSettings() });
                }
                // Sicherstellen, dass das aktuelle Fenster aktiv ist
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Wird aufgerufen, wenn die Navigation auf eine bestimmte Seite fehlschlägt
        /// </summary>
        /// <param name="sender">Der Rahmen, bei dem die Navigation fehlgeschlagen ist</param>
        /// <param name="e">Details über den Navigationsfehler</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Wird aufgerufen, wenn die Ausführung der Anwendung angehalten wird.  Der Anwendungszustand wird gespeichert,
        /// ohne zu wissen, ob die Anwendung beendet oder fortgesetzt wird und die Speicherinhalte dabei
        /// unbeschädigt bleiben.
        /// </summary>
        /// <param name="sender">Die Quelle der Anhalteanforderung.</param>
        /// <param name="e">Details zur Anhalteanforderung.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Anwendungszustand speichern und alle Hintergrundaktivitäten beenden
            deferral.Complete();
        }
    }
}
