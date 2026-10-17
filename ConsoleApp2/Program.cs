using System;
using System.Collections.Generic;
using System.Linq;

namespace StoreInventory
{
    public enum Category
    {
        Электроника,
        Одежда,
        Продукты,
        Бытовая_химия
    }

    public class Product
    {
        private static int _nextId = 1;
        public int Id { get; private set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public Category Category { get; set; }
        public bool IsInStock => Quantity > 0;

        public Product(string name, decimal price, int quantity, Category category)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Название не может быть пустым.");
            if (price < 0)
                throw new ArgumentException("Цена не может быть отрицательной.");
            if (quantity < 0)
                throw new ArgumentException("Количество не может быть отрицательным.");

            Id = _nextId++;
            Name = name;
            Price = price;
            Quantity = quantity;
            Category = category;
        }
    }

    public class SaleRecord
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int QuantitySold { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime SaleDate { get; set; }
    }

    public class Store
    {
        private List<Product> _products = new List<Product>();
        private Stack<SaleRecord> _salesHistory = new Stack<SaleRecord>();
        private List<SaleRecord> _allSales = new List<SaleRecord>();

        public Store()
        {
            InitializeTestData();
        }

        private void InitializeTestData()
        {
            _products.Add(new Product("Телефон", 85000.00m, 12, Category.Электроника));
            _products.Add(new Product("Футболка", 1200.50m, 45, Category.Одежда));
            _products.Add(new Product("Хлеб", 65.00m, 150, Category.Продукты));
            _products.Add(new Product("Стиральный порошок", 350.00m, 0, Category.Бытовая_химия));
            _products.Add(new Product("Наушники", 5500.00m, 20, Category.Электроника));
        }

        public void AddProduct(string name, decimal price, int quantity, Category category)
        {
            _products.Add(new Product(name, price, quantity, category));
            Console.WriteLine("Товар успешно добавлен.");
        }

        public void DeleteProduct(int id)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product != null)
            {
                _products.Remove(product);
                Console.WriteLine($"Товар '{product.Name}' (ID: {id}) удалён.");
            }
            else
            {
                Console.WriteLine("Товар с таким ID не найден.");
            }
        }

        public void SupplyProduct(int id, int amount)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product != null)
            {
                product.Quantity += amount;
                Console.WriteLine($"Поставка успешна. Остаток '{product.Name}': {product.Quantity} шт.");
            }
            else
            {
                Console.WriteLine("Товар с таким ID не найден.");
            }
        }

        public void SellProduct(int id, int amount)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                Console.WriteLine("Товар с таким ID не найден.");
                return;
            }
            if (!product.IsInStock)
            {
                Console.WriteLine("Ошибка: товара нет на складе.");
                return;
            }
            if (product.Quantity < amount)
            {
                Console.WriteLine($"Ошибка: на складе только {product.Quantity} шт.");
                return;
            }

            product.Quantity -= amount;
            decimal totalPrice = product.Price * amount;

            var sale = new SaleRecord
            {
                ProductId = product.Id,
                ProductName = product.Name,
                QuantitySold = amount,
                TotalPrice = totalPrice,
                SaleDate = DateTime.Now
            };

            _salesHistory.Push(sale);
            _allSales.Add(sale);
            Console.WriteLine($"Продано: {amount} шт. '{product.Name}' на сумму {totalPrice:F2} руб.");
        }

        public void UndoLastSale()
        {
            if (_salesHistory.Count == 0)
            {
                Console.WriteLine("История продаж пуста.");
                return;
            }

            var lastSale = _salesHistory.Pop();
            var product = _products.FirstOrDefault(p => p.Id == lastSale.ProductId);

            if (product != null)
            {
                product.Quantity += lastSale.QuantitySold;
                Console.WriteLine($"Отменена продажа '{lastSale.ProductName}'. Возвращено: {lastSale.QuantitySold} шт.");
            }
            else
            {
                Console.WriteLine($"Продажа отменена, но товар '{lastSale.ProductName}' удалён из базы.");
            }
        }

        public void PrintSalesReport()
        {
            if (_allSales.Count == 0)
            {
                Console.WriteLine("Продаж ещё не было.");
                return;
            }

            Console.WriteLine("\nОТЧЁТ О ПРОДАЖАХ");
            var groupedSales = _allSales.GroupBy(s => s.ProductName)
                .Select(g => new
                {
                    Name = g.Key,
                    TotalQty = g.Sum(s => s.QuantitySold),
                    TotalSum = g.Sum(s => s.TotalPrice)
                });

            decimal grandTotal = 0;
            foreach (var item in groupedSales)
            {
                Console.WriteLine($"Товар: {item.Name}");
                Console.WriteLine($"  Продано штук: {item.TotalQty}");
                Console.WriteLine($"  Общая сумма:  {item.TotalSum:F2} руб.\n");
                grandTotal += item.TotalSum;
            }
            Console.WriteLine($"ИТОГОВАЯ ВЫРУЧКА: {grandTotal:F2} руб.");
        }

        public void SearchProducts()
        {
            Console.WriteLine("Поиск: 1 - по коду, 2 - по названию, 3 - по категории");
            Console.Write("Выберите тип поиска: ");
            string choice = Console.ReadLine();
            List<Product> found = new List<Product>();

            switch (choice)
            {
                case "1":
                    Console.Write("Введите ID товара: ");
                    string idInput = Console.ReadLine();
                    if (int.TryParse(idInput, out int searchId))
                    {
                        found = _products.Where(p => p.Id == searchId).ToList();
                    }
                    else
                    {
                        Console.WriteLine("Ошибка: введено некорректное значение ID.");
                        return;
                    }
                    break;
                case "2":
                    Console.Write("Введите название: ");
                    string searchName = Console.ReadLine()?.ToLower() ?? "";
                    found = _products.Where(p => p.Name.ToLower().Contains(searchName)).ToList();
                    break;
                case "3":
                    Console.WriteLine("Выберите категорию:");
                    string[] categories = Enum.GetNames(typeof(Category));
                    for (int i = 0; i < categories.Length; i++)
                        Console.WriteLine($"{i + 1}. {categories[i]}");

                    string catInput = Console.ReadLine();
                    if (int.TryParse(catInput, out int catIndex) && catIndex >= 1 && catIndex <= categories.Length)
                    {
                        Category selectedCat = (Category)Enum.Parse(typeof(Category), categories[catIndex - 1]);
                        found = _products.Where(p => p.Category == selectedCat).ToList();
                    }
                    else
                    {
                        Console.WriteLine("Ошибка: введено некорректное значение категории.");
                        return;
                    }
                    break;
                default:
                    Console.WriteLine("Неверный выбор.");
                    return;
            }
            PrintProductList(found, "Результаты поиска");
        }

        public void PrintAllProducts()
        {
            PrintProductList(_products, "Текущий ассортимент");
        }

        private void PrintProductList(List<Product> list, string title)
        {
            Console.WriteLine($"\n{title}");
            if (list.Count == 0)
            {
                Console.WriteLine("Ничего не найдено.");
                return;
            }
            foreach (var p in list)
            {
                Console.WriteLine($"[ID: {p.Id}] {p.Name} | Цена: {p.Price:F2} руб. | Кол-во: {p.Quantity} | В наличии: {(p.IsInStock ? "Да" : "Нет")} | Категория: {p.Category}");
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Store store = new Store();
            bool isRunning = true;

            while (isRunning)
            {
                Console.Clear();
                Console.WriteLine("СИСТЕМА УЧЁТА ТОВАРОВ В МАГАЗИНЕ");
                Console.WriteLine("1. Показать все товары");
                Console.WriteLine("2. Добавить товар");
                Console.WriteLine("3. Удалить товар");
                Console.WriteLine("4. Заказать поставку");
                Console.WriteLine("5. Продать товар");
                Console.WriteLine("6. Поиск товаров");
                Console.WriteLine("7. Отменить последнюю продажу");
                Console.WriteLine("8. Отчёт о продажах");
                Console.WriteLine("0. Выход");
                Console.Write("Выберите пункт меню: ");
                string choice = Console.ReadLine();

                try
                {
                    switch (choice)
                    {
                        case "1":
                            store.PrintAllProducts();
                            WaitForKey();
                            break;
                        case "2":
                            AddProductUI(store);
                            break;
                        case "3":
                            DeleteProductUI(store);
                            break;
                        case "4":
                            SupplyProductUI(store);
                            break;
                        case "5":
                            SellProductUI(store);
                            break;
                        case "6":
                            store.SearchProducts();
                            WaitForKey();
                            break;
                        case "7":
                            store.UndoLastSale();
                            WaitForKey();
                            break;
                        case "8":
                            store.PrintSalesReport();
                            WaitForKey();
                            break;
                        case "0":
                            isRunning = false;
                            Console.WriteLine("До свидания.");
                            break;
                        default:
                            Console.WriteLine("Введён неверный пункт меню.");
                            WaitForKey();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                    WaitForKey();
                }
            }
        }

        static void AddProductUI(Store store)
        {
            Console.Write("Введите название товара: ");
            string name = Console.ReadLine();
            decimal price = GetValidDecimal("Введите цену: ");
            int quantity = GetValidInt("Введите количество: ");

            Console.WriteLine("Выберите категорию:");
            string[] categories = Enum.GetNames(typeof(Category));
            for (int i = 0; i < categories.Length; i++)
                Console.WriteLine($"{i + 1}. {categories[i]}");

            int catIndex = GetValidInt("Введите номер категории: ", 1, categories.Length);
            Category category = (Category)Enum.Parse(typeof(Category), categories[catIndex - 1]);
            store.AddProduct(name, price, quantity, category);
            WaitForKey();
        }

        static void DeleteProductUI(Store store)
        {
            store.PrintAllProducts();
            int id = GetValidInt("Введите ID товара для удаления: ");
            store.DeleteProduct(id);
            WaitForKey();
        }

        static void SupplyProductUI(Store store)
        {
            store.PrintAllProducts();
            int id = GetValidInt("Введите ID товара для поставки: ");
            int amount = GetValidInt("Введите количество для поставки: ", 1);
            store.SupplyProduct(id, amount);
            WaitForKey();
        }

        static void SellProductUI(Store store)
        {
            store.PrintAllProducts();
            int id = GetValidInt("Введите ID товара для продажи: ");
            int amount = GetValidInt("Сколько штук продать: ", 1);
            store.SellProduct(id, amount);
            WaitForKey();
        }

        static int GetValidInt(string prompt, int min = 0, int max = int.MaxValue)
        {
            int result;
            while (true)
            {
                Console.Write(prompt);
                string input = Console.ReadLine();
                if (int.TryParse(input, out result) && result >= min && result <= max)
                {
                    return result;
                }
                Console.WriteLine($"Ошибка ввода. Введите целое число от {min} до {max}.");
            }
        }

        static decimal GetValidDecimal(string prompt)
        {
            decimal result;
            while (true)
            {
                Console.Write(prompt);
                string input = Console.ReadLine();
                if (decimal.TryParse(input, out result) && result >= 0)
                {
                    return result;
                }
                Console.WriteLine("Ошибка ввода. Введите неотрицательное число.");
            }
        }

        static void WaitForKey()
        {
            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }
    }
}