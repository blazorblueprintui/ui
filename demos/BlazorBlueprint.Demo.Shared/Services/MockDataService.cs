using System.Globalization;

namespace BlazorBlueprint.Demo.Services;

/// <summary>
/// Service for generating mock data for demos.
/// </summary>
public class MockDataService
{
    private static readonly Random _random = new();

    private static readonly string[] _firstNames = {
        "James", "Mary", "John", "Patricia", "Robert", "Jennifer", "Michael", "Linda",
        "William", "Barbara", "David", "Elizabeth", "Richard", "Susan", "Joseph", "Jessica",
        "Thomas", "Sarah", "Charles", "Karen", "Christopher", "Nancy", "Daniel", "Lisa",
        "Matthew", "Betty", "Anthony", "Margaret", "Mark", "Sandra", "Donald", "Ashley",
        "Steven", "Kimberly", "Paul", "Emily", "Andrew", "Donna", "Joshua", "Michelle",
        "Kenneth", "Dorothy", "Kevin", "Carol", "Brian", "Amanda", "George", "Melissa",
        "Edward", "Deborah", "Ronald", "Stephanie", "Timothy", "Rebecca", "Jason", "Sharon",
        "Jeffrey", "Laura", "Ryan", "Cynthia", "Jacob", "Kathleen", "Gary", "Amy",
        "Nicholas", "Shirley", "Eric", "Angela", "Jonathan", "Helen", "Stephen", "Anna",
        "Larry", "Brenda", "Justin", "Pamela", "Scott", "Nicole", "Brandon", "Emma",
        "Benjamin", "Samantha", "Samuel", "Katherine", "Raymond", "Christine", "Gregory", "Debra"
    };

    private static readonly string[] _lastNames = {
        "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis",
        "Rodriguez", "Martinez", "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson", "Thomas",
        "Taylor", "Moore", "Jackson", "Martin", "Lee", "Perez", "Thompson", "White",
        "Harris", "Sanchez", "Clark", "Ramirez", "Lewis", "Robinson", "Walker", "Young",
        "Allen", "King", "Wright", "Scott", "Torres", "Nguyen", "Hill", "Flores",
        "Green", "Adams", "Nelson", "Baker", "Hall", "Rivera", "Campbell", "Mitchell",
        "Carter", "Roberts", "Gomez", "Phillips", "Evans", "Turner", "Diaz", "Parker",
        "Cruz", "Edwards", "Collins", "Reyes", "Stewart", "Morris", "Morales", "Murphy",
        "Cook", "Rogers", "Gutierrez", "Ortiz", "Morgan", "Cooper", "Peterson", "Bailey",
        "Reed", "Kelly", "Howard", "Ramos", "Kim", "Cox", "Ward", "Richardson",
        "Watson", "Brooks", "Chavez", "Wood", "James", "Bennett", "Gray", "Mendoza"
    };

    private static readonly string[] _roles = {
        "Admin", "User", "Guest", "Moderator", "Manager", "Developer", "Designer", "Analyst"
    };

    private static readonly string[] _statuses = {
        "Active", "Inactive", "Pending", "Suspended"
    };

    private static readonly string[] _departments = {
        "Engineering", "Sales", "Marketing", "Support", "HR", "Finance", "Operations", "Product"
    };

    private static readonly string[] _productAdjectives = {
        "Pro", "Ultra", "Mini", "Max", "Lite", "Smart", "Classic", "Premium", "Elite", "Eco",
        "Slim", "Flex", "Rapid", "Fusion", "Nano", "Turbo", "Swift", "Bold", "Core", "Pure"
    };

    private static readonly string[] _productNouns = {
        "Headphones", "Speaker", "Backpack", "Watch", "Keyboard", "Lamp", "Sneakers", "Bottle",
        "Camera", "Notebook", "Hoodie", "Sunglasses", "Wallet", "Mug", "Charger", "Pad",
        "Stand", "Hub", "Cable", "Pouch", "Earbuds", "Mat", "Tote", "Jacket", "Desk"
    };

    private static readonly string[] _productCategories = {
        "Electronics", "Footwear", "Accessories", "Home", "Apparel"
    };

    // A set of representative Unsplash photo IDs that render nicely as product images.
    private static readonly string[] _productImageUrls = {
        "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=400&auto=format&fit=crop",
        "https://images.unsplash.com/photo-1526170375885-4d8ecf77b99f?w=400&auto=format&fit=crop",
        "https://images.unsplash.com/photo-1496181133206-80ce9b88a853?w=400&auto=format&fit=crop",
        "https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?w=400&auto=format&fit=crop",
        "https://images.unsplash.com/photo-1587829741301-dc798b83add3?w=400&auto=format&fit=crop",
        "https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=400&auto=format&fit=crop",
        "https://images.unsplash.com/photo-1608256246200-53e635b5b65f?w=400&auto=format&fit=crop",
        "https://images.unsplash.com/photo-1600269452121-4f2416e55c28?w=400&auto=format&fit=crop",
        "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=400&auto=format&fit=crop",
        "https://images.unsplash.com/photo-1553062407-98eeb64c6a62?w=400&auto=format&fit=crop",
        "https://images.unsplash.com/photo-1572635196237-14b3f281503f?w=400&auto=format&fit=crop",
        "https://images.unsplash.com/photo-1507473885765-e6ed057f782c?w=400&auto=format&fit=crop",
        "https://images.unsplash.com/photo-1485955900006-10f4d324d411?w=400&auto=format&fit=crop",
        "https://images.unsplash.com/photo-1495474472287-4d71bcdd2085?w=400&auto=format&fit=crop",
        "https://images.unsplash.com/photo-1602607630074-87e3df36f809?w=400&auto=format&fit=crop",
        "https://images.unsplash.com/photo-1556821840-3a63f15732ce?w=400&auto=format&fit=crop",
        "https://images.unsplash.com/photo-1598033129183-c4f50c736f10?w=400&auto=format&fit=crop",
        "https://images.unsplash.com/photo-1473966968600-fa801b869a1a?w=400&auto=format&fit=crop",
        "https://images.unsplash.com/photo-1608043152269-423dbba4e7e1?w=400&auto=format&fit=crop",
        "https://images.unsplash.com/photo-1602143407151-7111542de6e8?w=400&auto=format&fit=crop",
    };

    /// <summary>
    /// Generates a list of mock person records.
    /// </summary>
    /// <param name="count">Number of records to generate.</param>
    /// <returns>List of person records with randomized data.</returns>
    public static List<Person> GeneratePersons(int count)
    {
        var persons = new List<Person>();
        for (var i = 0; i < count; i++)
        {
            persons.Add(new Person
            {
                Id = i + 1,
                Name = GenerateFullName(),
                Email = GenerateEmail(),
                Age = _random.Next(18, 70),
                Role = _roles[_random.Next(_roles.Length)],
                Status = _statuses[_random.Next(_statuses.Length)],
                Department = _departments[_random.Next(_departments.Length)],
                LastPromotionDate = GeneratePromotionDate(),
                Salary = _random.Next(40000, 150000),
                JoinDate = DateTime.Now.AddDays(-_random.Next(1, 3650)), // Random date within last 10 years
                IsActive = _random.Next(100) > 20 // 80% chance of being active
            });
        }
        return persons;
    }

    private static string GenerateFullName()
    {
        var firstName = _firstNames[_random.Next(_firstNames.Length)];
        var lastName = _lastNames[_random.Next(_lastNames.Length)];
        return $"{firstName} {lastName}";
    }

    private static string GenerateEmail()
    {
        var firstName = _firstNames[_random.Next(_firstNames.Length)].ToLower(CultureInfo.InvariantCulture);
        var lastName = _lastNames[_random.Next(_lastNames.Length)].ToLower(CultureInfo.InvariantCulture);
        var domain = _random.Next(5) switch
        {
            0 => "example.com",
            1 => "company.com",
            2 => "business.org",
            3 => "enterprise.net",
            _ => "organization.com"
        };
        var suffix = _random.Next(100) > 70 ? _random.Next(1, 999).ToString(CultureInfo.InvariantCulture) : "";
        return $"{firstName}.{lastName}{suffix}@{domain}";
    }

    private static DateTimeOffset? GeneratePromotionDate() =>
        DateTimeOffset.UtcNow - TimeSpan.FromDays(_random.Next(365, 730));

    /// <summary>
    /// Generates a list of mock product records.
    /// </summary>
    /// <param name="count">Number of records to generate.</param>
    /// <returns>List of product records with randomized data.</returns>
    public static List<Product> GenerateProducts(int count)
    {
        var products = new List<Product>();
        for (var i = 0; i < count; i++)
        {
            var adj = _productAdjectives[_random.Next(_productAdjectives.Length)];
            var noun = _productNouns[_random.Next(_productNouns.Length)];
            var category = _productCategories[_random.Next(_productCategories.Length)];
            var imageUrl = _productImageUrls[i % _productImageUrls.Length];
            var stock = _random.Next(100) < 10 ? 0 : _random.Next(1, 100);

            products.Add(new Product
            {
                Id = i + 1,
                Name = $"{adj} {noun}",
                Category = category,
                Price = Math.Round((decimal)(_random.NextDouble() * 290 + 9.99), 2),
                Rating = Math.Round(_random.NextDouble() * 2 + 3, 1),
                Stock = stock,
                ImageUrl = imageUrl,
            });
        }

        return products;
    }

/// <summary>
/// Represents a person with various properties for demo purposes.
/// </summary>
public class Person
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public DateTimeOffset? LastPromotionDate { get; set; }
    public int Salary { get; set; }
    public DateTime JoinDate { get; set; }
    public bool IsActive { get; set; }
}
