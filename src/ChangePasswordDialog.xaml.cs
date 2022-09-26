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
    public sealed partial class ChangePasswordDialog : ContentDialog
    {
        private static string MasterPassword;

        public enum DialogResult
        {
            OK,
            OldPasswordWrong,
            DifferentNewPasswords,
            Cancel
        }
        
        public ChangePasswordDialog(string masterPassword)
        {
            this.InitializeComponent();
            MasterPassword = masterPassword;
            NewPassword = null;
        }

        public DialogResult Result
        {
            get;
            private set;
        }

        public string NewPassword
        {
            get;
            private set;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if(App.HashString(pwboxOldPassword.Password).Equals(MasterPassword))
            {
                if(pwboxNewPassword1.Password.Equals(pwboxNewPassword2.Password))
                {
                    NewPassword = App.HashString(pwboxNewPassword1.Password);
                    Result = DialogResult.OK;
                }
                else
                {
                    Result = DialogResult.DifferentNewPasswords;
                }
            }
            else
            {
                Result = DialogResult.OldPasswordWrong;
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
