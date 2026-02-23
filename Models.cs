namespace EStoreManagementAPI;

public class User
{
    public int Id { get; set; }
    public required string Email { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
}

public class Category
{
    public int Id { get; set; }
    public required string Name { get; set; } = string.Empty;
}

public class Product
{
    public int Id { get; set; }
    public required string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }

    public int CategoryId { get; set; }
    public Category? Category { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }

    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
