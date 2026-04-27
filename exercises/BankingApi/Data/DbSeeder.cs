using BankingApi.Models;

namespace BankingApi.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        // Avoid inserting duplicate seed rows in prod since the db isn't wiped on restart.
        // Compared to in-memory db, which is wiped on restart.
        if (db.Branches.Any())
        {
            return;
        }

        db.Branches.AddRange(
            new Branch { Name = "London HQ", Address = "1 Threadneedle St" },
            new Branch { Name = "Manchester", Address = "12 King St" },
            new Branch { Name = "Edinburgh", Address = "45 George St" }
        );

        db.SaveChanges();

        if (db.Accounts.Any())
        {
            return;
        }

        db.Accounts.AddRange(
            new Account { AccountNumber = "ACC-1000", BranchId = 1 },
            new Account { AccountNumber = "ACC-1001", BranchId = 2 },
            new Account { AccountNumber = "ACC-1002", BranchId = 3 }
        );

        db.SaveChanges();

        if (db.Customers.Any())
        {
            return;
        }

        db.Customers.AddRange(
            new Customer { FullName = "Ada Lovelace", Email = "ada@example.com" },
            new Customer { FullName = "Alan Turing", Email = "alan@example.com" },
            new Customer { FullName = "Grace Hopper", Email = "grace@example.com" }
        );

        db.SaveChanges();

        if (db.Transactions.Any())
        {
            return;
        }

        db.Transactions.AddRange(
            new Transaction
                { Type = TransactionType.Credit, Amount = 1000, Description = "Opening deposit", AccountId = 1 },
            new Transaction
                { Type = TransactionType.Credit, Amount = 1000, Description = "Opening deposit", AccountId = 2 },
            new Transaction
                { Type = TransactionType.Credit, Amount = 1000, Description = "Opening deposit", AccountId = 3 }
        );

        db.SaveChanges();
    }
}