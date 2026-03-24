using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ElectronicStore.Pages
{
    /// <summary>
    /// Логика взаимодействия для LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            Application.Current.MainWindow.Title = "Вход";
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ProductsPage(false));
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (pin.Password == "1234")
            {
                NavigationService.Navigate(new ProductsPage(true));
            }
            else
            {
                MessageBox.Show("Неверный пароль");
            }
        }
    }
}
