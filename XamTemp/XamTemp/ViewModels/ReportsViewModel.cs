namespace XamTemp.ViewModels
{
    using Realms.Exceptions;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Xamarin.Forms;
    using XamTemp.Models;
    using XamTemp.Resources.Strings;

    class ReportsViewModel : BaseReportViewModel
    {
        public ObservableCollection<ReportGroup> Reports { get; set; }

        public ReportsViewModel()
        {
            Title = "Reports";
            Reports = new ObservableCollection<ReportGroup>();
            LoadTemperaturesCommand = new Command(async () => await ExecuteLoadTemperature().ConfigureAwait(false));
            AddTemperatureCommand = new Command(async () => await ExecuteAddTemperature().ConfigureAwait(false));
            RemoveReportCommand = new Command<Report>(async (report) => await ExecuteDeleteTemperature(report).ConfigureAwait(false));
            SentDataCommand = new Command<Report>(async (report) => await ExecuteSentData(report).ConfigureAwait(false));

            MessagingCenter.Subscribe<BaseReportViewModel>(this, App.DataReset, sender =>
            {
                Reports.Clear();
            });
        }

        public Command LoadTemperaturesCommand { get; set; }
        public Command AddTemperatureCommand { get; set; }
        public Command RemoveReportCommand { get; set; }
        public Command SentDataCommand { get; set; }

        private async Task ExecuteLoadTemperature()
        {
            if (IsBusy) { return; }
            IsBusy = true;
            try
            {
                // Update reports.
                Reports.Clear();
                var reports = await service.GetReportsAsync();
                foreach (var elem in reports)
                {
                    AddReport(elem);
                }
                OnPropertyChanged(nameof(Reports));
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ExecuteAddTemperature()
        {
            if (IsBusy) { return; }
            IsBusy = true;
            try
            {
                var temperature = await Device.InvokeOnMainThreadAsync(async () =>
                await Application.Current.MainPage.DisplayPromptAsync(AppResources.Temperature, AppResources.InputTemperature, AppResources.Ok, AppResources.Cancel, AppResources.Temperature, keyboard: Keyboard.Numeric));
                var saturation = await Device.InvokeOnMainThreadAsync(async () =>
                await Application.Current.MainPage.DisplayPromptAsync(AppResources.Saturation, AppResources.InputSaturation, AppResources.Ok, AppResources.Cancel, AppResources.Saturation, keyboard: Keyboard.Numeric));
                var intSaturation = int.Parse(saturation);
                var doubleTemperature = double.Parse(temperature);
                var added = await service.AddReportAsync(new Report
                {
                    Saturation = intSaturation,
                    Sent = false,
                    Temperature = doubleTemperature
                });
                AddReport(added);
            }
            catch (Exception e)
            {
                var message = string.Format(AppResources.ExceptionMessageReportAdd, e.Message);
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

        /// <summary>
        /// Add a report to the reports collection. Remeber to run OnPropertyChanged after this or those executions.
        /// </summary>
        /// <param name="report">Report to add.</param>
        private void AddReport(Report report)
        {
            var reportGroup = Reports.SingleOrDefault(s => s.Date.Year.Equals(report.CreatedAt.Year) && s.Date.DayOfYear.Equals(report.CreatedAt.DayOfYear));
            if (reportGroup != null)
            {
                reportGroup.Add(report);
            }
            else
            {
                var newList = new List<Report>
                {
                    report
                };
                // Insert the item as first item, since it's the last added.
                Reports.Insert(0, new ReportGroup(report.CreatedAt, newList));
            }
        }

        private async Task ExecuteDeleteTemperature(Report report)
        {
            if (IsBusy) { return; }
            IsBusy = true;
            try
            {
                // Ask consensus.
                var response = await Device.InvokeOnMainThreadAsync(
                    async () => await Application.Current.MainPage.DisplayAlert(
                        AppResources.Delete,
                        AppResources.DeleteConfirm,
                        AppResources.Sure,
                        AppResources.NotSure));
                if (!response) { return; }

                // Find and remove report.
                var group = Reports.SingleOrDefault(a => a.Date.Year.Equals(report.CreatedAt.Year) && a.Date.DayOfYear.Equals(report.CreatedAt.DayOfYear));
                if (group != null && group.Contains(report))
                {
                    group.Remove(report);
                    await service.DeleteReportAsync(report.Id);
                    OnPropertyChanged(nameof(Reports));
                }
            }
            catch (Exception e)
            {
                var message = string.Format(AppResources.ExceptionMessageReportDelete, e.Message);
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

        private async Task ExecuteSentData(Report report)
        {
            if (IsBusy) { return; }
            IsBusy = true;
            try
            {
                var result = await service.SwitchSentReportAsync(report);
                if (result != null)
                {
                    OnPropertyChanged(nameof(Reports));
                }
            }
            catch (RealmInvalidTransactionException)
            {
                await Device.InvokeOnMainThreadAsync(
                    async () => await Application.Current.MainPage.DisplayAlert(
                        "Error",
                        $"Error while saving data: RealmInvalidTransactionException. Please, contact the developer.",
                        "Ouch..."));
            }
            catch (Exception e)
            {
                var message = string.Format(AppResources.ExceptionMessageReportSend, e.Message);
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

        public ReportGroup FindReportGroup(Report report) => Reports.SingleOrDefault(s => s.Date.Year.Equals(report.CreatedAt.Year) && s.Date.DayOfYear.Equals(report.CreatedAt.DayOfYear));
    }
}
