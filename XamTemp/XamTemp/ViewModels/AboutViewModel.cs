namespace XamTemp.ViewModels
{
    using MvvmHelpers;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Xamarin.CommunityToolkit.Helpers;
    using Xamarin.Essentials;
    using Xamarin.Forms;
    using XamTemp.Resources.Strings;

    class AboutViewModel : BaseViewModel
    {
        public LocalizedString AppVersion { get; } = new LocalizedString(
            () => string.Format(AppResources.Version, AppInfo.VersionString));

        public LocalizedString AboutIntro { get; } = new LocalizedString(() => AppResources.AboutIntro);

        public LocalizedString AboutBody { get; } = new LocalizedString(() => AppResources.AboutBody);

        public LocalizedString MadeWith { get; } = new LocalizedString(() => AppResources.MadeWith);

        public LocalizedString SendFeedback { get; } = new LocalizedString(() => AppResources.SendFeedback);

        public AboutViewModel()
        {
            Title = "About";
            OpenUrlCommand = new Command<string>(async (url) => await ExecuteOpenUrl(url).ConfigureAwait(false));
            SendEmailButton = new Command(async () => await ExecuteSendEmail().ConfigureAwait(false));
        }

        public Command OpenUrlCommand { get; set; }

        public Command SendEmailButton { get; set; }

        private async Task ExecuteOpenUrl(string url)
        {
            if (IsBusy) { return; }
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
            if (IsBusy) { return; }
            IsBusy = true;
            try
            {
                var message = new EmailMessage
                {
                    Subject = AppResources.FeedbackFor,
                    To = new List<string> { "daniele.tentoni.1996@gmail.com" }
                };
                await Email.ComposeAsync(message);
            }
            catch (FeatureNotSupportedException)
            {
                await Device.InvokeOnMainThreadAsync(async () => await Application.Current.MainPage.DisplayAlert(AppResources.UnsupportedFeature, AppResources.UnsupportedEmail, AppResources.Bad));
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
