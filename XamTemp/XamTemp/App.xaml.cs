namespace XamTemp
{
    using Xamarin.Forms;
    using XamTemp.Services;

    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            DependencyService.Register<ReportService>();

            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
