namespace XamTemp.Views
{
    using System.Linq;

    using Xamarin.Forms;
    using Xamarin.Forms.Xaml;
    using XamTemp.ViewModels;

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ReportsPage : ContentPage
    {
        private readonly ReportsViewModel viewModel;
        public ReportsPage()
        {
            InitializeComponent();
            viewModel = (ReportsViewModel)this.BindingContext;
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (!(viewModel?.Reports.Any() ?? true))
            {
                viewModel.LoadTemperaturesCommand.Execute(null);
            }
        }
    }
}