using System.Windows;
using System.Windows.Controls;

namespace ElectronicStore.Pages
{
    public partial class ManagerPage : Page
    {
        public ManagerPage()
        {
            Application.Current.MainWindow.Title = "Панель менеджера";
            InitializeComponent();
        }

        private void GoProducts(object sender, RoutedEventArgs e) {
            NavigationService.Navigate(new ProductsManagePage());
        }
            

        private void GoCategories(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new CategoriesManagePage());
        }
            

        private void GoTags(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new TagsManagePage());
        }
            

        private void GoBrands(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new BrandsManagePage());
        }
            

        private void GoBackProducts(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ProductsPage(true));
        }
            
    }
}
