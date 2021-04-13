namespace XamTemp.ViewModels
{
    using MvvmHelpers;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Xamarin.Essentials;
    using Xamarin.Forms;

    class AboutViewModel : BaseViewModel
    {
        public AboutViewModel()
        {
            Title = "About";
            OpenUrlCommand = new Command<string>(async (url) => await ExecuteOpenUrl(url));
            SendEmailButton = new Command(async () => await ExecuteSendEmail());
        }

        public Command OpenUrlCommand { get; set; }
        
        public Command SendEmailButton { get; set; }
        
        private async Task ExecuteOpenUrl(string url)
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                await Launcher.TryOpenAsync(url);
            }
            catch (UriFormatException)
            {
                Debug.WriteLine("Uri is malformed.");
            }
            catch (FeatureNotSupportedException)
            {
                await Device.InvokeOnMainThreadAsync(async () => await Application.Current.MainPage.DisplayAlert("Unsupported feature", "Your device dosen't support https urls", "Bad..."));
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ExecuteSendEmail()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var message = new EmailMessage
                {
                    Subject = "Feedback for XamTemp",
                    To = new List<string>() { "daniele.tentoni.1996@gmail.com" }
                };
                await Email.ComposeAsync(message);
            }
            catch (FeatureNotSupportedException)
            {
                await Device.InvokeOnMainThreadAsync(async () => await Application.Current.MainPage.DisplayAlert("Unsupported feature", "Your device doesn't support email sending", "Bad..."));
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
