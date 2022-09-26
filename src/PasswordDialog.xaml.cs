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
    public sealed partial class PasswordDialog : ContentDialog
    {
        public enum DialogResult
        {
            OK,
            Cancel
        }

        public PasswordDialog()
        {
            this.InitializeComponent();
        }

        public string Password
        {
            get;
            private set;
        }

        public DialogResult Result
        {
            get;
            private set;
        }
        
        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.Result = DialogResult.OK;
            this.Password = pwBox.Password;
            this.Hide();
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.Result = DialogResult.Cancel;
            this.Password = pwBox.Password;
            this.Hide();
        }
    }
}
