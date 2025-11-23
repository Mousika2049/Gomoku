namespace GomokuAI
{
    public partial class StartPage : ContentPage
    {
        public StartPage()
        {
            InitializeComponent();
        }

        private async void OnEasyClicked(object sender, EventArgs e)
        {
            // 深度 2
            await Navigation.PushAsync(new MainPage(2));
        }

        private async void OnMediumClicked(object sender, EventArgs e)
        {
            // 深度 3
            await Navigation.PushAsync(new MainPage(3));
        }

        private async void OnHardClicked(object sender, EventArgs e)
        {
            // 深度 4
            await Navigation.PushAsync(new MainPage(4));
        }
    }
}