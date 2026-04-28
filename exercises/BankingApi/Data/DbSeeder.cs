using BankingApi.Models;
using Microsoft.AspNetCore.Identity;

namespace BankingApi.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(
        AppDbContext db,
        UserManager<ApplicationUser> userManager)
    {
        if (await userManager.FindByEmailAsync("alice@example.com") is not null)
        {
            return;
        }

        var alice = await CreateUserAsync(userManager, "alice@example.com", "Passw0rd!");
        var bob = await CreateUserAsync(userManager, "bob@example.com", "Passw0rd!");

        await SeedUserDataAsync(db, alice.Id);
        await SeedUserDataAsync(db, bob.Id);
    }

    private static async Task<ApplicationUser> CreateUserAsync(
        UserManager<ApplicationUser> userManager, string email, string password)
    {
        var user = new ApplicationUser { UserName = email, Email = email };
        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
            throw new InvalidOperationException($"Failed to seed user {email}: {errors}");
        }

        return user;
    }

    public static async Task SeedUserDataAsync(AppDbContext db, string ownerId)
    {
        db.Branches.Add(new Branch { Name = "London HQ", Address = "1 Threadneedle St" });
        await db.SaveChangesAsync();

        db.Accounts.Add(new Account
        {
            OwnerId = ownerId,
            AccountNumber = $"ACC-{ownerId}-CHK",
            BranchId = 1
        });
        await db.SaveChangesAsync();

        db.Transactions.Add(new Transaction
        {
            Type = TransactionType.Credit,
            Amount = 1000,
            Description = "Opening deposit",
            AccountId = 1
        });
        await db.SaveChangesAsync();

        // Avoid inserting duplicate seed rows in prod since the db isn't wiped on restart.
        // Compared to in-memory db, which is wiped on restart.
        // if (db.Branches.Any())
        // {
        //     return;
        // }

        // db.Branches.AddRange(
        //     new Branch { Name = "London HQ", Address = "1 Threadneedle St" },
        //     new Branch { Name = "Manchester", Address = "12 King St" },
        //     new Branch { Name = "Edinburgh", Address = "45 George St" }
        // );
        // await db.SaveChangesAsync();

        // if (db.Accounts.Any())
        // {
        //     return;
        // }

        // db.Accounts.AddRange(
        //     new Account { AccountNumber = "ACC-1000", BranchId = 1 },
        //     new Account { AccountNumber = "ACC-1001", BranchId = 2 },
        //     new Account { AccountNumber = "ACC-1002", BranchId = 3 }
        // );
        // await db.SaveChangesAsync();

        // if (db.Customers.Any())
        // {
        //     return;
        // }

        // db.Customers.AddRange(
        //     new Customer { FullName = "Ada Lovelace", Email = "ada@example.com" },
        //     new Customer { FullName = "Alan Turing", Email = "alan@example.com" },
        //     new Customer { FullName = "Grace Hopper", Email = "grace@example.com" }
        // );
        // await db.SaveChangesAsync();

        // if (db.Transactions.Any())
        // {
        //     return;
        // }

        // db.Transactions.AddRange(
        //     new Transaction
        //         { Type = TransactionType.Credit, Amount = 1000, Description = "Opening deposit", AccountId = 1 },
        //     new Transaction
        //         { Type = TransactionType.Credit, Amount = 1000, Description = "Opening deposit", AccountId = 2 },
        //     new Transaction
        //         { Type = TransactionType.Credit, Amount = 1000, Description = "Opening deposit", AccountId = 3 }
        // );
        // await db.SaveChangesAsync();
    }
}