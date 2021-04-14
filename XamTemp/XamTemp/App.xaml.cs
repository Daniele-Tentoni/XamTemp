namespace XamTemp
{
    using Xamarin.Forms;
    using XamTemp.Services;

    public partial class App : Application
    {
        public static readonly string DataReset = "DataReset";

        public App()
        {
            InitializeComponent();

            DependencyService.Register<ReportService>();

            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
            // Does nothing at the moment.
            // TODO: Add notifications.
        }

        protected override void OnSleep()
        {
            // Does nothing at the moment.
        }

        protected override void OnResume()
        {
            // Does nothing at the moment.
        }
    }
}
