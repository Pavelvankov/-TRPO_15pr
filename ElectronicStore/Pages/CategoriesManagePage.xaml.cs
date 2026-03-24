using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ElectronicStore.Models;

namespace ElectronicStore.Pages
{
    public partial class CategoriesManagePage : Page
    {
        private readonly ElectronicsStorePr15Context db = new();
        private Category selectedItem = null;

        public CategoriesManagePage()
        {
            Application.Current.MainWindow.Title = "Категории";
            InitializeComponent();
            LoadList();
        }

        private void LoadList()
        {
            ItemsList.ItemsSource = db.Categories.OrderBy(c => c.Name).ToList();
        }

        private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ItemsList.SelectedItem is Category cat)
            {
                selectedItem = cat;
                NameBox.Text = cat.Name;
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
            var duplicate = db.Categories.FirstOrDefault(c =>
                c.Name.ToLower() == name.ToLower() &&
                (selectedItem == null || c.Id != selectedItem.Id));

            if (duplicate != null)
            {
                MessageBox.Show("Ошибка: категория с таким названием уже существует");
                return;
            }

            if (selectedItem == null)
            {
                var cat = new Category
                {
                    Id = db.Categories.Any() ? db.Categories.Max(c => c.Id) + 1 : 1,
                    Name = name
                };
                db.Categories.Add(cat);
            }
            else
            {
                selectedItem.Name = name;
                db.Categories.Update(selectedItem);
            }

            db.SaveChanges();
            LoadList();
        }

        private void Delete(object sender, RoutedEventArgs e)
        {
            if (selectedItem == null) { MessageBox.Show("Выберите категорию"); return; }

            if (MessageBox.Show("Удалить категорию?", "Подтверждение",
                MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                db.Categories.Remove(selectedItem);
                db.SaveChanges();
                selectedItem = null;
                NameBox.Text = "";
                LoadList();
            }
        }

        private void GoBack(object sender, RoutedEventArgs e) =>
            NavigationService.Navigate(new ManagerPage());
    }
}
