namespace XamTemp.ViewModels
{
    using Microcharts;
    using MvvmHelpers;
    using SkiaSharp;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Xamarin.Forms;
    using XamTemp.Services;

    class ChartsViewModel : BaseReportViewModel
    {
        public LineChart TemperaturesChart { get; set; }
        public LineChart SaturationsChart { get; set; }
        public Command LoadChartsCommand { get; set; }
        public ChartsViewModel()
        {
            Title = "Charts";
            TemperaturesChart = new LineChart
            {
                LabelOrientation = Orientation.Horizontal,
                ValueLabelOrientation = Orientation.Horizontal,
                LineSize = 5,
                LabelTextSize = 20,
                LabelColor = SKColor.Parse("#fdd835")
            };
            SaturationsChart = new LineChart
            {
                LabelOrientation = Orientation.Horizontal,
                ValueLabelOrientation = Orientation.Horizontal,
                LineSize = 5,
                LabelTextSize = 20,
                LabelColor = SKColor.Parse("#fdd835")
            };
            LoadChartsCommand = new Command(async () => await ExecuteLoadCharts());
        }
        private async Task ExecuteLoadCharts()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {

                var reports = await service.GetReportsAsync();
                var grouped = reports.GroupBy(g => g.CreatedAt.ToLocalTime().DateTime.ToShortDateString());
                var temperatures = new List<ChartEntry>();
                var saturations = new List<ChartEntry>();
                // Update line chart.
                foreach (var group in grouped)
                {
                    var label = group.Key;
                    var temperature = group.Average(a => a.Temperature);
                    var saturation = group.Average(a => a.Saturation);
                    temperatures.Add(new ChartEntry(float.Parse(temperature.ToString()))
                    {
                        Label = label,
                        ValueLabel = temperature.ToString("0.00"),
                        Color = temperature > 37.5 ? SKColor.Parse("#F00") : SKColor.Parse("#0F0")
                    });
                    saturations.Add(new ChartEntry(float.Parse(saturation.ToString()))
                    {
                        Label = label,
                        ValueLabel = saturation.ToString("0.00"),
                        Color = saturation < 90 ? SKColor.Parse("#F00") : SKColor.Parse("#0F0")
                    });
                }
                TemperaturesChart.Entries = temperatures;
                TemperaturesChart.MinValue = temperatures.Min(m => m.Value);
                TemperaturesChart.MaxValue = temperatures.Max(m => m.Value);
                OnPropertyChanged(nameof(TemperaturesChart));
                SaturationsChart.Entries = saturations;
                SaturationsChart.MinValue = saturations.Min(m => m.Value);
                SaturationsChart.MaxValue = saturations.Max(m => m.Value);
                OnPropertyChanged(nameof(SaturationsChart));
            }
            finally { IsBusy = false; }
        }
    }
}
