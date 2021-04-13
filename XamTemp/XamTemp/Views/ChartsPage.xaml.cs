namespace XamTemp.Views
{
    using Xamarin.Forms;
    using Xamarin.Forms.Xaml;
    using XamTemp.ViewModels;

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChartsPage : ContentPage
    {
        private readonly ChartsViewModel viewModel;
        public ChartsPage()
        {
            InitializeComponent();
            viewModel = (ChartsViewModel)BindingContext;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            viewModel.LoadChartsCommand.Execute(null);
        }
    }
}