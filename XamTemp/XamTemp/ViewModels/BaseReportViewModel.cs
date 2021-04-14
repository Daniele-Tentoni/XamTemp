namespace XamTemp.ViewModels
{
    using MvvmHelpers;
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Xamarin.Forms;
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
                        "Reset",
                        "Are you sure to reset all report data and restore the initial setup of the applicazion?",
                        "Yes, reset data!",
                        "No, don't reset."));
                if (response)
                {
                    await service.ResetData();
                    // Notify the application of the data reset.
                    MessagingCenter.Send(this, App.DataReset);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Exception thrown resetting data: {e.Message}");
                await Device.InvokeOnMainThreadAsync(
                    async () => await Application.Current.MainPage.DisplayAlert(
                        "Error",
                        $"Error while resetting data: {e.Message}",
                        "Bad..."));
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
