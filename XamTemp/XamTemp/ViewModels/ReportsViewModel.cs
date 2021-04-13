namespace XamTemp.ViewModels
{
    using Microcharts;
    using MvvmHelpers;
    using Realms.Exceptions;
    using SkiaSharp;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Xamarin.Forms;
    using XamTemp.Models;
    using XamTemp.Services;

    class ReportsViewModel : BaseViewModel
    {
        readonly ReportService service;

        public ObservableCollection<ReportGroup> Reports { get; set; }

        public LineChart Chart { get; set; }

        public ReportsViewModel()
        {
            Title = "Temperatures";
            service = DependencyService.Get<ReportService>();
            Reports = new ObservableCollection<ReportGroup>();
            Chart = new LineChart
            {
                LabelOrientation = Orientation.Horizontal,
                ValueLabelOrientation = Orientation.Horizontal,
                LineSize = 5,
                LabelTextSize = 20,
                LabelColor = SKColor.Parse("#fdd835")
            };
            LoadTemperaturesCommand = new Command(async () => await ExecuteLoadTemperature());
            AddTemperatureCommand = new Command(async () => await ExecuteAddTemperature());
            RemoveReportCommand = new Command<Report>(async (report) => await ExecuteDeleteTemperature(report));
            ResetDataCommand = new Command(async () => await ExecuteResetData());
            SentDataCommand = new Command<Report>(async (report) => await ExecuteSentData(report));
        }

        public Command LoadTemperaturesCommand { get; set; }
        public Command AddTemperatureCommand { get; set; }
        public Command RemoveReportCommand { get; set; }
        public Command ResetDataCommand { get; set; }
        public Command SentDataCommand { get; set; }
        private async Task ExecuteLoadTemperature()
        {
            if (this.IsBusy) return;
            IsBusy = true;
            try
            {
                // Update reports.
                Reports.Clear();
                var reports = await service.GetReportsAsync();
                var entries = new List<ChartEntry>();
                foreach (var elem in reports)
                {
                    AddReport(elem);
                }
                OnPropertyChanged(nameof(Reports));

                // Update line chart.
                foreach (var group in Reports)
                {
                    var label = group.Date.ToLocalTime().Date.ToShortDateString();
                    var value = group.Average(a => a.Temperature);
                    entries.Add(new ChartEntry(float.Parse(value.ToString()))
                    {
                        Label = label,
                        ValueLabel = value.ToString("0.00"),
                        Color = value > 37.5 ? SKColor.Parse("#F00") : SKColor.Parse("#0F0")
                    });
                }
                Chart.Entries = entries;
                Chart.MinValue = entries.Min(m => m.Value);
                Chart.MaxValue = entries.Max(m => m.Value);
                OnPropertyChanged(nameof(Chart));
            }
            finally
            {
                IsBusy = false;
            }
        }
        private async Task ExecuteAddTemperature()
        {
            if (this.IsBusy) return;
            IsBusy = true;
            try
            {
                var temperature = await Device.InvokeOnMainThreadAsync(async () =>
                await Application.Current.MainPage.DisplayPromptAsync("Temperature", "Input Temperature", "Confirm", "Cancel", "Temperature", keyboard: Keyboard.Numeric));
                var saturation = await Device.InvokeOnMainThreadAsync(async () =>
                await Application.Current.MainPage.DisplayPromptAsync("Saturation", "Input your saturation", "Ok", "Cancel", "Saturation", keyboard: Keyboard.Numeric));
                var intSaturation = int.Parse(saturation);
                var doubleTemperature = double.Parse(temperature);
                var added = await service.AddReportAsync(new Report
                {
                    Saturation = intSaturation,
                    Temperature = doubleTemperature
                });
                AddReport(added);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Exception thrown deleting a report: {e.Message}");
                await Device.InvokeOnMainThreadAsync(
                    async () => await Application.Current.MainPage.DisplayAlert(
                        "Error",
                        $"Error while adding data: {e.Message}",
                        "Bad..."));
            }
            finally
            {
                IsBusy = false;
            }
        }

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
                Reports.Add(new ReportGroup(report.CreatedAt, newList));
            }
        }

        private async Task ExecuteDeleteTemperature(Report report)
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                // Ask consensus.
                var response = await Device.InvokeOnMainThreadAsync(
                    async () => await Application.Current.MainPage.DisplayAlert(
                        "Delete",
                        "Are you sure to delete this report? (You will not be able to retreive it)",
                        "Yes, I'm sure!",
                        "No, don't do it."));
                if (!response) return;

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
                Debug.WriteLine($"Exception thrown deleting a report: {e.Message}");
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

        private async Task ExecuteResetData()
        {
            if (IsBusy) return;
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
                    Reports.Clear();
                    Chart.Entries = new List<ChartEntry>();
                    OnPropertyChanged(nameof(Chart));
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

        private async Task ExecuteSentData(Report report)
        {
            if (IsBusy) return;
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
                Debug.WriteLine($"Exception thrown resetting data: {e.Message}");
                await Device.InvokeOnMainThreadAsync(
                    async () => await Application.Current.MainPage.DisplayAlert(
                        "Error",
                        $"Error while sending data: {e.Message}",
                        "Bad..."));
            }
            finally
            {
                IsBusy = false;
            }
        }

        public ReportGroup FindReportGroup(Report report) => Reports.SingleOrDefault(s => s.Date.Year.Equals(report.CreatedAt.Year) && s.Date.DayOfYear.Equals(report.CreatedAt.DayOfYear));
    }
}
