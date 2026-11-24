using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using StockTrack.Controllers;
using StockTrack.Models;

namespace StockTrack.Avalonia.Views;

public partial class MainWindow : Window
{
    private readonly ProductController _productController;
    private readonly StockController _stockController;
    private readonly CategoryController _categoryController;
    private readonly SupplierController _supplierController;
    private readonly UserController _userController;
    private readonly ReportController _reportController;
    private readonly AuthController _authController;

    private readonly User _currentUser;
    private int _selectedProductId = 0;
    private int _selectedUserId = 0;
    private DataTable? _currentReportTable;

    public MainWindow() : this(new User { UserID = 1, Username = "admin", FullName = "System Administrator", Role = "Admin" })
    {
    }

    public MainWindow(User user)
    {
        InitializeComponent();

        _currentUser = user;

        var userRepo = new UserRepository(DatabaseConfig.ConnectionString);
        var catRepo = new CategoryRepository(DatabaseConfig.ConnectionString);
        var supRepo = new SupplierRepository(DatabaseConfig.ConnectionString);
        var prodRepo = new ProductRepository(DatabaseConfig.ConnectionString);
        var stockRepo = new StockRepository(DatabaseConfig.ConnectionString);
        var auditRepo = new AuditRepository(DatabaseConfig.ConnectionString);
        var reportRepo = new ReportRepository(DatabaseConfig.ConnectionString);

        _authController = new AuthController(userRepo, auditRepo);
        _userController = new UserController(userRepo, auditRepo);
        _categoryController = new CategoryController(catRepo, auditRepo);
        _supplierController = new SupplierController(supRepo, auditRepo);
        _productController = new ProductController(prodRepo, auditRepo);
        _stockController = new StockController(stockRepo, prodRepo, auditRepo);
        _reportController = new ReportController(reportRepo, auditRepo);

        ApplyRolePermissions();
        LoadAllData();
    }

    private void ApplyRolePermissions()
    {
        TxtCurrentUser.Text = $"Logged in as: {_currentUser.FullName} ({_currentUser.Role})";

        // Role-based Direction & Authorization:
        if (_currentUser.Role == "Staff")
        {
            TabUsers.IsVisible = false;
            TabReports.IsVisible = false;
            TabSuppliersCategories.IsVisible = false;
            PnlProductActions.IsVisible = false;
            ChkAdminNegativeOverride.IsEnabled = false;
        }
        else if (_currentUser.Role == "Inventory Manager")
        {
            TabUsers.IsVisible = false;
            TabReports.IsVisible = true;
            TabSuppliersCategories.IsVisible = true;
            PnlProductActions.IsVisible = true;
            ChkAdminNegativeOverride.IsEnabled = false;
        }
        else // Admin
        {
            TabUsers.IsVisible = true;
            TabReports.IsVisible = true;
            TabSuppliersCategories.IsVisible = true;
            PnlProductActions.IsVisible = true;
            ChkAdminNegativeOverride.IsEnabled = true;
        }
    }

    private void LoadAllData()
    {
        LoadDashboard();
        LoadDropdowns();
        LoadProducts();
        LoadStockTransactions();
        LoadSuppliersAndCategories();
        if (_currentUser.Role == "Admin")
        {
            LoadUsers();
        }
    }

    private void LoadDashboard()
    {
        try
        {
            var stats = _productController.GetDashboardStats();
            TxtTotalProducts.Text = stats.TotalProducts.ToString("N0");
            TxtLowStockCount.Text = stats.LowStockItems.ToString("N0");
            TxtTotalUnits.Text = stats.TotalStockUnits.ToString("N0");
            TxtLastRefreshed.Text = $"Synced: {DateTime.Now:HH:mm:ss}";

            // Badges
            TxtLowStockSplitBadge.Text = $"{stats.LowStockItems} items";
            TxtLowStockFullBadge.Text = $"{stats.LowStockItems} items";

            var lowStock = _productController.GetLowStockProducts();
            DataGridHelper.Populate(GridLowStock, lowStock);
            DataGridHelper.Populate(GridLowStockFull, lowStock);

            var recent = _stockController.GetRecentTransactions(15);
            TxtTransactionsFullBadge.Text = $"{recent.Rows.Count} transactions";
            DataGridHelper.Populate(GridRecentTransactions, recent);
            DataGridHelper.Populate(GridRecentTransactionsFull, recent);
        }
        catch (Exception ex)
        {
            ShowStatusMessage("Database Error", $"Failed to load dashboard: {ex.Message}");
        }
    }

    // --- Segment View Switcher Events ---
    private void BtnViewSplit_Click(object? sender, RoutedEventArgs e)
    {
        PnlSegmentSplit.IsVisible = true;
        PnlSegmentLowStock.IsVisible = false;
        PnlSegmentTransactions.IsVisible = false;
        UpdateSegmentButtons(BtnViewSplit);
    }

    private void BtnViewLowStock_Click(object? sender, RoutedEventArgs e)
    {
        PnlSegmentSplit.IsVisible = false;
        PnlSegmentLowStock.IsVisible = true;
        PnlSegmentTransactions.IsVisible = false;
        UpdateSegmentButtons(BtnViewLowStock);
    }

    private void BtnViewTransactions_Click(object? sender, RoutedEventArgs e)
    {
        PnlSegmentSplit.IsVisible = false;
        PnlSegmentLowStock.IsVisible = false;
        PnlSegmentTransactions.IsVisible = true;
        UpdateSegmentButtons(BtnViewTransactions);
    }

    private void UpdateSegmentButtons(Button activeButton)
    {
        var activeBg = new SolidColorBrush(Color.Parse("#2B579A"));
        var activeFg = Brushes.White;
        var inactiveBg = new SolidColorBrush(Color.Parse("#E1E1E1"));
        var inactiveFg = new SolidColorBrush(Color.Parse("#333333"));

        BtnViewSplit.Background = (activeButton == BtnViewSplit) ? activeBg : inactiveBg;
        BtnViewSplit.Foreground = (activeButton == BtnViewSplit) ? activeFg : inactiveFg;
        BtnViewSplit.FontWeight = (activeButton == BtnViewSplit) ? FontWeight.Bold : FontWeight.Normal;

        BtnViewLowStock.Background = (activeButton == BtnViewLowStock) ? activeBg : inactiveBg;
        BtnViewLowStock.Foreground = (activeButton == BtnViewLowStock) ? activeFg : inactiveFg;
        BtnViewLowStock.FontWeight = (activeButton == BtnViewLowStock) ? FontWeight.Bold : FontWeight.Normal;

        BtnViewTransactions.Background = (activeButton == BtnViewTransactions) ? activeBg : inactiveBg;
        BtnViewTransactions.Foreground = (activeButton == BtnViewTransactions) ? activeFg : inactiveFg;
        BtnViewTransactions.FontWeight = (activeButton == BtnViewTransactions) ? FontWeight.Bold : FontWeight.Normal;
    }

    private void BtnQuickRestock_Click(object? sender, RoutedEventArgs e)
    {
        MainTabs.SelectedIndex = 2; // Jump to Stock Movements tab
    }

    private void BtnQuickCatalog_Click(object? sender, RoutedEventArgs e)
    {
        MainTabs.SelectedIndex = 1; // Jump to Products tab
    }

    private void LoadDropdowns()
    {
        try
        {
            var catTable = _categoryController.GetCategories();
            var categories = new List<KeyValuePair<int, string>>();
            foreach (DataRow row in catTable.Rows)
            {
                categories.Add(new KeyValuePair<int, string>(
                    Convert.ToInt32(row["CategoryID"]),
                    row["CategoryName"].ToString() ?? ""));
            }
            CboCategory.ItemsSource = categories;
            if (categories.Count > 0) CboCategory.SelectedIndex = 0;

            var supTable = _supplierController.GetSuppliers();
            var suppliers = new List<KeyValuePair<int, string>>();
            foreach (DataRow row in supTable.Rows)
            {
                suppliers.Add(new KeyValuePair<int, string>(
                    Convert.ToInt32(row["SupplierID"]),
                    row["SupplierName"].ToString() ?? ""));
            }
            CboSupplier.ItemsSource = suppliers;
            if (suppliers.Count > 0) CboSupplier.SelectedIndex = 0;

            var prodTable = _productController.GetProducts();
            var products = new List<KeyValuePair<int, string>>();
            foreach (DataRow row in prodTable.Rows)
            {
                products.Add(new KeyValuePair<int, string>(
                    Convert.ToInt32(row["ProductID"]),
                    $"{row["ProductName"]} ({row["SKU"]})"));
            }
            CboStockProduct.ItemsSource = products;
            if (products.Count > 0) CboStockProduct.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            ShowStatusMessage("Error", $"Failed to load dropdowns: {ex.Message}");
        }
    }

    private void LoadProducts(string? keyword = null)
    {
        try
        {
            var table = string.IsNullOrWhiteSpace(keyword)
                ? _productController.GetProducts()
                : _productController.SearchProducts(keyword);
            DataGridHelper.Populate(GridProducts, table);
        }
        catch (Exception ex)
        {
            ShowStatusMessage("Error", $"Failed to load products: {ex.Message}");
        }
    }

    private void LoadStockTransactions()
    {
        try
        {
            var table = _stockController.GetAllTransactions();
            DataGridHelper.Populate(GridStockTransactions, table);
        }
        catch (Exception ex)
        {
            ShowStatusMessage("Error", $"Failed to load transactions: {ex.Message}");
        }
    }

    private void LoadSuppliersAndCategories()
    {
        try
        {
            DataGridHelper.Populate(GridCategories, _categoryController.GetCategories());
            DataGridHelper.Populate(GridSuppliers, _supplierController.GetSuppliers());
        }
        catch (Exception ex)
        {
            ShowStatusMessage("Error", $"Failed to load suppliers/categories: {ex.Message}");
        }
    }

    private void LoadUsers()
    {
        try
        {
            DataGridHelper.Populate(GridUsers, _userController.GetUsers());
        }
        catch (Exception ex)
        {
            ShowStatusMessage("Error", $"Failed to load users: {ex.Message}");
        }
    }

    // --- Dashboard Events ---
    private void BtnRefreshDashboard_Click(object? sender, RoutedEventArgs e)
    {
        LoadAllData();
        ShowStatusMessage("Refreshed", "Dashboard data reloaded from database.");
    }

    // --- Products Events ---
    private void BtnSearchProduct_Click(object? sender, RoutedEventArgs e)
    {
        LoadProducts(TxtProductSearch.Text);
    }

    private void BtnResetProductSearch_Click(object? sender, RoutedEventArgs e)
    {
        TxtProductSearch.Text = string.Empty;
        LoadProducts();
    }

    private void GridProducts_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        DataRow? row = (GridProducts.SelectedItem as DataGridRowModel)?.Row;
        if (row != null)
        {
            _selectedProductId = Convert.ToInt32(row["ProductID"]);
            TxtSKU.Text = row["SKU"].ToString();
            TxtProductName.Text = row["ProductName"].ToString();
            TxtUnitPrice.Text = row["UnitPrice"].ToString();
            TxtQuantity.Text = row["Quantity"].ToString();
            TxtReorderLevel.Text = row["ReorderLevel"].ToString();

            int catId = Convert.ToInt32(row["CategoryID"]);
            var categories = CboCategory.ItemsSource as List<KeyValuePair<int, string>>;
            var catItem = categories?.FirstOrDefault(c => c.Key == catId);
            if (catItem.HasValue && catItem.Value.Key == catId)
            {
                CboCategory.SelectedItem = catItem.Value;
            }

            int supId = Convert.ToInt32(row["SupplierID"]);
            var suppliers = CboSupplier.ItemsSource as List<KeyValuePair<int, string>>;
            var supItem = suppliers?.FirstOrDefault(s => s.Key == supId);
            if (supItem.HasValue && supItem.Value.Key == supId)
            {
                CboSupplier.SelectedItem = supItem.Value;
            }
        }
    }

    private void BtnSaveProduct_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var p = ReadProductForm();
            _productController.AddProduct(p, _currentUser.UserID);
            ShowStatusMessage("Product Added", $"Product '{p.ProductName}' added to database.");
            LoadProducts();
            LoadDropdowns();
            LoadDashboard();
            ClearProductForm();
        }
        catch (Exception ex)
        {
            ShowStatusMessage("Save Error", ex.Message);
        }
    }

    private void BtnUpdateProduct_Click(object? sender, RoutedEventArgs e)
    {
        if (_selectedProductId <= 0)
        {
            ShowStatusMessage("Select Required", "Please click a product from the table first.");
            return;
        }

        try
        {
            var p = ReadProductForm();
            p.ProductID = _selectedProductId;
            _productController.EditProduct(p, _currentUser.UserID);
            ShowStatusMessage("Product Updated", $"Product '{p.ProductName}' updated in database.");
            LoadProducts();
            LoadDropdowns();
            LoadDashboard();
            ClearProductForm();
        }
        catch (Exception ex)
        {
            ShowStatusMessage("Update Error", ex.Message);
        }
    }

    private void BtnDeleteProduct_Click(object? sender, RoutedEventArgs e)
    {
        if (_selectedProductId <= 0)
        {
            ShowStatusMessage("Select Required", "Please click a product from the table first.");
            return;
        }

        try
        {
            _productController.RemoveProduct(_selectedProductId, _currentUser.UserID);
            ShowStatusMessage("Product Deleted", "Product record removed from database.");
            LoadProducts();
            LoadDropdowns();
            LoadDashboard();
            ClearProductForm();
        }
        catch (Exception ex)
        {
            ShowStatusMessage("Delete Error", ex.Message);
        }
    }

    private void BtnClearProduct_Click(object? sender, RoutedEventArgs e)
    {
        ClearProductForm();
    }

    private Product ReadProductForm()
    {
        if (string.IsNullOrWhiteSpace(TxtSKU.Text) ||
            string.IsNullOrWhiteSpace(TxtProductName.Text) ||
            string.IsNullOrWhiteSpace(TxtUnitPrice.Text) ||
            string.IsNullOrWhiteSpace(TxtQuantity.Text) ||
            string.IsNullOrWhiteSpace(TxtReorderLevel.Text))
        {
            throw new ArgumentException("Please fill in all product fields.");
        }

        if (!decimal.TryParse(TxtUnitPrice.Text.Trim(), out decimal price) || price < 0)
            throw new ArgumentException("Unit price must be a valid non-negative number.");

        if (!int.TryParse(TxtQuantity.Text.Trim(), out int qty) || qty < 0)
            throw new ArgumentException("Quantity must be a non-negative integer.");

        if (!int.TryParse(TxtReorderLevel.Text.Trim(), out int reorder) || reorder < 0)
            throw new ArgumentException("Reorder level must be a non-negative integer.");

        int catId = 1;
        if (CboCategory.SelectedItem is KeyValuePair<int, string> catPair) catId = catPair.Key;

        int supId = 1;
        if (CboSupplier.SelectedItem is KeyValuePair<int, string> supPair) supId = supPair.Key;

        return new Product
        {
            SKU = TxtSKU.Text.Trim(),
            ProductName = TxtProductName.Text.Trim(),
            CategoryID = catId,
            SupplierID = supId,
            UnitPrice = price,
            Quantity = qty,
            ReorderLevel = reorder
        };
    }

    private void ClearProductForm()
    {
        _selectedProductId = 0;
        TxtSKU.Text = string.Empty;
        TxtProductName.Text = string.Empty;
        TxtUnitPrice.Text = string.Empty;
        TxtQuantity.Text = string.Empty;
        TxtReorderLevel.Text = "10";
    }

    // --- Stock Transactions Events ---
    private void BtnPostStock_Click(object? sender, RoutedEventArgs e)
    {
        if (CboStockProduct.SelectedItem is not KeyValuePair<int, string> productItem)
        {
            ShowStatusMessage("Validation", "Please select a product.");
            return;
        }

        if (!int.TryParse(TxtStockQuantity.Text?.Trim(), out int qty) || qty <= 0)
        {
            ShowStatusMessage("Validation", "Please enter a valid positive quantity.");
            return;
        }

        int pid = productItem.Key;
        bool allowNegative = ChkAdminNegativeOverride.IsChecked == true && _currentUser.Role == "Admin";
        string reason = TxtStockReason.Text?.Trim() ?? "Stock Movement";

        try
        {
            int index = CboTransactionType.SelectedIndex;
            if (index == 0) // STOCK IN
            {
                _stockController.RecordStockIn(pid, qty, reason, _currentUser.UserID);
                ShowStatusMessage("Stock-IN Recorded", $"Added {qty} units to product inventory.");
            }
            else if (index == 1) // STOCK OUT
            {
                _stockController.RecordStockOut(pid, qty, reason, _currentUser.UserID, allowNegative);
                ShowStatusMessage("Stock-OUT Recorded", $"Issued {qty} units from inventory.");
            }
            else // ADJUSTMENT
            {
                _stockController.RecordStockAdjustment(pid, qty, reason, _currentUser.UserID, allowNegative);
                ShowStatusMessage("Adjustment Recorded", $"Adjusted inventory by {qty} units.");
            }

            LoadAllData();
            ClearStockForm();
        }
        catch (Exception ex)
        {
            ShowStatusMessage("Transaction Blocked", ex.Message);
        }
    }

    private void BtnClearStock_Click(object? sender, RoutedEventArgs e)
    {
        ClearStockForm();
    }

    private void ClearStockForm()
    {
        TxtStockQuantity.Text = string.Empty;
        TxtStockReason.Text = string.Empty;
        ChkAdminNegativeOverride.IsChecked = false;
    }

    // --- Reports Events ---
    public void BtnRunReport_Click(object? sender, RoutedEventArgs? e)
    {
        try
        {
            int idx = CboReportType.SelectedIndex;
            _currentReportTable = idx switch
            {
                0 => _reportController.GetCurrentStockReport(),
                1 => _reportController.GetStockMovementReport(),
                2 => _reportController.GetLowStockReport(),
                3 => _reportController.GetSupplierReport(),
                _ => _reportController.GetCurrentStockReport()
            };

            DataGridHelper.Populate(GridReports, _currentReportTable);
            ShowStatusMessage("Report Generated", $"Generated {(_currentReportTable.Rows.Count)} records.");
        }
        catch (Exception ex)
        {
            ShowStatusMessage("Report Error", ex.Message);
        }
    }

    private void BtnExportCsv_Click(object? sender, RoutedEventArgs e)
    {
        if (_currentReportTable == null || _currentReportTable.Rows.Count == 0)
        {
            BtnRunReport_Click(sender, e);
        }

        if (_currentReportTable != null && _currentReportTable.Rows.Count > 0)
        {
            try
            {
                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string path = Path.Combine(desktop, $"Inventory_Report_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
                _reportController.ExportToCsv(_currentReportTable, path, _currentUser.UserID);
                ShowStatusMessage("Exported to CSV", $"Saved to Desktop: {Path.GetFileName(path)}");
            }
            catch (Exception ex)
            {
                ShowStatusMessage("Export Error", ex.Message);
            }
        }
    }

    // --- User Accounts Events (Admin) ---
    private void GridUsers_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        DataRow? row = (GridUsers.SelectedItem as DataGridRowModel)?.Row;
        if (row != null)
        {
            _selectedUserId = Convert.ToInt32(row["UserID"]);
            TxtUserUsername.Text = row["Username"].ToString();
            TxtUserFullName.Text = row["FullName"].ToString();
            ChkUserIsActive.IsChecked = Convert.ToBoolean(row["IsActive"]);

            string role = row["Role"]?.ToString() ?? "Staff";
            foreach (var itemObj in CboUserRole.Items)
            {
                if (itemObj is ComboBoxItem item && item.Content?.ToString() == role)
                {
                    CboUserRole.SelectedItem = item;
                    break;
                }
            }
            TxtUserPassword.Text = string.Empty;
        }
    }

    private void BtnCreateUser_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            string username = TxtUserUsername.Text?.Trim() ?? "";
            string fullName = TxtUserFullName.Text?.Trim() ?? "";
            string role = (CboUserRole.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Staff";
            string password = TxtUserPassword.Text ?? "";

            var newUser = new User
            {
                Username = username,
                FullName = fullName,
                Role = role,
                IsActive = ChkUserIsActive.IsChecked == true
            };

            _userController.AddUser(newUser, password, _currentUser.UserID);
            ShowStatusMessage("User Created", $"User '{username}' ({role}) created in database.");
            LoadUsers();
            ClearUserForm();
        }
        catch (Exception ex)
        {
            ShowStatusMessage("User Create Error", ex.Message);
        }
    }

    private void BtnUpdateUser_Click(object? sender, RoutedEventArgs e)
    {
        if (_selectedUserId <= 0)
        {
            ShowStatusMessage("Select Required", "Click a user from the table first.");
            return;
        }

        try
        {
            string username = TxtUserUsername.Text?.Trim() ?? "";
            string fullName = TxtUserFullName.Text?.Trim() ?? "";
            string role = (CboUserRole.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Staff";
            string? password = string.IsNullOrWhiteSpace(TxtUserPassword.Text) ? null : TxtUserPassword.Text;

            var updatedUser = new User
            {
                UserID = _selectedUserId,
                Username = username,
                FullName = fullName,
                Role = role,
                IsActive = ChkUserIsActive.IsChecked == true
            };

            _userController.EditUser(updatedUser, password, _currentUser.UserID);
            ShowStatusMessage("User Updated", $"User '{username}' updated in database.");
            LoadUsers();
            ClearUserForm();
        }
        catch (Exception ex)
        {
            ShowStatusMessage("User Update Error", ex.Message);
        }
    }

    private void BtnToggleUserActive_Click(object? sender, RoutedEventArgs e)
    {
        if (_selectedUserId <= 0)
        {
            ShowStatusMessage("Select Required", "Click a user from the table first.");
            return;
        }

        try
        {
            bool newStatus = !(ChkUserIsActive.IsChecked == true);
            _userController.ToggleUserStatus(_selectedUserId, newStatus, _currentUser.UserID);
            ShowStatusMessage("Status Changed", $"User ID {_selectedUserId} active status changed to {newStatus}.");
            LoadUsers();
            ClearUserForm();
        }
        catch (Exception ex)
        {
            ShowStatusMessage("Status Error", ex.Message);
        }
    }

    private void BtnDeleteUser_Click(object? sender, RoutedEventArgs e)
    {
        if (_selectedUserId <= 0)
        {
            ShowStatusMessage("Select Required", "Click a user from the table first.");
            return;
        }

        try
        {
            _userController.RemoveUser(_selectedUserId, _currentUser.UserID);
            ShowStatusMessage("User Deleted", $"User ID {_selectedUserId} removed from database.");
            LoadUsers();
            ClearUserForm();
        }
        catch (Exception ex)
        {
            ShowStatusMessage("Delete Error", ex.Message);
        }
    }

    private void BtnClearUser_Click(object? sender, RoutedEventArgs e)
    {
        ClearUserForm();
    }

    private void ClearUserForm()
    {
        _selectedUserId = 0;
        TxtUserUsername.Text = string.Empty;
        TxtUserFullName.Text = string.Empty;
        TxtUserPassword.Text = string.Empty;
        ChkUserIsActive.IsChecked = true;
        CboUserRole.SelectedIndex = 2; // Staff
    }

    // --- Logout & Session Termination ---
    private void BtnLogout_Click(object? sender, RoutedEventArgs e)
    {
        _authController.Logout();

        // Re-open login window and close main window
        var loginWindow = new LoginWindow();
        loginWindow.Show();
        this.Close();
    }

    private void ShowStatusMessage(string category, string message)
    {
        Console.WriteLine($"[{category}] {message}");
        Title = $"Wholesale Inventory Management System - [{category}: {message}]";
    }
}
