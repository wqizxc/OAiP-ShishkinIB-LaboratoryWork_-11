using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WarehouseApp.Models;

namespace WarehouseApp
{
    public partial class DashboardWindow : Window
    {
        private readonly int _userId;
        private int _selectedWaybillId = 0;
        private int _selectedProductId = 0;

        public DashboardWindow(int userId)
        {
            InitializeComponent();
            _userId = userId;
            LoadUserInfo();
            RefreshList();
            StatusMsg.Text = " Полный доступ к накладным и товарам склада.";
        }

        private void LoadUserInfo()
        {
            var db = WarehouseContext.GetContext();
            var user = db.Employees.Find(_userId);
            if (user != null)
            {
                WelcomeText.Text = $"{user.LastName} {user.FirstName}";
                RoleText.Text = $"Должность: {user.Position}";
            }
        }

        private void RefreshList()
        {
            var db = WarehouseContext.GetContext();
            var data = db.Waybills.OrderByDescending(w => w.Date).ToList();
            WaybillGrid.ItemsSource = data;
            StatusMsg.Text = $" Загружено накладных: {data.Count}";
            ClearWaybillForm();
        }
        private void AddWaybill_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FormNumber.Text) || !FormDate.SelectedDate.HasValue)
            { StatusMsg.Text = " Заполните номер и дату накладной"; return; }

            var db = WarehouseContext.GetContext();
            db.Waybills.Add(new Waybill
            {
                Number = FormNumber.Text.Trim(),
                Date = FormDate.SelectedDate.Value,
                Type = ((ComboBoxItem)FormType.SelectedItem).Content.ToString(),
                EmployeeId = _userId
            });
            db.SaveChanges();
            StatusMsg.Text = " Накладная добавлена";
            RefreshList();
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            var db = WarehouseContext.GetContext();
            if (_selectedProductId > 0 && !string.IsNullOrWhiteSpace(FormProductName.Text))
            {
                var product = db.Products.Find(_selectedProductId);
                if (product != null && product.WaybillId == _selectedWaybillId)
                {
                    if (string.IsNullOrWhiteSpace(FormProductArticle.Text))
                    { StatusMsg.Text = " Заполните артикул товара"; return; }
                    if (!int.TryParse(FormProductQuantity.Text, out int qty) || qty <= 0)
                    { StatusMsg.Text = " Укажите корректное количество (>0)"; return; }

                    product.Name = FormProductName.Text.Trim();
                    product.Article = FormProductArticle.Text.Trim();
                    product.Quantity = qty;

                    db.SaveChanges();
                    StatusMsg.Text = " Товар успешно обновлён";
                    LoadProducts(_selectedWaybillId);
                    ClearProductForm();
                    return;
                }
            }
            if (_selectedWaybillId > 0)
            {
                if (string.IsNullOrWhiteSpace(FormNumber.Text) || !FormDate.SelectedDate.HasValue)
                { StatusMsg.Text = " Заполните номер и дату накладной"; return; }

                var waybill = db.Waybills.Find(_selectedWaybillId);
                if (waybill != null)
                {
                    waybill.Number = FormNumber.Text.Trim();
                    waybill.Date = FormDate.SelectedDate.Value;
                    waybill.Type = ((ComboBoxItem)FormType.SelectedItem).Content.ToString();

                    db.SaveChanges();
                    StatusMsg.Text = " Накладная успешно обновлена";
                    RefreshList();
                }
            }
            else
            {
                StatusMsg.Text = "️ Сначала выберите запись в таблице";
            }
        }

        private void DeleteWaybill_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedWaybillId == 0) { StatusMsg.Text = " Выберите запись"; return; }
            if (MessageBox.Show("Удалить накладную и все её товары?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var db = WarehouseContext.GetContext();
                var w = db.Waybills.Find(_selectedWaybillId);
                if (w != null)
                {
                    db.Waybills.Remove(w);
                    db.SaveChanges();
                    StatusMsg.Text = " Накладная удалена";
                    RefreshList();
                }
            }
        }
        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedWaybillId == 0) { StatusMsg.Text = "️ Сначала выберите накладную"; return; }
            if (string.IsNullOrWhiteSpace(FormProductName.Text) || string.IsNullOrWhiteSpace(FormProductArticle.Text))
            { StatusMsg.Text = " Заполните название и артикул"; return; }
            if (!int.TryParse(FormProductQuantity.Text, out int qty) || qty <= 0)
            { StatusMsg.Text = " Некорректное количество"; return; }

            var db = WarehouseContext.GetContext();
            db.Products.Add(new Product
            {
                Name = FormProductName.Text.Trim(),
                Article = FormProductArticle.Text.Trim(),
                Quantity = qty,
                WaybillId = _selectedWaybillId
            });
            db.SaveChanges();
            StatusMsg.Text = " Товар добавлен";
            LoadProducts(_selectedWaybillId);
            ClearProductForm();
        }

        private void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedProductId == 0) { StatusMsg.Text = "️ Выберите товар"; return; }
            var db = WarehouseContext.GetContext();
            var p = db.Products.Find(_selectedProductId);
            if (p != null && p.WaybillId == _selectedWaybillId)
            {
                db.Products.Remove(p);
                db.SaveChanges();
                StatusMsg.Text = " Товар удалён";
                LoadProducts(_selectedWaybillId);
            }
        }
        private void WaybillGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WaybillGrid.SelectedItem is Waybill w)
            {
                _selectedWaybillId = w.Id;
                FormNumber.Text = w.Number;
                FormDate.SelectedDate = w.Date;
                FormType.Text = w.Type;
                LoadProducts(w.Id);
                StatusMsg.Text = $" Выбрана накладная №{w.Number}";
            }
            else { ClearWaybillForm(); ProductGrid.ItemsSource = null; }
        }

        private void ProductGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProductGrid.SelectedItem is Product p)
            {
                _selectedProductId = p.Id;
                FormProductName.Text = p.Name;
                FormProductArticle.Text = p.Article;
                FormProductQuantity.Text = p.Quantity.ToString();
            }
            else ClearProductForm();
        }

        private void LoadProducts(int waybillId)
        {
            var db = WarehouseContext.GetContext();
            ProductGrid.ItemsSource = db.Products.Where(p => p.WaybillId == waybillId).ToList();
        }

        private void ClearWaybillForm()
        {
            FormNumber.Text = ""; FormDate.SelectedDate = DateTime.Today;
            FormType.SelectedIndex = 0; _selectedWaybillId = 0;
            WaybillGrid.SelectedItem = null;
        }

        private void ClearProductForm()
        {
            FormProductName.Text = ""; FormProductArticle.Text = "";
            FormProductQuantity.Text = "1"; _selectedProductId = 0;
            ProductGrid.SelectedItem = null;
        }

        private void RefreshList_Click(object sender, RoutedEventArgs e)
        {
            RefreshList();
            ProductGrid.ItemsSource = null;
            StatusMsg.Text = " Список обновлён";
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            Close();
        }
    }
}