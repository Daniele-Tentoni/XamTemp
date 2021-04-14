namespace XamTemp.ViewModels
{
    using Microcharts;
    using SkiaSharp;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Xamarin.Forms;

    class ChartsViewModel : BaseReportViewModel
    {
        public LineChart TemperaturesChart { get; set; }
        public LineChart SaturationsChart { get; set; }
        public Command LoadChartsCommand { get; set; }
        public ChartsViewModel()
        {
            // TODO: Get the color dinamically, as property.
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
            LoadChartsCommand = new Command(async () => await ExecuteLoadCharts().ConfigureAwait(false));
        }
        private async Task ExecuteLoadCharts()
        {
            if (IsBusy) { return; }
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
                        Color = ConvertTemperatureToColor(temperature)
                    });
                    saturations.Add(new ChartEntry(float.Parse(saturation.ToString()))
                    {
                        Label = label,
                        ValueLabel = saturation.ToString("0.00"),
                        Color = ConvertSaturationToColor(saturation)
                    });
                }
                if (temperatures.Any())
                {
                    temperatures = temperatures.OrderByDescending(o => o.Label).ToList();
                    TemperaturesChart.Entries = temperatures;
                    TemperaturesChart.MinValue = temperatures.Min(m => m.Value);
                    TemperaturesChart.MaxValue = temperatures.Max(m => m.Value);
                    OnPropertyChanged(nameof(TemperaturesChart));
                }
                if (saturations.Any())
                {
                    saturations = saturations.OrderByDescending(o => o.Label).ToList();
                    SaturationsChart.Entries = saturations;
                    SaturationsChart.MinValue = saturations.Min(m => m.Value);
                    SaturationsChart.MaxValue = saturations.Max(m => m.Value);
                    OnPropertyChanged(nameof(SaturationsChart));
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Exception thrown from Chart loader: {e.Message}");
            }
            finally { IsBusy = false; }
        }
        private SKColor ConvertTemperatureToColor(double temperature)
        {
            if (temperature > 37.5 || temperature < 32) { return red; }
            if (temperature > 37 || temperature < 35) { return yellow; }
            return green;
        }
        private SKColor ConvertSaturationToColor(double saturation)
        {
            if (saturation > 95) { return green; }
            if (saturation > 90) { return yellow; }
            return red;
        }

        private readonly SKColor red = SKColor.Parse("#e74c3c");
        private readonly SKColor yellow = SKColor.Parse("#f9e79f");
        private readonly SKColor green = SKColor.Parse("#a9dfbf");
    }
}
