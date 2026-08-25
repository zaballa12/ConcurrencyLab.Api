using ConcurrencyLab.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ConcurrencyLab.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<AppUser>>();

        await context.Database.MigrateAsync();

        if (await context.Users.AnyAsync())
        {
            return;
        }

        var admin = new AppUser
        {
            Username = "admin",
            Role = "Admin"
        };

        admin.PasswordHash = passwordHasher.HashPassword(admin, "Admin123!");

        var operatorUser = new AppUser
        {
            Username = "operator",
            Role = "Operator"
        };

        operatorUser.PasswordHash = passwordHasher.HashPassword(operatorUser, "Operator123!");

        context.Users.AddRange(admin, operatorUser);
        await context.SaveChangesAsync();
    }
}
