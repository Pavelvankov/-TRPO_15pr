using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ElectronicStore.Models;

namespace ElectronicStore.Pages
{
    public partial class TagsManagePage : Page
    {
        private readonly ElectronicsStorePr15Context db = new();
        private Tag selectedItem = null;

        public TagsManagePage()
        {
            Application.Current.MainWindow.Title = "Теги";
            InitializeComponent();
            LoadList();
        }

        private void LoadList()
        {
            ItemsList.ItemsSource = db.Tags.OrderBy(t => t.Name).ToList();
        }

        private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ItemsList.SelectedItem is Tag tag)
            {
                selectedItem = tag;
                NameBox.Text = tag.Name;
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
            var duplicate = db.Tags.FirstOrDefault(t =>
                t.Name.ToLower() == name.ToLower() &&
                (selectedItem == null || t.Id != selectedItem.Id));

            if (duplicate != null)
            {
                MessageBox.Show("Ошибка: тег с таким названием уже существует");
                return;
            }

            if (selectedItem == null)
            {
                var tag = new Tag
                {
                    Id = db.Tags.Any() ? db.Tags.Max(t => t.Id) + 1 : 1,
                    Name = name
                };
                db.Tags.Add(tag);
            }
            else
            {
                selectedItem.Name = name;
                db.Tags.Update(selectedItem);
            }

            db.SaveChanges();
            LoadList();
        }

        private void Delete(object sender, RoutedEventArgs e)
        {
            if (selectedItem == null) { MessageBox.Show("Выберите тег"); return; }

            if (MessageBox.Show("Удалить тег?", "Подтверждение",
                MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                db.Tags.Remove(selectedItem);
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

        private void AddNew(object sender, RoutedEventArgs e)
        {

        }
    }
}
