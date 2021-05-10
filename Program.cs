using System;
using NLog.Web;
using System.IO;
using System.Linq;
using NorthwindConsole.Model;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace NorthwindConsole
{
    class Program
    {
        // create static instance of Logger
        private static NLog.Logger logger = NLogBuilder.ConfigureNLog(Directory.GetCurrentDirectory() + "\\nlog.config").GetCurrentClassLogger();
        static void Main(string[] args)
        {
            logger.Info("Program started");

            try
            {
                string choice;
                do
                {
                    Console.WriteLine("1) Display Categories");
                    Console.WriteLine("2) Add Category");
                    Console.WriteLine("3) Display Category and related products");
                    Console.WriteLine("4) Display all Categories and their related products");
                    Console.WriteLine("5) Edit Category");
                    Console.WriteLine("6) Add Product");
                    Console.WriteLine("7) Edit Product");
                    Console.WriteLine("8) Display Products");
                    Console.WriteLine("9) Delete Category");
                    Console.WriteLine("10) Delete Product");
                    Console.WriteLine("\"q\" to quit");
                    choice = Console.ReadLine();
                    Console.Clear();
                    logger.Info($"Option {choice} selected");
                    if (choice == "1")
                    {
                        var db = new NWConsole_96_TCJContext();
                        var query = db.Categories.OrderBy(p => p.CategoryName);

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"{query.Count()} records returned");
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        foreach (var item in query)
                        {
                            Console.WriteLine($"{item.CategoryName} - {item.Description}");
                        }
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    else if (choice == "2")
                    {
                        Categories category = new Categories();
                        Console.WriteLine("Enter Category Name:");
                        category.CategoryName = Console.ReadLine();
                        Console.WriteLine("Enter the Category Description:");
                        category.Description = Console.ReadLine();
                        
                        ValidationContext context = new ValidationContext(category, null, null);
                        List<ValidationResult> results = new List<ValidationResult>();

                        var isValid = Validator.TryValidateObject(category, context, results, true);
                        if (isValid)
                        {
                            var db = new NWConsole_96_TCJContext();
                            // check for unique name
                            if (db.Categories.Any(c => c.CategoryName == category.CategoryName))
                            {
                                // generate validation error
                                isValid = false;
                                results.Add(new ValidationResult("Name exists", new string[] { "CategoryName" }));
                            }
                            else
                            {
                                logger.Info("Validation passed");
                                db.AddCategory(category);
                            }
                        }
                        if (!isValid)
                        {
                            foreach (var result in results)
                            {
                                logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                            }
                        }
                    }
                    else if (choice == "3")
                    {
                        var db = new NWConsole_96_TCJContext();
                        var query = db.Categories.OrderBy(p => p.CategoryId);

                        Console.WriteLine("Select the category whose products you want to display:");
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        foreach (var item in query)
                        {
                            Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
                        }
                        Console.ForegroundColor = ConsoleColor.White;
                        int id = int.Parse(Console.ReadLine());
                        Console.Clear();
                        logger.Info($"CategoryId {id} selected");
                        Categories category = db.Categories.Include("Products").FirstOrDefault(c => c.CategoryId == id);
                        Console.WriteLine($"{category.CategoryName} - {category.Description}");
                        foreach (Products p in category.Products)
                        {
                            Console.WriteLine(p.ProductName);
                        }
                    }
                    else if (choice == "4")
                    {
                        var db = new NWConsole_96_TCJContext();
                        var query = db.Categories.Include("Products").OrderBy(p => p.CategoryId);
                        foreach (var item in query)
                        {
                            Console.WriteLine($"{item.CategoryName}");
                            foreach (Products p in item.Products)
                            {
                                Console.WriteLine($"\t{p.ProductName}");
                            }
                        }
                    }
                    else if (choice == "5")
                    {
                        //Edit Catagory
                        Console.WriteLine("Choose the Catagory to edit");
                        var db = new NWConsole_96_TCJContext();
                        var category = GetCategories(db);
                        if (category != null)
                        {
                            //input category
                            Categories UpdatedCategory = InputCategory(db);
                            if (UpdatedCategory != null)
                            {
                                UpdatedCategory.CategoryId = category.CategoryId;
                                db.EditCategory(UpdatedCategory);
                                logger.Info($"Category (id: {category.CategoryId}) updated");
                            }
                        }
                    }
                    else if (choice == "6")
                    {
                        // add product
                        Console.WriteLine("Select which Category the product falls under");
                        var db = new NWConsole_96_TCJContext();
                        var category = GetCategories(db);
                        if (category != null)
                        {
                            var product = new Products();
                            product.Category = category;
                            product.CategoryId = category.CategoryId;
                            Console.WriteLine("Enter name of product");
                            product.ProductName = Console.ReadLine();
                            Console.WriteLine("Enter Quantity per unit");
                            product.QuantityPerUnit = Console.ReadLine();
                            Console.WriteLine("Enter unit price");
                            product.UnitPrice = Decimal.Parse(Console.ReadLine());
                            Console.WriteLine("Enter Units in stock");
                            product.UnitsInStock = short.Parse(Console.ReadLine());
                            Console.WriteLine("Enter units on order");
                            product.UnitsOnOrder = short.Parse(Console.ReadLine());
                            Console.WriteLine("Enter reorder level");
                            product.ReorderLevel = short.Parse(Console.ReadLine());
                            Console.WriteLine("Discontinued? (Y/N)");
                            string dis = Console.ReadLine();
                            if (dis.ToUpper() == "Y")
                            {
                                product.Discontinued = true;
                            }
                            else
                            {
                                product.Discontinued = false;
                            }

                            db.AddProduct(product);
                            logger.Info("Product added - {name}", product.ProductName);
                        }
                    }
                    else if (choice == "7")
                    {
                        // edit product
                        Console.WriteLine("Choose product to edit:");
                        var db = new NWConsole_96_TCJContext();
                        var product = GetProducts(db);
                        if (product !=null)
                        {
                            // input product
                            Products UpdatedProduct = InputProducts(db);
                            if (UpdatedProduct != null)
                            {
                                UpdatedProduct.ProductId = product.ProductId;
                                db.EditProduct(UpdatedProduct);
                                logger.Info($"Product (ID: {product.ProductId}) updated");
                            }
                        }
                    }
                    else if (choice == "8")
                    {
                        // display products
                        Console.Write("Choose products to display");
                        Console.WriteLine("1) All Products");
                        Console.WriteLine("2) Discontinued Produts");
                        Console.WriteLine("3) Active Products");
                        string pick = Console.ReadLine();
                        var db = new NWConsole_96_TCJContext();
                        if (pick == "1")
                        {
                            var query = db.Products.OrderBy(p => p.ProductName);
                            Console.WriteLine($"{query.Count()} records returned");
                            foreach (var item in query)
                            {
                                Console.WriteLine($"{item.ProductName}");
                            }
                        }
                        else if (pick == "2")
                        {
                            var query = db.Products.Where(p => p.Discontinued).OrderBy(p => p.ProductName);
                            Console.WriteLine($"{query.Count()} records returned");
                            foreach (var item in query)
                            {
                                Console.WriteLine($"{item.ProductName}");
                            }
                        }
                        else if (pick == "3")
                        {
                            var query = db.Products.Where(p => !p.Discontinued).OrderBy(p => p.ProductName);
                            Console.WriteLine($"{query.Count()} records returned");
                            foreach (var item in query)
                            {
                                Console.WriteLine($"{item.ProductName}");
                            }
                        }
                    }
                    else if (choice == "9")
                    {
                        // delete category
                    }
                    else if (choice == "10")
                    {
                        // dlete product
                        Console.WriteLine("Select Product to delete");
                        var db = new NWConsole_96_TCJContext();
                        var product = GetProducts(db);
                        if (product != null)
                        {
                            // delete product
                            db.DeleteProduct(product);
                            logger.Info($"Product (id: {product.ProductId}) deleted");
                        }

                    }
                    Console.WriteLine();

                } while (choice.ToLower() != "q");
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }

            logger.Info("Program ended");
        }

        public static Categories GetCategories(NWConsole_96_TCJContext db)
        {
            // display all categories
            var categories = db.Categories.OrderBy(c => c.CategoryId);
            foreach (Categories c in categories)
            {
                Console.WriteLine($"{c.CategoryId}: {c.CategoryName}");
            }
            if (int.TryParse(Console.ReadLine(), out int CategoryId))
            {
                Categories category = db.Categories.FirstOrDefault(c => c.CategoryId == CategoryId);
                if (category != null)
                {
                    return category;
                }
            }
            logger.Error("Invalid Category ID");
            return null;
        }

        public static Categories InputCategory(NWConsole_96_TCJContext db)
        {
            Categories category = new Categories();
            Console.WriteLine("Enter the Category name");
            category.CategoryName = Console.ReadLine();

            ValidationContext context = new ValidationContext(category, null, null);
            List<ValidationResult> results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(category, context, results, true);
            if (isValid)
            {
                // check for unique name
                if (db.Categories.Any(c => c.CategoryName == category.CategoryName))
                {
                    // generate validation error
                    isValid = false;
                    results.Add(new ValidationResult("Category name exists", new string[] { "Name" }));
                }
                else
                {
                    logger.Info("Validation passed");
                }
            }
            if (!isValid)
            {
                foreach (var result in results)
                {
                    logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                }
                return null;
            }

            return category;
        }

        public static Products GetProducts(NWConsole_96_TCJContext db)
        {
            // display all products
            var products = db.Products.OrderBy(p =>p.ProductId);
            foreach (Products p in products)
            {
                Console.WriteLine($"{p.ProductId}: {p.ProductName}");
            }
            if (int.TryParse(Console.ReadLine(), out int ProductId))
            {
                Products product = db.Products.FirstOrDefault(p => p.ProductId == ProductId);
                if (product != null)
                {
                    return product;
                }
            }
            logger.Error("Invalid Product ID");
            return null;
        }

        public static Products InputProducts(NWConsole_96_TCJContext db)
        {
            Products product = new Products();
            Console.WriteLine("Enter product name");
            product.ProductName = Console.ReadLine();

            ValidationContext context = new ValidationContext(product, null, null);
            List<ValidationResult> results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(product, context, results, true);
            if (isValid)
            {
                // check for unique name
                if (db.Products.Any(p => p.ProductName == product.ProductName))
                {
                    // generate validation error
                    isValid = false;
                    results.Add(new ValidationResult("Product name Exists", new string[] { "Name" }));
                }
                else{
                    logger.Info("Validation passed");
                }
            }
            if (!isValid)
            {
                foreach (var result in results)
                {
                    logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                }
                return null;
            }

            return product;
        }
    }
}
