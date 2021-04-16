namespace XamTemp.ViewModels
{
    using MvvmHelpers;
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Xamarin.Forms;
    using XamTemp.Resources.Strings;
    using XamTemp.Services;

    class BaseReportViewModel : BaseViewModel
    {
        protected readonly ReportService service;
        public BaseReportViewModel()
        {
            service = DependencyService.Get<ReportService>();
            ResetDataCommand = new Command(async () => await ExecuteResetData().ConfigureAwait(false));
        }
        public Command ResetDataCommand { get; set; }

        private async Task ExecuteResetData()
        {
            if (IsBusy) { return; }
            IsBusy = true;
            try
            {
                var response = await Device.InvokeOnMainThreadAsync(
                    async () => await Application.Current.MainPage.DisplayAlert(
                        AppResources.Reset,
                        AppResources.ResetConfirmationMessage,
                        AppResources.ResetYes,
                        AppResources.ResetNo));
                if (response)
                {
                    await service.ResetData();
                    // Notify the application of the data reset.
                    MessagingCenter.Send(this, App.DataReset);
                }
            }
            catch (Exception e)
            {
                var message = string.Format(AppResources.ExceptionMessageReportReset, e.Message);
                Debug.WriteLine(message);
                await Device.InvokeOnMainThreadAsync(
                    async () => await Application.Current.MainPage.DisplayAlert(
                        AppResources.Error,
                        message,
                        AppResources.Bad));
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
