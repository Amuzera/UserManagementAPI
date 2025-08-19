using Microsoft.EntityFrameworkCore;
using UserManagement.Models;


namespace UserManagement.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
}
