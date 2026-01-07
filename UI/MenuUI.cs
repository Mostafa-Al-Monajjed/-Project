using System;
using System.Linq;
using Spectre.Console;
using StoreManagement.Models;
using StoreManagement.Services;
using StoreManagement.Interfaces;
using StoreManagement.Exceptions;
using StoreManagement.Utils;

namespace StoreManagement.UI
{
    public class MenuUI
    {
        private readonly UserService _userService;
        private readonly CategoryService _categoryService;
        private readonly ProductService _productService;
        private readonly OrderService _orderService;
        private readonly InvoiceService _invoiceService;
        
        public MenuUI()
        {
            var userRepo = new FileRepository<User>("Storage/users.json");
            var categoryRepo = new FileRepository<Category>("Storage/categories.json");
            var productRepo = new FileRepository<Product>("Storage/products.json");
            var orderRepo = new FileRepository<Order>("Storage/orders.json");
            var invoiceRepo = new FileRepository<Invoice>("Storage/invoices.json");
            
            _userService = new UserService(userRepo);
            _categoryService = new CategoryService(categoryRepo);
            _productService = new ProductService(productRepo, _categoryService);
            _orderService = new OrderService(orderRepo, _productService, _userService);
            _invoiceService = new InvoiceService(invoiceRepo, _orderService);
            
            _userService.InitializeAdminUser();
        }

        public void Run()
        {

            AnsiConsole.Clear();
            AnsiConsole.Write(
                new FigletText("Store Managment System")
                    .LeftJustified()
                    .Color(Color.Blue));
            
            Thread.Sleep(1000);
            
            LoginMenu();
            
            while (_userService.IsLoggedIn)
            {
                MainMenu();
            }
        }

        private void LoginMenu()
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule("Login").LeftJustified());

            while (!_userService.IsLoggedIn)
            {
                var username = AnsiConsole.Ask<string>("Username:");
                var password = AnsiConsole.Prompt(
                    new TextPrompt<string>("Password:")
                        .Secret());

                try
                {
                    _userService.Login(username, password);
                    AnsiConsole.MarkupLine("[green]Login successful![/]");
                    Thread.Sleep(800);
                }
                catch (LoginFailedException ex)
                {
                    AnsiConsole.MarkupLine($"[red]{ex.Message}[/]");
                    if (!AnsiConsole.Confirm("Try again?"))
                    {
                        Environment.Exit(0);
                    }
                }
            }
        }

        private void MainMenu()
        {
            AnsiConsole.Clear();
            var user = _userService.CurrentUser;
            
            AnsiConsole.MarkupLine($"[yellow]Welcome, {user.FullName} ({user.Role})[/]");
            
            var menu = new SelectionPrompt<string>()
                .Title("[blue]Main Menu:[/]")
                .PageSize(10)
                .AddChoices(new[] {
                    "Products Management",
                    "Categories Management",
                    "Point of Sale",
                    "Orders Management",
                    "Reports",
                    "Users Management (Admin Only)",
                    "Logout",
                    "Exit"
                });

            var choice = AnsiConsole.Prompt(menu);

            switch (choice)
            {
                case "Products Management":
                    ProductsMenu();
                    break;
                case "Categories Management":
                    CategoriesMenu();
                    break;
                case "POS - Point of Sale":
                    POSMenu();
                    break;
                case "Orders Management":
                    OrdersMenu();
                    break;
                case "Reports":
                    ReportsMenu();
                    break;
                case "Users Management (Admin Only)":
                    if (user.Role == UserRole.Admin)
                        UsersMenu();
                    else
                        AnsiConsole.MarkupLine("[red]Access denied![/]");
                    break;
                case "Logout":
                    _userService.Logout();
                    LoginMenu();
                    return;
                case "Exit":
                    Environment.Exit(0);
                    break;
            }
        }

        private void ProductsMenu()
        {
            var menu = new SelectionPrompt<string>()
                .Title("[blue]Products Management:[/]")
                .PageSize(10)
                .AddChoices(new[] {
                    "List All Products",
                    "Add New Product",
                    "Update Product",
                    "Delete Product",
                    "Update Stock",
                    "Back"
                });

            while (true)
            {
                AnsiConsole.Clear();
                var choice = AnsiConsole.Prompt(menu);

                switch (choice)
                {
                    case "List All Products":
                        ListProducts();
                        break;
                    case "Add New Product":
                        AddProduct();
                        break;
                    case "Update Product":
                        UpdateProduct();
                        break;
                    case "Delete Product":
                        DeleteProduct();
                        break;
                    case "Update Stock":
                        UpdateStock();
                        break;
                    case "Back":
                        return;
                }
            }
        }

        private void ListProducts()
        {
            var products = _productService.GetAllProducts();
            
            var table = new Table();
            table.Border(TableBorder.Rounded);
            table.AddColumn("ID");
            table.AddColumn("Name");
            table.AddColumn("Category");
            table.AddColumn("Price");
            table.AddColumn("Stock");
            
            foreach (var product in products)
            {
                var category = _categoryService.GetAllCategories()
                    .FirstOrDefault(c => c.Id == product.CategoryId)?.Name ?? "Unknown";
                
                table.AddRow(
                    product.Id.Substring(0, 8),
                    product.Name,
                    category,
                    product.Price.ToString("C"),
                    product.QuantityInStock.ToString()
                );
            }
            
            AnsiConsole.Write(table);
            AnsiConsole.Write("Enter...");
            AnsiConsole.Console.Input.ReadKey(true);
        }

        private void AddProduct()
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule("Add New Product").LeftJustified());
            
            var categories = _categoryService.GetAllCategories();
            if (categories.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No categories available. Please add a category first.[/]");
                Thread.Sleep(1500);
                return;
            }

            var product = new Product();
            
            product.Name = AnsiConsole.Ask<string>("Product Name:");
            product.Description = AnsiConsole.Ask<string>("Description:");
            
            var categoryChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select Category:")
                    .AddChoices(categories.Select(c => c.Name).ToArray()));
            
            product.CategoryId = categories.First(c => c.Name == categoryChoice).Id;
            product.Price = AnsiConsole.Ask<decimal>("Price:");
            product.QuantityInStock = AnsiConsole.Ask<int>("Initial Stock Quantity:");
            
            try
            {
                _productService.AddProduct(product);
                AnsiConsole.MarkupLine("[green]Product added successfully![/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            }
            
            Thread.Sleep(800);
        }

        private void UpdateProduct()
        {
            var products = _productService.GetAllProducts();
            if (products.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No products available.[/]");
                Thread.Sleep(800);
                return;
            }

            var productChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select Product to Update:")
                    .AddChoices(products.Select(p => $"{p.Name} ({p.Id.Substring(0, 8)})").ToArray()));
            
            var productId = products.First(p => $"{p.Name} ({p.Id.Substring(0, 8)})" == productChoice).Id;
            var product = _productService.GetProductById(productId);
            
            product.Name = AnsiConsole.Prompt(
                new TextPrompt<string>("Product Name:")
                    .DefaultValue(product.Name));
            
            product.Description = AnsiConsole.Prompt(
                new TextPrompt<string>("Description:")
                    .DefaultValue(product.Description));
            
            product.Price = AnsiConsole.Prompt(
                new TextPrompt<decimal>("Price:")
                    .DefaultValue(product.Price));
            
            try
            {
                _productService.UpdateProduct(product);
                AnsiConsole.MarkupLine("[green]Product updated successfully![/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            }
            
            Thread.Sleep(800);
        }

        private void DeleteProduct()
        {
            var products = _productService.GetAllProducts();
            if (products.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No products available.[/]");
                Thread.Sleep(800);
                return;
            }

            var productChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select Product to Delete:")
                    .AddChoices(products.Select(p => $"{p.Name} ({p.Id.Substring(0, 8)})").ToArray()));
            
            var productId = products.First(p => $"{p.Name} ({p.Id.Substring(0, 8)})" == productChoice).Id;
            
            if (AnsiConsole.Confirm("Are you sure you want to delete this product?"))
            {
                _productService.DeleteProduct(productId);
                AnsiConsole.MarkupLine("[green]Product deleted successfully![/]");
            }
            
            Thread.Sleep(800);
        }

        private void UpdateStock()
        {
            var products = _productService.GetAllProducts();
            if (products.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No products available.[/]");
                Thread.Sleep(800);
                return;
            }

            var productChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select Product:")
                    .AddChoices(products.Select(p => $"{p.Name} (Stock: {p.QuantityInStock})").ToArray()));
            
            var product = products.First(p => $"{p.Name} (Stock: {p.QuantityInStock})" == productChoice);
            var quantity = AnsiConsole.Ask<int>($"Enter quantity to add (negative to remove):");
            
            try
            {
                _productService.UpdateStock(product.Id, quantity);
                AnsiConsole.MarkupLine("[green]Stock updated successfully![/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            }
            
            Thread.Sleep(800);
        }

        private void CategoriesMenu()
        {
            var menu = new SelectionPrompt<string>()
                .Title("[blue]Categories Management:[/]")
                .PageSize(10)
                .AddChoices(new[] {
                    "List All Categories",
                    "Add New Category",
                    "Update Category",
                    "Delete Category",
                    "Back"
                });

            while (true)
            {
                AnsiConsole.Clear();
                var choice = AnsiConsole.Prompt(menu);

                switch (choice)
                {
                    case "List All Categories":
                        ListCategories();
                        break;
                    case "Add New Category":
                        AddCategory();
                        break;
                    case "Update Category":
                        UpdateCategory();
                        break;
                    case "Delete Category":
                        DeleteCategory();
                        break;
                    case "Back":
                        return;
                }
            }
        }

        private void ListCategories()
        {
            var categories = _categoryService.GetAllCategories();
            
            var table = new Table();
            table.Border(TableBorder.Rounded);
            table.AddColumn("ID");
            table.AddColumn("Name");
            table.AddColumn("Description");
            table.AddColumn("Created At");
            
            foreach (var category in categories)
            {
                table.AddRow(
                    category.Id.Substring(0, 8),
                    category.Name,
                    category.Description,
                    category.CreatedAt.ToString("yyyy-MM-dd")
                );
            }
            
            AnsiConsole.Write(table);
            AnsiConsole.Write("Enter...");
            AnsiConsole.Console.Input.ReadKey(true);
        }

        private void AddCategory()
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule("Add New Category").LeftJustified());
            
            var category = new Category();
            
            category.Name = AnsiConsole.Ask<string>("Category Name:");
            category.Description = AnsiConsole.Ask<string>("Description:");
            
            try
            {
                _categoryService.AddCategory(category);
                AnsiConsole.MarkupLine("[green]Category added successfully![/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            }
            
            Thread.Sleep(800);
        }

        private void UpdateCategory()
        {
            var categories = _categoryService.GetAllCategories();
            if (categories.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No categories available.[/]");
                Thread.Sleep(800);
                return;
            }

            var categoryChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select Category to Update:")
                    .AddChoices(categories.Select(c => c.Name).ToArray()));
            
            var category = categories.First(c => c.Name == categoryChoice);
            
            category.Name = AnsiConsole.Prompt(
                new TextPrompt<string>("Category Name:")
                    .DefaultValue(category.Name));
            
            category.Description = AnsiConsole.Prompt(
                new TextPrompt<string>("Description:")
                    .DefaultValue(category.Description));
            
            try
            {
                _categoryService.UpdateCategory(category);
                AnsiConsole.MarkupLine("[green]Category updated successfully![/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            }
            
            Thread.Sleep(800);
        }

        private void DeleteCategory()
        {
            var categories = _categoryService.GetAllCategories();
            if (categories.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No categories available.[/]");
                Thread.Sleep(800);
                return;
            }

            var categoryChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select Category to Delete:")
                    .AddChoices(categories.Select(c => c.Name).ToArray()));
            
            var category = categories.First(c => c.Name == categoryChoice);
            
            var products = _productService.GetProductsByCategory(category.Id);
            if (products.Count > 0)
            {
                AnsiConsole.MarkupLine($"[red]Cannot delete category with {products.Count} associated products.[/]");
                Thread.Sleep(800);
                return;
            }
            
            if (AnsiConsole.Confirm("Are you sure you want to delete this category?"))
            {
                _categoryService.DeleteCategory(category.Id);
                AnsiConsole.MarkupLine("[green]Category deleted successfully![/]");
            }
            
            Thread.Sleep(800);
        }

        private void POSMenu()
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule("Point of Sale").LeftJustified());
            
            while (true)
            {
                AnsiConsole.MarkupLine("[yellow]Current Cart:[/]");
                var cartItems = _orderService.GetCartItems();
                
                if (cartItems.Count == 0)
                {
                    AnsiConsole.MarkupLine("[grey]Cart is empty[/]");
                }
                else
                {
                    var table = new Table();
                    table.Border(TableBorder.Rounded);
                    table.AddColumn("Product");
                    table.AddColumn("Qty");
                    table.AddColumn("Unit Price");
                    table.AddColumn("Total");
                    
                    foreach (var item in cartItems)
                    {
                        table.AddRow(
                            item.ProductName,
                            item.Quantity.ToString(),
                            item.UnitPrice.ToString("C"),
                            item.TotalPrice.ToString("C")
                        );
                    }
                    
                    AnsiConsole.Write(table);
                    AnsiConsole.MarkupLine($"[bold]Cart Total: {_orderService.CalculateTotal():C}[/]");
                }
                
                var menu = new SelectionPrompt<string>()
                    .Title("POS Menu:")
                    .AddChoices(new[] {
                        "Add Product to Cart",
                        "Remove from Cart",
                        "Update Quantity",
                        "Clear Cart",
                        "Checkout",
                        "Back to Main Menu"
                    });
                
                var choice = AnsiConsole.Prompt(menu);
                
                switch (choice)
                {
                    case "Add Product to Cart":
                        AddToCart();
                        break;
                    case "Remove from Cart":
                        RemoveFromCart();
                        break;
                    case "Update Quantity":
                        UpdateCartQuantity();
                        break;
                    case "Clear Cart":
                        _orderService.ClearCart();
                        AnsiConsole.MarkupLine("[green]Cart cleared![/]");
                        Thread.Sleep(800);
                        break;
                    case "Checkout":
                        Checkout();
                        break;
                    case "Back to Main Menu":
                        return;
                }
                
                AnsiConsole.Clear();
                AnsiConsole.Write(new Rule("Point of Sale").LeftJustified());
            }
        }

        private void AddToCart()
        {
            var products = _productService.GetAllProducts().Where(p => p.QuantityInStock > 0).ToList();
            if (products.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No products in stock.[/]");
                Thread.Sleep(800);
                return;
            }

            var productChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select Product:")
                    .AddChoices(products.Select(p => $"{p.Name} (Stock: {p.QuantityInStock}, Price: {p.Price:C})").ToArray()));
            
            var product = products.First(p => $"{p.Name} (Stock: {p.QuantityInStock}, Price: {p.Price:C})" == productChoice);
            var quantity = AnsiConsole.Ask<int>("Enter quantity:");
            
            try
            {
                _orderService.AddItem(product, quantity);
                AnsiConsole.MarkupLine($"[green]Added {quantity} x {product.Name} to cart[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            }
            
            Thread.Sleep(800);
        }

        private void RemoveFromCart()
        {
            var cartItems = _orderService.GetCartItems();
            if (cartItems.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]Cart is empty[/]");
                Thread.Sleep(800);
                return;
            }

            var itemChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select Item to Remove:")
                    .AddChoices(cartItems.Select(i => i.ProductName).ToArray()));
            
            var item = cartItems.First(i => i.ProductName == itemChoice);
            _orderService.RemoveItem(item.ProductId);
            AnsiConsole.MarkupLine("[green]Item removed from cart[/]");
            Thread.Sleep(800);
        }

        private void UpdateCartQuantity()
        {
            var cartItems = _orderService.GetCartItems();
            if (cartItems.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]Cart is empty[/]");
                Thread.Sleep(800);
                return;
            }

            var itemChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select Item:")
                    .AddChoices(cartItems.Select(i => i.ProductName).ToArray()));
            
            var item = cartItems.First(i => i.ProductName == itemChoice);
            var quantity = AnsiConsole.Ask<int>($"Enter new quantity for {item.ProductName}:");
            
            _orderService.UpdateQuantity(item.ProductId, quantity);
            AnsiConsole.MarkupLine("[green]Quantity updated[/]");
            Thread.Sleep(800);
        }

        private void Checkout()
        {
            if (_orderService.IsCartEmpty())
            {
                AnsiConsole.MarkupLine("[red]Cart is empty[/]");
                Thread.Sleep(800);
                return;
            }

            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule("Checkout").LeftJustified());
            
            var customerName = AnsiConsole.Ask<string>("Customer Name:");
            var phone = AnsiConsole.Ask<string>("Phone Number:");
            
            var paymentMethod = AnsiConsole.Prompt(
                new SelectionPrompt<PaymentMethod>()
                    .Title("Payment Method:")
                    .AddChoices(PaymentMethod.Cash, PaymentMethod.CreditCard));
            
            try
            {
                var order = _orderService.Checkout(customerName, phone, paymentMethod);
                var invoice = _invoiceService.GenerateInvoice(order);
                
                AnsiConsole.Clear();
                AnsiConsole.Write(new Panel(invoice.Content)
                    .Header("[green]Invoice Generated[/]")
                    .BorderColor(Color.Green));
                
                if (AnsiConsole.Confirm("Print invoice?"))
                {
                    Console.WriteLine("\n" + invoice.Content);
                }
                
                AnsiConsole.MarkupLine("[green]Order completed successfully![/]");
                    
                AnsiConsole.Write("Enter...");
                AnsiConsole.Console.Input.ReadKey(true);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
                AnsiConsole.Write("Enter...");
                AnsiConsole.Console.Input.ReadKey(true);
            }
        }

        private void OrdersMenu()
        {
            var menu = new SelectionPrompt<string>()
                .Title("[blue]Orders Management:[/]")
                .PageSize(10)
                .AddChoices(new[] {
                    "List All Orders",
                    "Search Orders by Customer",
                    "View Orders by Date Range",
                    "Back"
                });

            while (true)
            {
                AnsiConsole.Clear();
                var choice = AnsiConsole.Prompt(menu);

                switch (choice)
                {
                    case "List All Orders":
                        ListOrders();
                        break;
                    case "Search Orders by Customer":
                        SearchOrdersByCustomer();
                        break;
                    case "View Orders by Date Range":
                        ViewOrdersByDateRange();
                        break;
                    case "Back":
                        return;
                }
            }
        }

        private void ListOrders()
        {
            var orders = _orderService.GetAllOrders();
            
            var table = new Table();
            table.Border(TableBorder.Rounded);
            table.AddColumn("Order ID");
            table.AddColumn("Customer");
            table.AddColumn("Date");
            table.AddColumn("Items");
            table.AddColumn("Total");
            table.AddColumn("Status");
            
            foreach (var order in orders)
            {
                table.AddRow(
                    order.Id,
                    order.CustomerName,
                    order.OrderDate.ToString("yyyy-MM-dd"),
                    order.Items.Count.ToString(),
                    order.TotalAmount.ToString("C"),
                    order.Status.ToString()
                );
            }
            
            AnsiConsole.Write(table);
            AnsiConsole.Write("Enter...");
            AnsiConsole.Console.Input.ReadKey(true);
        }

        private void SearchOrdersByCustomer()
        {
            var customerName = AnsiConsole.Ask<string>("Enter customer name:");
            var orders = _orderService.GetOrdersByCustomer(customerName);
            
            if (orders.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No orders found for this customer.[/]");
            }
            else
            {
                var table = new Table();
                table.Border(TableBorder.Rounded);
                table.AddColumn("Order ID");
                table.AddColumn("Date");
                table.AddColumn("Items");
                table.AddColumn("Total");
                
                foreach (var order in orders)
                {
                    table.AddRow(
                        order.Id,
                        order.OrderDate.ToString("yyyy-MM-dd"),
                        order.Items.Count.ToString(),
                        order.TotalAmount.ToString("C")
                    );
                }
                
                AnsiConsole.Write(table);
            }
            
            AnsiConsole.Write("Enter...");
            AnsiConsole.Console.Input.ReadKey(true);
        }

        private void ViewOrdersByDateRange()
        {
            var startDate = AnsiConsole.Prompt(
                new TextPrompt<DateTime>("Start Date (yyyy-MM-dd):")
                    .DefaultValue(DateTime.Now.AddMonths(-1)));
            
            var endDate = AnsiConsole.Prompt(
                new TextPrompt<DateTime>("End Date (yyyy-MM-dd):")
                    .DefaultValue(DateTime.Now));
            
            var orders = _orderService.GetOrdersByDateRange(startDate, endDate);
            
            var table = new Table();
            table.Border(TableBorder.Rounded);
            table.AddColumn("Order ID");
            table.AddColumn("Customer");
            table.AddColumn("Date");
            table.AddColumn("Total");
            
            foreach (var order in orders)
            {
                table.AddRow(
                    order.Id,
                    order.CustomerName,
                    order.OrderDate.ToString("yyyy-MM-dd"),
                    order.TotalAmount.ToString("C")
                );
            }
            
            AnsiConsole.Write(table);
            AnsiConsole.MarkupLine($"[bold]Total Orders: {orders.Count}[/]");
            AnsiConsole.MarkupLine($"[bold]Total Revenue: {orders.Sum(o => o.TotalAmount):C}[/]");
            
            AnsiConsole.Write("Enter...");
            AnsiConsole.Console.Input.ReadKey(true);
        }

        private void ReportsMenu()
        {
            var menu = new SelectionPrompt<string>()
                .Title("[blue]Reports:[/]")
                .PageSize(10)
                .AddChoices(new[] {
                    "Monthly Sales Report",
                    "Customer Invoices",
                    "Monthly Profit",
                    "Back"
                });

            while (true)
            {
                AnsiConsole.Clear();
                var choice = AnsiConsole.Prompt(menu);

                switch (choice)
                {
                    case "Monthly Sales Report":
                        GenerateMonthlyReport();
                        break;
                    case "Customer Invoices":
                        ViewCustomerInvoices();
                        break;
                    case "Monthly Profit":
                        ShowMonthlyProfit();
                        break;
                    case "Back":
                        return;
                }
            }
        }

        private void GenerateMonthlyReport()
        {
            var year = AnsiConsole.Ask<int>("Enter year:", DateTime.Now.Year);
            var month = AnsiConsole.Ask<int>("Enter month (1-12):", DateTime.Now.Month);
            
            var report = _invoiceService.GenerateMonthlyReport(year, month);
            
            AnsiConsole.Clear();
            AnsiConsole.Write(new Panel(report)
                .Header($"[blue]Monthly Report - {month}/{year}[/]")
                .BorderColor(Color.Blue));
            
            if (AnsiConsole.Confirm("Print report?"))
            {
                Console.WriteLine("\n" + report);
            }
            
            AnsiConsole.Write("Enter...");
            AnsiConsole.Console.Input.ReadKey(true);
        }

        private void ViewCustomerInvoices()
        {
            var customerName = AnsiConsole.Ask<string>("Enter customer name:");
            var invoices = _invoiceService.GetInvoicesByCustomer(customerName);
            
            if (invoices.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No invoices found for this customer.[/]");
            }
            else
            {
                var panel = new Panel(invoices.First().Content)
                    .Header($"[blue]Invoice for {customerName}[/]")
                    .BorderColor(Color.Blue);
                
                AnsiConsole.Write(panel);
            }
            
            
            AnsiConsole.Write("Enter...");
            AnsiConsole.Console.Input.ReadKey(true);
        }

        private void ShowMonthlyProfit()
        {
            var year = AnsiConsole.Ask<int>("Enter year:", DateTime.Now.Year);
            var month = AnsiConsole.Ask<int>("Enter month (1-12):", DateTime.Now.Month);
            
            var profit = _orderService.CalculateMonthlyProfit(year, month);
            var orders = _orderService.GetOrdersByMonth(year, month)
                .Where(o => o.Status == OrderStatus.Completed)
                .ToList();
            
            AnsiConsole.Write(new Rule($"Profit Report - {month}/{year}").LeftJustified());
            AnsiConsole.MarkupLine($"[bold]Total Orders: {orders.Count}[/]");
            AnsiConsole.MarkupLine($"[bold]Total Profit: {profit:C}[/]");
            
            AnsiConsole.Write("Enter...");
            AnsiConsole.Console.Input.ReadKey(true);
        }

        private void UsersMenu()
        {
            var menu = new SelectionPrompt<string>()
                .Title("[blue]Users Management:[/]")
                .PageSize(10)
                .AddChoices(new[] {
                    "List All Users",
                    "Add New User",
                    "Update User",
                    "Delete User",
                    "Back"
                });

            while (true)
            {
                AnsiConsole.Clear();
                var choice = AnsiConsole.Prompt(menu);

                switch (choice)
                {
                    case "List All Users":
                        ListUsers();
                        break;
                    case "Add New User":
                        AddUser();
                        break;
                    case "Update User":
                        UpdateUser();
                        break;
                    case "Delete User":
                        DeleteUser();
                        break;
                    case "Back":
                        return;
                }
            }
        }

        private void ListUsers()
        {
            var users = _userService.GetAllUsers(_userService.CurrentUser);
            
            var table = new Table();
            table.Border(TableBorder.Rounded);
            table.AddColumn("ID");
            table.AddColumn("Username");
            table.AddColumn("Full Name");
            table.AddColumn("Role");
            table.AddColumn("Created");
            table.AddColumn("Active");
            
            foreach (var user in users)
            {
                table.AddRow(
                    user.Id.Substring(0, 8),
                    user.Username,
                    user.FullName,
                    user.Role.ToString(),
                    user.CreatedAt.ToString("yyyy-MM-dd"),
                    user.IsActive ? "✅" : "❌"
                );
            }
            
            AnsiConsole.Write(table);
            AnsiConsole.Write("Enter...");
            AnsiConsole.Console.Input.ReadKey(true);
        }

        private void AddUser()
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule("Add New User").LeftJustified());
            
            var user = new User();
            
            user.Username = AnsiConsole.Ask<string>("Username:");
            user.Password = AnsiConsole.Prompt(
                new TextPrompt<string>("Password:")
                    .Secret());
            user.FullName = AnsiConsole.Ask<string>("Full Name:");
            
            var roleChoice = AnsiConsole.Prompt(
                new SelectionPrompt<UserRole>()
                    .Title("Role:")
                    .AddChoices(UserRole.Admin, UserRole.Employee));
            
            user.Role = roleChoice;
            
            try
            {
                _userService.CreateUser(user, _userService.CurrentUser);
                AnsiConsole.MarkupLine("[green]User created successfully![/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            }
            
            Thread.Sleep(1500);
        }

        private void UpdateUser()
        {
            var users = _userService.GetAllUsers(_userService.CurrentUser);
            if (users.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No users available.[/]");
                Thread.Sleep(1500);
                return;
            }

            var userChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select User to Update:")
                    .AddChoices(users.Select(u => $"{u.Username} ({u.Role})").ToArray()));
            
            var user = users.First(u => $"{u.Username} ({u.Role})" == userChoice);
            
            user.FullName = AnsiConsole.Prompt(
                new TextPrompt<string>("Full Name:")
                    .DefaultValue(user.FullName));
            
            user.IsActive = AnsiConsole.Confirm("Is active?", user.IsActive);
            
            try
            {
                _userService.UpdateUser(user, _userService.CurrentUser);
                AnsiConsole.MarkupLine("[green]User updated successfully![/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            }
            
            Thread.Sleep(1500);
        }

        private void DeleteUser()
        {
            var users = _userService.GetAllUsers(_userService.CurrentUser);
            if (users.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No users available.[/]");
                Thread.Sleep(1500);
                return;
            }

            var userChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select User to Delete:")
                    .AddChoices(users.Select(u => $"{u.Username} ({u.Role})").ToArray()));
            
            var user = users.First(u => $"{u.Username} ({u.Role})" == userChoice);
            
            if (AnsiConsole.Confirm($"Are you sure you want to delete user {user.Username}?"))
            {
                try
                {
                    _userService.DeleteUser(user.Id, _userService.CurrentUser);
                    AnsiConsole.MarkupLine("[green]User deleted successfully![/]");
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
                }
            }
            
            Thread.Sleep(1500);
        }
    }
}
