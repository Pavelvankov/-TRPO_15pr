using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using ElectronicStore.Models;
using Microsoft.EntityFrameworkCore;

namespace ElectronicStore.Pages;

public partial class ProductsPage : Page
{

    public ObservableCollection<Product> products { get; set; } = new();
    public ICollectionView productsView { get; set; }

    private readonly ElectronicsStorePr15Context db = new();

    public string searchQuery { get; set; } = null!;
    public string filterPriceFrom { get; set; } = null!;
    public string filterPriceTo { get; set; } = null!;

    private Category selectedCategory;
    private Brand selectedBrand;

    public List<Category> categories { get; set; } = new();
    public List<Brand> brands { get; set; } = new();

    public ProductsPage(bool isManager = false)
    {
        Application.Current.MainWindow.Title = "магазина электроники";
        productsView = CollectionViewSource.GetDefaultView(products);
        productsView.Filter = FilterProducts;
        InitializeComponent();
        LoadList();

        if (isManager)
        {
            ManagerPanelButton.Visibility = Visibility.Visible;
        }
    }

    public void LoadList()
    {
        products.Clear();
        foreach (var p in db.Products.Include(p => p.Category).Include(p => p.Brand).Include(p => p.Tags).ToList())
            products.Add(p);

        categories = db.Categories.OrderBy(c => c.Name).ToList();
        brands = db.Brands.OrderBy(b => b.Name).ToList();

        CategoryCombo.ItemsSource = categories;
        BrandCombo.ItemsSource = brands;
    }

    public bool FilterProducts(object obj)
    {
        if (obj is not Product product)
            return false;

        if (!string.IsNullOrEmpty(searchQuery) &&
            !product.Name.Contains(searchQuery, StringComparison.CurrentCultureIgnoreCase))
            return false;

        if (selectedCategory != null && product.CategoryId != selectedCategory.Id)
            return false;

        if (selectedBrand != null && product.BrandId != selectedBrand.Id)
            return false;

        if (!string.IsNullOrEmpty(filterPriceFrom) &&
            decimal.TryParse(filterPriceFrom, out decimal priceFrom) &&
            product.Price < priceFrom)
            return false;

        if (!string.IsNullOrEmpty(filterPriceTo) &&
            decimal.TryParse(filterPriceTo, out decimal priceTo) &&
            product.Price > priceTo)
            return false;

        return true;
    }

    private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {

        bool fromHasLetters = !string.IsNullOrEmpty(filterPriceFrom) && !decimal.TryParse(filterPriceFrom, out _);
        bool toHasLetters   = !string.IsNullOrEmpty(filterPriceTo) && !decimal.TryParse(filterPriceTo, out _);

        if (fromHasLetters || toHasLetters)
        {
            Error.Text = "цена должна содержать только цифры";
            Error.Visibility = Visibility.Visible;
            return;
        }
        
        if (!string.IsNullOrEmpty(filterPriceFrom) && !string.IsNullOrEmpty(filterPriceTo))
        {
            if (decimal.TryParse(filterPriceFrom, out decimal from) &&
                decimal.TryParse(filterPriceTo, out decimal to))
            {
                if (from > to)
                {
                    Error.Text = "от не должна превышать до";
                    Error.Visibility = Visibility.Visible;
                    return;
                }
            }
        }
        Error.Visibility = Visibility.Collapsed;
        productsView.Refresh();
    }

    private void SortCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        productsView.SortDescriptions.Clear();
        var selected = (ComboBoxItem)((ComboBox)sender).SelectedItem;

        switch (selected.Tag)
        {
            case "NameAsc":
                productsView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
                break;
            case "PriceAsc":
                productsView.SortDescriptions.Add(new SortDescription("Price", ListSortDirection.Ascending));
                break;
            case "PriceDesc":
                productsView.SortDescriptions.Add(new SortDescription("Price", ListSortDirection.Descending));
                break;
            case "StockAsc":
                productsView.SortDescriptions.Add(new SortDescription("Stock", ListSortDirection.Ascending));
                break;
            case "StockDesc":
                productsView.SortDescriptions.Add(new SortDescription("Stock", ListSortDirection.Descending));
                break;
            default:
                break;
        }
        productsView.Refresh();
    }

    private void CategoryCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        selectedCategory = CategoryCombo.SelectedItem as Category;
        productsView.Refresh();
    }

    private void BrandCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        selectedBrand = BrandCombo.SelectedItem as Brand;
        productsView.Refresh();
    }

    private void ResetFilters(object sender, RoutedEventArgs e)
    {
        searchQuery = null!;
        filterPriceFrom = null!;
        filterPriceTo = null!;
        SearchBox.Text = "";
        PriceFromBox.Text = "";
        PriceToBox.Text = "";
        CategoryCombo.SelectedItem = null;
        BrandCombo.SelectedItem = null;

        productsView.SortDescriptions.Clear();
        productsView.Refresh();
    }

    private void ProductsList_ContextMenuClosing(object sender, ContextMenuEventArgs e)
    {
    }

    private void ManagerPanelButton_Click(object sender, RoutedEventArgs e)
    {
        NavigationService.Navigate(new ManagerPage());
    }
}

public class StockToBorderBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int stock && stock < 10)
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffd129"));
        return new SolidColorBrush(Colors.Transparent);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
