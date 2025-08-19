namespace UserManagement.Models;

public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public DateOnly? DateOfBirth { get; set; }
    public bool IsActive { get; set; } = true;
}
