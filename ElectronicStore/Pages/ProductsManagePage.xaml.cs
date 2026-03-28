using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ElectronicStore.Models;
using Microsoft.EntityFrameworkCore;

namespace ElectronicStore.Pages
{
    public partial class ProductsManagePage : Page
    {
        private readonly ElectronicsStorePr15Context db = new();
        private Product selectedItem = null;

        public ProductsManagePage()
        {
            Application.Current.MainWindow.Title = "Управление товарами";
            InitializeComponent();
            LoadCombos();
            LoadList();
        }

        private void LoadCombos()
        {
            CategoryCombo.ItemsSource = db.Categories.OrderBy(c => c.Name).ToList();
            BrandCombo.ItemsSource = db.Brands.OrderBy(b => b.Name).ToList();
            TagsList.ItemsSource = db.Tags.OrderBy(t => t.Name).ToList();
        }

        private void LoadList()
        {
            ItemsList.ItemsSource = db.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Tags)
                .OrderBy(p => p.Name)
                .ToList();
        }

        private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ItemsList.SelectedItem is Product p)
            {
                selectedItem = p;
                NameBox.Text = p.Name;
                DescriptionBox.Text = p.Description;
                PriceBox.Text = p.Price.ToString();
                StockBox.Text = p.Stock.ToString();
                RatingBox.Text = p.Rating.ToString();
                CreatedAtBox.Text = p.CreatedAt.ToString("yyyy-MM-dd");

                CategoryCombo.SelectedItem = ((System.Collections.Generic.List<Category>)CategoryCombo.ItemsSource)
                    .FirstOrDefault(c => c.Id == p.CategoryId);
                BrandCombo.SelectedItem = ((System.Collections.Generic.List<Brand>)BrandCombo.ItemsSource)
                    .FirstOrDefault(b => b.Id == p.BrandId);

                TagsList.SelectedItems.Clear();
                foreach (var tag in p.Tags)
                {
                    var listTag = ((System.Collections.Generic.List<Tag>)TagsList.ItemsSource)
                        .FirstOrDefault(t => t.Id == tag.Id);
                    if (listTag != null)
                        TagsList.SelectedItems.Add(listTag);
                }
            }
        }

        private void AddNew(object sender, RoutedEventArgs e)
        {
            selectedItem = null;
            NameBox.Text = "";
            DescriptionBox.Text = "";
            PriceBox.Text = "";
            StockBox.Text = "";
            RatingBox.Text = "";
            CreatedAtBox.Text = DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd");
            CategoryCombo.SelectedItem = null;
            BrandCombo.SelectedItem = null;
            TagsList.SelectedItems.Clear();
            ItemsList.SelectedItem = null;
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text) ||
                string.IsNullOrWhiteSpace(DescriptionBox.Text) ||
                string.IsNullOrWhiteSpace(PriceBox.Text) ||
                string.IsNullOrWhiteSpace(StockBox.Text) ||
                string.IsNullOrWhiteSpace(RatingBox.Text) ||
                string.IsNullOrWhiteSpace(CreatedAtBox.Text) ||
                CategoryCombo.SelectedItem == null ||
                BrandCombo.SelectedItem == null)
            {
                MessageBox.Show("Ошибка: заполните все обязательные поля");
                return;
            }

            if (!decimal.TryParse(PriceBox.Text, out decimal price))
            {
                MessageBox.Show("Ошибка: цена должна быть числом");
                return;
            }
            if (!int.TryParse(StockBox.Text, out int stock))
            {
                MessageBox.Show("Ошибка: количество должно быть целым числом");
                return;
            }
            if (!decimal.TryParse(RatingBox.Text, out decimal rating) || rating < 0 || rating > 5)
            {
                MessageBox.Show("Ошибка: рейтинг должен быть числом от 0 до 5");
                return;
            }
            if (!DateOnly.TryParse(CreatedAtBox.Text, out DateOnly createdAt))
            {
                MessageBox.Show("Ошибка: дата должна быть в формате гггг-мм-дд");
                return;
            }

            var name = NameBox.Text.Trim();


            var selectedTags = TagsList.SelectedItems.Cast<Tag>().ToList();

            if (selectedItem == null)
            {
                var product = new Product
                {
                    Id = db.Products.Any() ? db.Products.Max(p => p.Id) + 1 : 1,
                    Name = name,
                    Description = DescriptionBox.Text.Trim(),
                    Price = price,
                    Stock = stock,
                    Rating = rating,
                    CreatedAt = createdAt,
                    CategoryId = ((Category)CategoryCombo.SelectedItem).Id,
                    BrandId = ((Brand)BrandCombo.SelectedItem).Id
                };
                foreach (var tag in selectedTags)
                    product.Tags.Add(tag);
                db.Products.Add(product);
            }
            else
            {
                var tracked = db.Products.Include(p => p.Tags).First(p => p.Id == selectedItem.Id);
                tracked.Name = name;
                tracked.Description = DescriptionBox.Text.Trim();
                tracked.Price = price;
                tracked.Stock = stock;
                tracked.Rating = rating;
                tracked.CreatedAt = createdAt;
                tracked.CategoryId = ((Category)CategoryCombo.SelectedItem).Id;
                tracked.BrandId = ((Brand)BrandCombo.SelectedItem).Id;
                tracked.Tags.Clear();
                foreach (var tag in selectedTags)
                    tracked.Tags.Add(tag);
            }

            db.SaveChanges();
            LoadList();
        }

        private void Delete(object sender, RoutedEventArgs e)
        {
            if (selectedItem == null) { MessageBox.Show("Выберите товар"); return; }


            var hasProducts = db.Products.Any(p => p.BrandId == selectedItem.Id);

            if (hasProducts)
            {
                MessageBox.Show("Нельзя удалить бренд. Сначала удалите товары этого бренда.");
                return;
            }


            if (MessageBox.Show("Удалить товар?", "Подтверждение",
                MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var tracked = db.Products.Include(p => p.Tags).First(p => p.Id == selectedItem.Id);
                tracked.Tags.Clear();
                db.Products.Remove(tracked);
                db.SaveChanges();
                selectedItem = null;
                AddNew(null, null);
                LoadList();
            }
        }

        private void GoBack(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ManagerPage());
        }
        private void RatingBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
   
        }

        private void RatingBox_PreviewTextInput_1(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, 0) && e.Text != ",")
            {
                e.Handled = true;
            }
        }

        private void PriceBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, 0) && e.Text != ",")
            {
                e.Handled = true;
            }
        }
    }
}
