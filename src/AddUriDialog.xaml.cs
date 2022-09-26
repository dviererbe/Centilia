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

// Die Elementvorlage "Inhaltsdialogfeld" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace Centilia
{
    public sealed partial class AddUriDialog : ContentDialog
    {
        public enum DialogResult
        {
            OK,
            Cancel
        }

        public AddUriDialog()
        {
            this.InitializeComponent();
        }

        public DialogResult Result
        {
            get;
            private set;
        }

        public string RawURI
        {
            get;
            private set;
        }

        public Uri URI
        {
            get;
            private set;
        }

        private void txtURI_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtURI.Text))
            {
                RawURI = string.Empty;
                URI = null;
                txtWarning.Visibility = Visibility.Visible;
                this.IsPrimaryButtonEnabled = false;

                return;
            }

            RawURI = txtURI.Text?.Trim()?.ToLower();
            
            if (Uri.TryCreate(RawURI, UriKind.RelativeOrAbsolute, out Uri uri))
            {
                URI = uri;
                txtWarning.Visibility = Visibility.Collapsed;
                this.IsPrimaryButtonEnabled = true;
            }
            else
            {
                URI = null;
                txtWarning.Visibility = Visibility.Visible;
                this.IsPrimaryButtonEnabled = false;
            }

            
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Result = DialogResult.OK;

            //check if something went wrong
            if(URI == null || string.IsNullOrWhiteSpace(txtURI.Text))
            {
                txtWarning.Visibility = Visibility.Visible;
                this.IsPrimaryButtonEnabled = false;

                return;
            }

            this.Hide();
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Result = DialogResult.Cancel;

            this.Hide();   
        }
    }
}
