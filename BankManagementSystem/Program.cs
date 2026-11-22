using BankManagementSystem.Data;
using BankManagementSystem.Entities;
using Microsoft.EntityFrameworkCore;

namespace BankManagementSystem
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using var context = new ApplicationDbContext();

            // Apply migrations and seed initial data
            context.Database.Migrate();
            DbInitializer.Seed(context);

            bool running = true;

            while (running)
            {
                Console.WriteLine("======================================");
                Console.WriteLine("       National Bank - Management");
                Console.WriteLine("======================================");
                Console.WriteLine("1) Add a new Customer");
                Console.WriteLine("2) Open a new Account for a Customer");
                Console.WriteLine("3) Update Account Status (Active / Closed)");
                Console.WriteLine("4) Remove an Account from a Customer");
                Console.WriteLine("5) List all Customers (with accounts)");
                Console.WriteLine("0) Exit");
                Console.WriteLine("--------------------------------------");

                Console.Write("Enter choice: ");
                string choice = Console.ReadLine() ?? "";

                switch (choice)
                {
                    case "1":
                        AddCustomer(context);
                        Pause();
                        break;

                    case "2":
                        OpenAccount(context);
                        Pause();
                        break;

                    case "3":
                        UpdateAccountStatus(context);
                        Pause();
                        break;

                    case "4":
                        RemoveAccountFromCustomer(context);
                        Pause();
                        break;

                    case "5":
                        ListCustomers(context);
                        Pause();
                        break;

                    case "0":
                        running = false;
                        Console.WriteLine("Goodbye!");
                        break;

                    default:
                        Console.WriteLine(
                            "Invalid choice. Please enter a number from 0 to 5.");
                        Pause();
                        break;
                }

                if (running)
                {
                    Console.Clear();
                }
            }
        }

        // Option 1: Add Customer
        static void AddCustomer(ApplicationDbContext context)
        {
            Console.WriteLine("\n--- Add New Customer ---");

            string fullName = ReadRequired("Full Name: ");
            string nationalId = ReadRequired("National ID: ");

            if (context.Customers.Any(c => c.NationalId == nationalId))
            {
                Console.WriteLine("This National ID is already registered.");
                return;
            }

            DateTime dateOfBirth =
                ReadDate("Date of Birth (yyyy-MM-dd): ");

            string email = ReadRequired("Email: ");

            if (!email.Contains("@") || !email.Contains("."))
            {
                Console.WriteLine("Invalid email address.");
                return;
            }

            string phone = ReadRequired("Phone: ");
            string address = ReadRequired("Address: ");

            Console.WriteLine("Customer Type:");
            Console.WriteLine("1) Individual");
            Console.WriteLine("2) Business");

            string typeChoice = ReadChoice("Choice: ", "1", "2");

            string customerType =
                typeChoice == "1" ? "Individual" : "Business";

            var customer = new Customer
            {
                FullName = fullName,
                NationalId = nationalId,
                DateOfBirth = dateOfBirth,
                Email = email,
                PhoneNumber = phone,
                Address = address,
                CustomerType = customerType
            };

            context.Customers.Add(customer);
            context.SaveChanges();

            Console.WriteLine(
                $"\nCustomer created successfully. CustomerId = {customer.Id}");
        }

        // Option 2: Open Account or Add CoHolder
        static void OpenAccount(ApplicationDbContext context)
        {
            Console.WriteLine("\n--- Open New Account ---");

            string accountNumber = ReadRequired("Account Number: ");
            string branchCode = ReadRequired("Branch Code: ");
            int customerId = ReadInt("Customer Id: ");

            // Check Branch
            var branch = context.Branches
                .FirstOrDefault(b => b.Code == branchCode);

            if (branch == null)
            {
                Console.WriteLine("Branch does not exist.");
                return;
            }

            // Check Customer
            var customer = context.Customers
                .FirstOrDefault(c => c.Id == customerId);

            if (customer == null)
            {
                Console.WriteLine("Customer does not exist.");
                return;
            }

            // Check existing Account
            var existingAccount = context.Accounts
                .FirstOrDefault(a => a.AccountNumber == accountNumber);

            if (existingAccount != null &&
                existingAccount.BranchCode != branchCode)
            {
                Console.WriteLine(
                    "This account belongs to another branch.");
                return;
            }

            // Check duplicate ownership
            bool alreadyLinked = context.CustomerAccounts.Any(ca =>
                ca.AccountNumber == accountNumber &&
                ca.CustomerId == customerId);

            if (alreadyLinked)
            {
                Console.WriteLine(
                    "This customer is already linked to this account.");
                return;
            }

            string accountType = "";

            // Ask for Account Type only for a new Account
            if (existingAccount == null)
            {
                Console.WriteLine("Account Type:");
                Console.WriteLine("1) Savings");
                Console.WriteLine("2) Current");
                Console.WriteLine("3) Business");

                string typeChoice =
                    ReadChoice("Choice: ", "1", "2", "3");

                accountType = typeChoice switch
                {
                    "1" => "Savings",
                    "2" => "Current",
                    _ => "Business"
                };
            }

            Console.WriteLine("Ownership Role:");
            Console.WriteLine("1) Primary");
            Console.WriteLine("2) CoHolder");

            string ownershipChoice =
                ReadChoice("Choice: ", "1", "2");

            string ownershipType =
                ownershipChoice == "1" ? "Primary" : "CoHolder";

            Console.WriteLine(
                $"Validating branch '{branchCode}' and customer {customerId}...");

            using var transaction =
                context.Database.BeginTransaction();

            try
            {
                // Create Account if it does not exist
                if (existingAccount == null)
                {
                    var account = new Account
                    {
                        AccountNumber = accountNumber,
                        AccountType = accountType,
                        CurrentBalance = 0,
                        OpeningDate = DateTime.Now,
                        BranchCode = branchCode
                    };

                    context.Accounts.Add(account);
                }

                // Link Customer to Account
                var customerAccount = new CustomerAccount
                {
                    CustomerId = customerId,
                    AccountNumber = accountNumber,
                    OwnershipType = ownershipType,
                    OwnershipStartDate = DateTime.Now,
                    AccountStatus = "Active"
                };

                context.CustomerAccounts.Add(customerAccount);

                context.SaveChanges();
                transaction.Commit();

                if (existingAccount == null)
                {
                    Console.WriteLine(
                        $"Account '{accountNumber}' created and linked to customer {customerId} as {ownershipType} owner.");
                }
                else
                {
                    Console.WriteLine(
                        $"Customer {customerId} added as {ownershipType} owner to joint account '{accountNumber}'.");
                }
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine($"Operation failed: {ex.Message}");
            }
        }

        // Option 3: Update Account Status
        static void UpdateAccountStatus(ApplicationDbContext context)
        {
            Console.WriteLine("\n--- Update Account Status ---");

            string accountNumber = ReadRequired("Account Number: ");
            int customerId = ReadInt("Customer Id: ");

            var customerAccount = context.CustomerAccounts
                .FirstOrDefault(ca =>
                    ca.AccountNumber == accountNumber &&
                    ca.CustomerId == customerId);

            if (customerAccount == null)
            {
                Console.WriteLine(
                    "This customer-account link does not exist.");
                return;
            }

            Console.WriteLine("New Status:");
            Console.WriteLine("1) Active");
            Console.WriteLine("2) Closed");

            string statusChoice =
                ReadChoice("Choice: ", "1", "2");

            customerAccount.AccountStatus =
                statusChoice == "1" ? "Active" : "Closed";

            context.SaveChanges();

            Console.WriteLine(
                $"Status updated to {customerAccount.AccountStatus}.");
        }

        // Option 4: Remove Account from Customer
        static void RemoveAccountFromCustomer(
            ApplicationDbContext context)
        {
            Console.WriteLine("\n--- Remove Account From Customer ---");

            string accountNumber = ReadRequired("Account Number: ");
            int customerId = ReadInt("Customer Id: ");

            var customerAccount = context.CustomerAccounts
                .FirstOrDefault(ca =>
                    ca.AccountNumber == accountNumber &&
                    ca.CustomerId == customerId);

            if (customerAccount == null)
            {
                Console.WriteLine(
                    "This customer-account link does not exist.");
                return;
            }

            context.CustomerAccounts.Remove(customerAccount);
            context.SaveChanges();

            Console.WriteLine("Ownership link deleted.");

            // Check if the account has other owners
            bool hasOtherOwners = context.CustomerAccounts
                .Any(ca => ca.AccountNumber == accountNumber);

            if (!hasOtherOwners)
            {
                bool hasTransactions = context.Transactions
                    .Any(t => t.AccountNumber == accountNumber);

                if (!hasTransactions)
                {
                    var account = context.Accounts
                        .FirstOrDefault(a =>
                            a.AccountNumber == accountNumber);

                    if (account != null)
                    {
                        context.Accounts.Remove(account);
                        context.SaveChanges();

                        Console.WriteLine(
                            $"That was the last owner - account '{accountNumber}' was also removed.");
                    }
                }
                else
                {
                    Console.WriteLine(
                        "The account was retained because it has transaction history.");
                }
            }
        }

        // Option 5: List Customers with Accounts
        static void ListCustomers(ApplicationDbContext context)
        {
            Console.WriteLine("\n--- All Customers ---");

            var customers = context.Customers
                .AsNoTracking()
                .Include(c => c.CustomerAccounts)
                    .ThenInclude(ca => ca.Account)
                        .ThenInclude(a => a.Branch)
                .OrderBy(c => c.Id)
                .ToList();

            if (!customers.Any())
            {
                Console.WriteLine("No customers found.");
                return;
            }

            foreach (var customer in customers)
            {
                Console.WriteLine(
                    $"\n#{customer.Id} {customer.FullName} ({customer.CustomerType})");

                if (!customer.CustomerAccounts.Any())
                {
                    Console.WriteLine("   (no accounts)");
                    continue;
                }

                foreach (var customerAccount in customer.CustomerAccounts)
                {
                    var account = customerAccount.Account;

                    Console.WriteLine(
                        $"   {account.AccountNumber}  " +
                        $"{account.AccountType}  " +
                        $"Balance: {account.CurrentBalance:N2}  " +
                        $"{customerAccount.OwnershipType}  " +
                        $"{customerAccount.AccountStatus}  " +
                        $"@ {account.Branch.Name}");
                }
            }
        }

        // Helper: Read required text
        static string ReadRequired(string message)
        {
            while (true)
            {
                Console.Write(message);

                string value = Console.ReadLine()?.Trim() ?? "";

                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }

                Console.WriteLine(
                    "This field is required. Try again.");
            }
        }

        // Helper: Read positive integer safely
        static int ReadInt(string message)
        {
            while (true)
            {
                Console.Write(message);

                string input = Console.ReadLine() ?? "";

                if (int.TryParse(input, out int result) &&
                    result > 0)
                {
                    return result;
                }

                Console.WriteLine(
                    "Please enter a valid positive number.");
            }
        }

        // Helper: Read date safely
        static DateTime ReadDate(string message)
        {
            while (true)
            {
                Console.Write(message);

                string input = Console.ReadLine() ?? "";

                if (DateTime.TryParse(input, out DateTime result) &&
                    result < DateTime.Today)
                {
                    return result;
                }

                Console.WriteLine(
                    "Please enter a valid date in the past.");
            }
        }

        // Helper: Read allowed choices
        static string ReadChoice(
            string message,
            params string[] choices)
        {
            while (true)
            {
                Console.Write(message);

                string input = Console.ReadLine() ?? "";

                if (choices.Contains(input))
                {
                    return input;
                }

                Console.WriteLine("Invalid choice. Try again.");
            }
        }

        // Helper: Pause before returning to menu
        static void Pause()
        {
            Console.WriteLine(
                "\nPress any key to return to the menu...");

            Console.ReadKey(true);
        }
    }
}