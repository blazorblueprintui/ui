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
    public int Salary { get; set; }
    public DateTime JoinDate { get; set; }
    public bool IsActive { get; set; }
}
