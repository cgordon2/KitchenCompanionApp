namespace RecipePOC
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));


            // Routing.RegisterRoute(nameof(HomePage), typeof(HomePage));
            Routing.RegisterRoute(nameof(Notifications), typeof(Notifications)); 
           // Routing.RegisterRoute(nameof(Search), typeof(Search)); 
            Routing.RegisterRoute(nameof(CreateRecipe), typeof(CreateRecipe));
            Routing.RegisterRoute(nameof(Profile), typeof(Profile));
            Routing.RegisterRoute(nameof(IngredientDetail), typeof(IngredientDetail));
            Routing.RegisterRoute(nameof(AIAssistantShopping), typeof(AIAssistantShopping));
            Routing.RegisterRoute(nameof(CreateIngredient), typeof(CreateIngredient));

            Routing.RegisterRoute(nameof(SetupProfile), typeof(SetupProfile));
            Routing.RegisterRoute(nameof(Followers), typeof(Followers));
            Routing.RegisterRoute(nameof(Following), typeof(Following));
            Routing.RegisterRoute(nameof(RegisterUser), typeof(RegisterUser)); 
        }
    }
}
