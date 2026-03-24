using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ElectronicStore.Models;

namespace ElectronicStore.Pages
{
    public partial class BrandsManagePage : Page
    {
        private readonly ElectronicsStorePr15Context db = new();
        private Brand selectedItem = null;

        public BrandsManagePage()
        {
            Application.Current.MainWindow.Title = "Бренды";
            InitializeComponent();
            LoadList();
        }

        private void LoadList()
        {
            ItemsList.ItemsSource = db.Brands.OrderBy(b => b.Name).ToList();
        }

        private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ItemsList.SelectedItem is Brand brand)
            {
                selectedItem = brand;
                NameBox.Text = brand.Name;
            }
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                MessageBox.Show("Ошибка: заполните поле «Название»");
                return;
            }

            var name = NameBox.Text.Trim();
            var duplicate = db.Brands.FirstOrDefault(b =>
                b.Name.ToLower() == name.ToLower() &&
                (selectedItem == null || b.Id != selectedItem.Id));

            if (duplicate != null)
            {
                MessageBox.Show("Ошибка: бренд с таким названием уже существует");
                return;
            }

            if (selectedItem == null)
            {
                var brand = new Brand
                {
                    Id = db.Brands.Any() ? db.Brands.Max(b => b.Id) + 1 : 1,
                    Name = name
                };
                db.Brands.Add(brand);
            }
            else
            {
                selectedItem.Name = name;
                db.Brands.Update(selectedItem);
            }

            db.SaveChanges();
            LoadList();
        }

        private void Delete(object sender, RoutedEventArgs e)
        {
            if (selectedItem == null) { MessageBox.Show("Выберите бренд"); return; }

            if (MessageBox.Show("Удалить бренд?", "Подтверждение",
                MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                db.Brands.Remove(selectedItem);
                db.SaveChanges();
                selectedItem = null;
                NameBox.Text = "";
                LoadList();
            }
        }

        private void GoBack(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ManagerPage());
        }
    }
}
