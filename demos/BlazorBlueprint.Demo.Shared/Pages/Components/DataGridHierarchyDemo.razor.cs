using BlazorBlueprint.Components;
using BlazorBlueprint.Primitives;
using BlazorBlueprint.Primitives.Filtering;
using BlazorBlueprint.Primitives.Utilities;
using Microsoft.AspNetCore.Components;

namespace BlazorBlueprint.Demo.Pages.Components;

public partial class DataGridHierarchyDemo : ComponentBase
{
    private int totalPlaceCount;
    private FilterDefinition orgFilter = new();
    private Func<Employee, bool>? orgFilterPredicate;
    private HierarchyFilterMode orgFilterMode = HierarchyFilterMode.ShowMatchedOnly;

    private static readonly SelectOption<HierarchyFilterMode>[] hierarchyFilterModeOptions =
    {
        new(HierarchyFilterMode.ShowMatchedSubtree, "Show Matched Subtree"),
        new(HierarchyFilterMode.ShowMatchedOnly, "Show Matched Only")
    };

    private static readonly string[] departments = { "Executive", "Engineering", "Product", "Sales" };
    private static readonly string[] locations = { "Copenhagen", "Seattle", "London", "New York", "San Francisco", "Austin", "Denver", "Portland", "Remote", "Chicago", "Berlin", "Tokyo", "Dublin", "Mumbai", "Madrid", "Singapore", "Dubai", "Boston", "Philadelphia", "Paris" };

    private readonly FilterField[] orgFields =
    {
        new() { Name = "Name", Label = "Name", Type = FilterFieldType.Text, Placeholder = "e.g. Alice" },
        new() { Name = "JobTitle", Label = "Job Title", Type = FilterFieldType.Text, Placeholder = "e.g. Director" },
        new()
        {
            Name = "Department", Label = "Department", Type = FilterFieldType.Enum,
            Options = departments.Select(d => new SelectOption<string>(d, d)).ToArray()
        },
        new()
        {
            Name = "Location", Label = "Location", Type = FilterFieldType.Enum,
            Options = locations.Order().Select(l => new SelectOption<string>(l, l)).ToArray()
        }
    };

    protected override void OnInitialized() =>
        totalPlaceCount = CountPlaces(worldData);

    private void HandleOrgFilterChanged(FilterDefinition newFilter)
    {
        orgFilterPredicate = newFilter.IsEmpty || !HasCompleteConditions(newFilter)
            ? null
            : newFilter.ToFunc<Employee>(orgFields);
        StateHasChanged();
    }

    private static bool HasCompleteConditions(FilterDefinition filter)
    {
        foreach (var c in filter.Conditions)
        {
            if (!string.IsNullOrEmpty(c.Field))
            {
                return true;
            }
        }

        foreach (var g in filter.Groups)
        {
            if (HasCompleteConditions(g))
            {
                return true;
            }
        }

        return false;
    }

    private static int CountPlaces(List<Place>? items)
    {
        if (items == null)
        {
            return 0;
        }

        var count = 0;
        foreach (var item in items)
        {
            count += 1 + CountPlaces(item.Children);
        }
        return count;
    }

    // ── Medal Entry (nested hierarchy) ──

    private sealed class MedalEntry
    {
        public string Name { get; init; } = "";
        public int Gold { get; init; }
        public int Silver { get; init; }
        public int Bronze { get; init; }
        public int Total => Gold + Silver + Bronze;
        public List<MedalEntry>? Children { get; init; }
    }

    private List<MedalEntry> continents = new()
    {
        new()
        {
            Name = "Africa", Gold = 15, Silver = 12, Bronze = 18,
            Children = new()
            {
                new() { Name = "Kenya", Gold = 4, Silver = 2, Bronze = 5 },
                new() { Name = "South Africa", Gold = 3, Silver = 3, Bronze = 2 },
                new() { Name = "Ethiopia", Gold = 3, Silver = 3, Bronze = 4 },
                new() { Name = "Algeria", Gold = 2, Silver = 1, Bronze = 3 },
                new() { Name = "Egypt", Gold = 1, Silver = 1, Bronze = 2 },
                new() { Name = "Nigeria", Gold = 1, Silver = 1, Bronze = 1 },
                new() { Name = "Morocco", Gold = 1, Silver = 1, Bronze = 1 }
            }
        },
        new()
        {
            Name = "Asia", Gold = 103, Silver = 87, Bronze = 100,
            Children = new()
            {
                new() { Name = "China", Gold = 38, Silver = 32, Bronze = 19 },
                new() { Name = "Japan", Gold = 27, Silver = 14, Bronze = 17 },
                new() { Name = "South Korea", Gold = 16, Silver = 11, Bronze = 10 },
                new() { Name = "India", Gold = 7, Silver = 10, Bronze = 14 },
                new() { Name = "Iran", Gold = 6, Silver = 5, Bronze = 12 },
                new() { Name = "Uzbekistan", Gold = 5, Silver = 8, Bronze = 15 },
                new() { Name = "Chinese Taipei", Gold = 4, Silver = 7, Bronze = 13 }
            }
        },
        new()
        {
            Name = "Europe", Gold = 122, Silver = 127, Bronze = 165,
            Children = new()
            {
                new() { Name = "Great Britain", Gold = 22, Silver = 21, Bronze = 22 },
                new() { Name = "France", Gold = 16, Silver = 26, Bronze = 22 },
                new() { Name = "Netherlands", Gold = 15, Silver = 7, Bronze = 12 },
                new() { Name = "Germany", Gold = 12, Silver = 13, Bronze = 8 },
                new() { Name = "Italy", Gold = 12, Silver = 13, Bronze = 15 },
                new() { Name = "Australia", Gold = 11, Silver = 9, Bronze = 14 },
                new() { Name = "Spain", Gold = 6, Silver = 5, Bronze = 10 },
                new() { Name = "Hungary", Gold = 5, Silver = 7, Bronze = 6 },
                new() { Name = "Poland", Gold = 4, Silver = 6, Bronze = 8 },
                new() { Name = "Sweden", Gold = 4, Silver = 5, Bronze = 8 },
                new() { Name = "Romania", Gold = 3, Silver = 5, Bronze = 11 },
                new() { Name = "Norway", Gold = 4, Silver = 3, Bronze = 5 }
            }
        },
        new() { Name = "Oceania", Gold = 18, Silver = 13, Bronze = 22 },
        new() { Name = "North America", Gold = 59, Silver = 66, Bronze = 55 },
        new() { Name = "South America", Gold = 10, Silver = 14, Bronze = 22 }
    };

    // ── Employee (self-referencing / flat hierarchy) ──

    private sealed class Employee
    {
        public string Id { get; init; } = "";
        public string? ReportsToId { get; init; }
        public string Name { get; init; } = "";
        public string JobTitle { get; init; } = "";
        public string Department { get; init; } = "";
        public string Location { get; init; } = "";
        public string? AvatarUrl { get; init; }
    }

    private static string Avatar(string name) =>
        $"_content/BlazorBlueprint.Demo.Shared/images/avatars/{name}.jpg";

    private static string GetInitials(string name)
    {
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length >= 2
            ? $"{parts[0][0]}{parts[^1][0]}"
            : name[..1].ToUpperInvariant();
    }

    private static string CountryCodeToFlag(string code) =>
        string.Concat(code.ToUpperInvariant().Select(c => char.ConvertFromUtf32(0x1F1E6 + c - 'A')));

    private List<Employee> employees = new()
    {
        // Level 0 — CEO
        new() { Id = "1", Name = "Mathew Taylor", JobTitle = "CEO", Department = "Executive", Location = "Singapore", AvatarUrl = "_content/BlazorBlueprint.Demo.Shared/images/mathew-icon.png" },

        // Level 1 — C-suite (3 direct reports)
        new() { Id = "2", ReportsToId = "1", Name = "Alice Chen", JobTitle = "VP Engineering", Department = "Engineering", Location = "Seattle", AvatarUrl = Avatar("alice-chen") },
        new() { Id = "3", ReportsToId = "1", Name = "William Williams", JobTitle = "VP Product", Department = "Product", Location = "London", AvatarUrl = Avatar("william-williams") },
        new() { Id = "4", ReportsToId = "1", Name = "Sarah Kim", JobTitle = "VP Sales", Department = "Sales", Location = "New York", AvatarUrl = Avatar("sarah-kim") },

        // Level 2 — Engineering directors (under Alice, 6 reports to trigger pagination)
        new() { Id = "10", ReportsToId = "2", Name = "Bob Smith", JobTitle = "Director, Frontend", Department = "Engineering", Location = "Seattle", AvatarUrl = Avatar("bob-smith") },
        new() { Id = "11", ReportsToId = "2", Name = "Carol Wu", JobTitle = "Director, Backend", Department = "Engineering", Location = "San Francisco", AvatarUrl = Avatar("carol-wu") },
        new() { Id = "12", ReportsToId = "2", Name = "Dave Jones", JobTitle = "Director, Infrastructure", Department = "Engineering", Location = "Remote", AvatarUrl = Avatar("dave-jones") },
        new() { Id = "13", ReportsToId = "2", Name = "Nina Patel", JobTitle = "Director, QA", Department = "Engineering", Location = "Austin", AvatarUrl = Avatar("nina-patel") },
        new() { Id = "14", ReportsToId = "2", Name = "Oscar Rivera", JobTitle = "Director, Security", Department = "Engineering", Location = "Denver", AvatarUrl = Avatar("oscar-rivera") },
        new() { Id = "15", ReportsToId = "2", Name = "Grace Liu", JobTitle = "Director, Data", Department = "Engineering", Location = "San Francisco", AvatarUrl = Avatar("grace-liu") },

        // Level 3 — Frontend team leads (under Bob, 6 reports)
        new() { Id = "20", ReportsToId = "10", Name = "Tom Green", JobTitle = "Lead, React", Department = "Engineering", Location = "Seattle", AvatarUrl = Avatar("tom-green") },
        new() { Id = "21", ReportsToId = "10", Name = "Lisa Park", JobTitle = "Lead, Blazor", Department = "Engineering", Location = "Portland", AvatarUrl = Avatar("lisa-park") },
        new() { Id = "22", ReportsToId = "10", Name = "James Wilson", JobTitle = "Lead, Mobile", Department = "Engineering", Location = "Remote", AvatarUrl = Avatar("james-wilson") },
        new() { Id = "23", ReportsToId = "10", Name = "Amy Zhang", JobTitle = "Lead, Design Systems", Department = "Engineering", Location = "Seattle", AvatarUrl = Avatar("amy-zhang") },
        new() { Id = "24", ReportsToId = "10", Name = "Kevin Moore", JobTitle = "Lead, Accessibility", Department = "Engineering", Location = "Chicago", AvatarUrl = Avatar("kevin-moore") },
        new() { Id = "25", ReportsToId = "10", Name = "Rachel Adams", JobTitle = "Lead, Performance", Department = "Engineering", Location = "Remote", AvatarUrl = Avatar("rachel-adams") },

        // Level 4 — Blazor engineers (under Lisa, 7 reports)
        new() { Id = "30", ReportsToId = "21", Name = "Mike Johnson", JobTitle = "Senior Engineer", Department = "Engineering", Location = "Portland", AvatarUrl = Avatar("mike-johnson") },
        new() { Id = "31", ReportsToId = "21", Name = "Emily Davis", JobTitle = "Senior Engineer", Department = "Engineering", Location = "Remote", AvatarUrl = Avatar("emily-davis") },
        new() { Id = "32", ReportsToId = "21", Name = "Chris Brown", JobTitle = "Engineer", Department = "Engineering", Location = "Portland", AvatarUrl = Avatar("chris-brown") },
        new() { Id = "33", ReportsToId = "21", Name = "Anna Lee", JobTitle = "Engineer", Department = "Engineering", Location = "Seattle", AvatarUrl = Avatar("anna-lee") },
        new() { Id = "34", ReportsToId = "21", Name = "Ryan Taylor", JobTitle = "Engineer", Department = "Engineering", Location = "Remote", AvatarUrl = Avatar("ryan-taylor") },
        new() { Id = "35", ReportsToId = "21", Name = "Sofia Martinez", JobTitle = "Junior Engineer", Department = "Engineering", Location = "Portland", AvatarUrl = Avatar("sofia-martinez") },
        new() { Id = "36", ReportsToId = "21", Name = "Daniel Kim", JobTitle = "Intern", Department = "Engineering", Location = "Portland", AvatarUrl = Avatar("daniel-kim") },

        // Level 5 — Mike's mentees (under Mike, 3 reports)
        new() { Id = "40", ReportsToId = "30", Name = "Alex Turner", JobTitle = "Junior Engineer", Department = "Engineering", Location = "Portland", AvatarUrl = Avatar("alex-turner") },
        new() { Id = "41", ReportsToId = "30", Name = "Zoe Chen", JobTitle = "Junior Engineer", Department = "Engineering", Location = "Remote", AvatarUrl = Avatar("zoe-chen") },
        new() { Id = "42", ReportsToId = "30", Name = "Ian Cooper", JobTitle = "Intern", Department = "Engineering", Location = "Portland", AvatarUrl = Avatar("ian-cooper") },

        // Level 3 — Backend team leads (under Carol)
        new() { Id = "50", ReportsToId = "11", Name = "Victor Hsu", JobTitle = "Lead, APIs", Department = "Engineering", Location = "San Francisco", AvatarUrl = Avatar("victor-hsu") },
        new() { Id = "51", ReportsToId = "11", Name = "Diana Ross", JobTitle = "Lead, Services", Department = "Engineering", Location = "Austin", AvatarUrl = Avatar("diana-ross") },
        new() { Id = "52", ReportsToId = "11", Name = "Peter Chang", JobTitle = "Lead, Database", Department = "Engineering", Location = "Remote", AvatarUrl = Avatar("peter-chang") },

        // Level 4 — API engineers (under Victor)
        new() { Id = "60", ReportsToId = "50", Name = "Hannah White", JobTitle = "Senior Engineer", Department = "Engineering", Location = "San Francisco", AvatarUrl = Avatar("hannah-white") },
        new() { Id = "61", ReportsToId = "50", Name = "Lucas Scott", JobTitle = "Engineer", Department = "Engineering", Location = "Remote", AvatarUrl = Avatar("lucas-scott") },

        // Level 2 — Product managers (under William, 7 reports to trigger pagination)
        new() { Id = "70", ReportsToId = "3", Name = "Eve Brown", JobTitle = "Senior PM", Department = "Product", Location = "London", AvatarUrl = Avatar("eve-brown") },
        new() { Id = "71", ReportsToId = "3", Name = "Frank Lee", JobTitle = "Senior Designer", Department = "Product", Location = "Berlin", AvatarUrl = Avatar("frank-lee") },
        new() { Id = "72", ReportsToId = "3", Name = "Maria Garcia", JobTitle = "PM, Growth", Department = "Product", Location = "Madrid", AvatarUrl = Avatar("maria-garcia") },
        new() { Id = "73", ReportsToId = "3", Name = "Akira Tanaka", JobTitle = "PM, Platform", Department = "Product", Location = "Tokyo", AvatarUrl = Avatar("akira-tanaka") },
        new() { Id = "74", ReportsToId = "3", Name = "Liam O'Brien", JobTitle = "PM, Enterprise", Department = "Product", Location = "Dublin", AvatarUrl = Avatar("liam-obrien") },
        new() { Id = "75", ReportsToId = "3", Name = "Priya Sharma", JobTitle = "Designer", Department = "Product", Location = "Mumbai", AvatarUrl = Avatar("priya-sharma") },
        new() { Id = "76", ReportsToId = "3", Name = "Nadia Volkov", JobTitle = "UX Researcher", Department = "Product", Location = "Berlin", AvatarUrl = Avatar("nadia-volkov") },

        // Level 3 — Design team (under Frank)
        new() { Id = "80", ReportsToId = "71", Name = "Oliver Wright", JobTitle = "UI Designer", Department = "Product", Location = "Berlin", AvatarUrl = Avatar("oliver-wright") },
        new() { Id = "81", ReportsToId = "71", Name = "Chloe Martin", JobTitle = "UI Designer", Department = "Product", Location = "Paris", AvatarUrl = Avatar("chloe-martin") },
        new() { Id = "82", ReportsToId = "71", Name = "Yuki Sato", JobTitle = "Motion Designer", Department = "Product", Location = "Tokyo", AvatarUrl = Avatar("yuki-sato") },

        // Level 2 — Sales team (under Sarah, 6 reports)
        new() { Id = "90", ReportsToId = "4", Name = "Marcus Johnson", JobTitle = "Director, Enterprise", Department = "Sales", Location = "New York", AvatarUrl = Avatar("marcus-johnson") },
        new() { Id = "91", ReportsToId = "4", Name = "Jessica Taylor", JobTitle = "Director, SMB", Department = "Sales", Location = "Chicago", AvatarUrl = Avatar("jessica-taylor") },
        new() { Id = "92", ReportsToId = "4", Name = "David Park", JobTitle = "Director, APAC", Department = "Sales", Location = "Singapore", AvatarUrl = Avatar("david-park") },
        new() { Id = "93", ReportsToId = "4", Name = "Elena Costa", JobTitle = "Director, EMEA", Department = "Sales", Location = "London", AvatarUrl = Avatar("elena-costa") },
        new() { Id = "94", ReportsToId = "4", Name = "Robert Chen", JobTitle = "Director, Partnerships", Department = "Sales", Location = "San Francisco", AvatarUrl = Avatar("robert-chen") },
        new() { Id = "95", ReportsToId = "4", Name = "Fatima Al-Hassan", JobTitle = "Sales Operations", Department = "Sales", Location = "Dubai", AvatarUrl = Avatar("fatima-al-hassan") },

        // Level 3 — Enterprise sales reps (under Marcus, 6 reports)
        new() { Id = "100", ReportsToId = "90", Name = "Tyler Brooks", JobTitle = "Account Executive", Department = "Sales", Location = "New York", AvatarUrl = Avatar("tyler-brooks") },
        new() { Id = "101", ReportsToId = "90", Name = "Samantha Reed", JobTitle = "Account Executive", Department = "Sales", Location = "Boston", AvatarUrl = Avatar("samantha-reed") },
        new() { Id = "102", ReportsToId = "90", Name = "Jake Morrison", JobTitle = "Account Executive", Department = "Sales", Location = "New York", AvatarUrl = Avatar("jake-morrison") },
        new() { Id = "103", ReportsToId = "90", Name = "Megan Foster", JobTitle = "SDR", Department = "Sales", Location = "Philadelphia", AvatarUrl = Avatar("megan-foster") },
        new() { Id = "104", ReportsToId = "90", Name = "Brandon Lee", JobTitle = "SDR", Department = "Sales", Location = "Remote", AvatarUrl = Avatar("brandon-lee") },
        new() { Id = "105", ReportsToId = "90", Name = "Ashley Wong", JobTitle = "SDR", Department = "Sales", Location = "New York", AvatarUrl = Avatar("ashley-wong") }
    };

    // ── Department (lazy loading) ──

    private sealed class Department
    {
        public string Id { get; init; } = "";
        public string Name { get; init; } = "";
        public int EmployeeCount { get; init; }
        public string Head { get; init; } = "";
        public bool HasSubDepartments { get; init; }
    }

    private List<Department> rootDepartments = new()
    {
        new() { Id = "eng", Name = "Engineering", EmployeeCount = 45, Head = "Alice Chen", HasSubDepartments = true },
        new() { Id = "product", Name = "Product", EmployeeCount = 20, Head = "William Williams", HasSubDepartments = true },
        new() { Id = "hr", Name = "Human Resources", EmployeeCount = 8, Head = "Jane Doe", HasSubDepartments = false },
        new() { Id = "finance", Name = "Finance", EmployeeCount = 12, Head = "John Doe", HasSubDepartments = false }
    };

    private static async Task<IEnumerable<Department>> LoadDepartmentChildrenAsync(Department parent)
    {
        await Task.Delay(800);

        return parent.Id switch
        {
            "eng" => new List<Department>
            {
                new() { Id = "eng-fe", Name = "Frontend", EmployeeCount = 15, Head = "Bob Smith", HasSubDepartments = false },
                new() { Id = "eng-be", Name = "Backend", EmployeeCount = 18, Head = "Carol Wu", HasSubDepartments = false },
                new() { Id = "eng-infra", Name = "Infrastructure", EmployeeCount = 12, Head = "Dave Jones", HasSubDepartments = false }
            },
            "product" => new List<Department>
            {
                new() { Id = "product-design", Name = "Design", EmployeeCount = 8, Head = "Frank Lee", HasSubDepartments = false },
                new() { Id = "product-pm", Name = "Product Management", EmployeeCount = 12, Head = "Eve Brown", HasSubDepartments = false }
            },
            _ => Enumerable.Empty<Department>()
        };
    }

    // ── Place (large geography dataset) ──

    private sealed class Place
    {
        public string Id { get; set; } = "";
        public string Name { get; init; } = "";
        public string Type { get; init; } = "";
        public long Population { get; init; }
        public string Code { get; init; } = "";
        public List<Place>? Children { get; init; }
    }

    // ── FileEntry (file explorer) ──

    private IReadOnlyCollection<FileEntry>? selectedFiles;

    private sealed class FileEntry
    {
        public string Id { get; init; } = "";
        public string Name { get; init; } = "";
        public bool IsFolder { get; init; }
        public long? Size { get; init; }
        public string? Extension { get; init; }
        public DateTime Modified { get; init; }
        public List<FileEntry>? Children { get; init; }
    }

    private static string GetFileIcon(FileEntry entry)
    {
        if (entry.IsFolder)
        {
            return "folder";
        }

        return entry.Extension switch
        {
            ".cs" or ".js" or ".ts" or ".razor" or ".html" or ".css" => "file-code",
            ".json" or ".xml" or ".yaml" or ".yml" => "file-json-2",
            ".png" or ".jpg" or ".svg" or ".gif" or ".ico" => "file-image",
            ".md" or ".txt" => "file-text",
            ".sln" or ".csproj" => "file-cog",
            _ => "file"
        };
    }

    private static string FormatFileSize(long? bytes)
    {
        if (bytes == null)
        {
            return "--";
        }

        double b = bytes.Value;
        string[] units = { "B", "KB", "MB", "GB" };
        var i = 0;
        while (b >= 1024 && i < units.Length - 1)
        {
            b /= 1024;
            i++;
        }

        return $"{b:0.#} {units[i]}";
    }

    private static readonly List<FileEntry> fileTree = new()
    {
        new()
        {
            Id = "f1", Name = "src", IsFolder = true, Modified = new(2025, 12, 5),
            Children = new()
            {
                new()
                {
                    Id = "f2", Name = "Components", IsFolder = true, Modified = new(2025, 12, 1),
                    Children = new()
                    {
                        new() { Id = "f3", Name = "Button.razor", Extension = ".razor", Size = 2_150, Modified = new(2025, 12, 1) },
                        new() { Id = "f4", Name = "Button.razor.cs", Extension = ".cs", Size = 1_840, Modified = new(2025, 12, 1) },
                        new() { Id = "f5", Name = "Dialog.razor", Extension = ".razor", Size = 3_420, Modified = new(2025, 11, 28) },
                        new() { Id = "f6", Name = "Dialog.razor.cs", Extension = ".cs", Size = 4_200, Modified = new(2025, 11, 28) },
                        new() { Id = "f7", Name = "DataGrid.razor", Extension = ".razor", Size = 8_960, Modified = new(2025, 12, 1) },
                        new() { Id = "f8", Name = "DataGrid.razor.cs", Extension = ".cs", Size = 15_300, Modified = new(2025, 12, 1) }
                    }
                },
                new()
                {
                    Id = "f9", Name = "Pages", IsFolder = true, Modified = new(2025, 12, 5),
                    Children = new()
                    {
                        new() { Id = "f10", Name = "Home.razor", Extension = ".razor", Size = 1_200, Modified = new(2025, 12, 5) },
                        new() { Id = "f11", Name = "Settings.razor", Extension = ".razor", Size = 2_800, Modified = new(2025, 11, 30) },
                        new() { Id = "f12", Name = "Dashboard.razor", Extension = ".razor", Size = 5_100, Modified = new(2025, 12, 3) }
                    }
                },
                new()
                {
                    Id = "f13", Name = "wwwroot", IsFolder = true, Modified = new(2025, 11, 15),
                    Children = new()
                    {
                        new()
                        {
                            Id = "f14", Name = "css", IsFolder = true, Modified = new(2025, 11, 15),
                            Children = new()
                            {
                                new() { Id = "f15", Name = "app.css", Extension = ".css", Size = 920, Modified = new(2025, 11, 15) },
                                new() { Id = "f16", Name = "tailwind.css", Extension = ".css", Size = 48_200, Modified = new(2025, 11, 15) }
                            }
                        },
                        new()
                        {
                            Id = "f17", Name = "images", IsFolder = true, Modified = new(2025, 10, 20),
                            Children = new()
                            {
                                new() { Id = "f18", Name = "logo.svg", Extension = ".svg", Size = 4_500, Modified = new(2025, 10, 20) },
                                new() { Id = "f19", Name = "hero.png", Extension = ".png", Size = 131_400, Modified = new(2025, 10, 20) },
                                new() { Id = "f20", Name = "og-image.jpg", Extension = ".jpg", Size = 89_600, Modified = new(2025, 10, 20) }
                            }
                        },
                        new() { Id = "f21", Name = "favicon.ico", Extension = ".ico", Size = 1_100, Modified = new(2025, 10, 1) }
                    }
                },
                new() { Id = "f22", Name = "App.razor", Extension = ".razor", Size = 520, Modified = new(2025, 12, 5) },
                new() { Id = "f23", Name = "Program.cs", Extension = ".cs", Size = 1_300, Modified = new(2025, 12, 3) }
            }
        },
        new()
        {
            Id = "f24", Name = "tests", IsFolder = true, Modified = new(2025, 12, 4),
            Children = new()
            {
                new() { Id = "f25", Name = "ComponentTests.cs", Extension = ".cs", Size = 3_200, Modified = new(2025, 12, 4) },
                new() { Id = "f26", Name = "PageTests.cs", Extension = ".cs", Size = 2_100, Modified = new(2025, 12, 2) },
                new() { Id = "f27", Name = "IntegrationTests.cs", Extension = ".cs", Size = 4_800, Modified = new(2025, 12, 4) }
            }
        },
        new()
        {
            Id = "f28", Name = "docs", IsFolder = true, Modified = new(2025, 12, 6),
            Children = new()
            {
                new() { Id = "f29", Name = "README.md", Extension = ".md", Size = 4_800, Modified = new(2025, 12, 6) },
                new() { Id = "f30", Name = "CHANGELOG.md", Extension = ".md", Size = 12_400, Modified = new(2025, 12, 5) },
                new() { Id = "f31", Name = "architecture.md", Extension = ".md", Size = 6_200, Modified = new(2025, 11, 20) }
            }
        },
        new() { Id = "f32", Name = ".gitignore", Size = 310, Modified = new(2025, 10, 1) },
        new() { Id = "f33", Name = "BlazorApp.sln", Extension = ".sln", Size = 1_200, Modified = new(2025, 10, 1) },
        new() { Id = "f34", Name = "global.json", Extension = ".json", Size = 85, Modified = new(2025, 10, 1) }
    };

    private static List<Place> worldData = AssignIds(BuildWorldData());

    private static List<Place> AssignIds(List<Place> places)
    {
        var counter = 0;
        AssignIdsRecursive(places, ref counter);
        return places;
    }

    private static void AssignIdsRecursive(List<Place>? places, ref int counter)
    {
        if (places == null)
        {
            return;
        }

        foreach (var place in places)
        {
            place.Id = $"p{counter++}";
            AssignIdsRecursive(place.Children, ref counter);
        }
    }

    private static List<Place> BuildWorldData() => new()
    {
        new()
        {
            Name = "Africa", Type = "Continent", Population = 1_460_000_000, Code = "AF",
            Children = new()
            {
                new()
                {
                    Name = "Nigeria", Type = "Country", Population = 223_800_000, Code = "NG",
                    Children = new()
                    {
                        new() { Name = "Lagos", Type = "State", Population = 15_400_000, Code = "LA", Children = new() { new() { Name = "Lagos", Type = "City", Population = 9_600_000, Code = "LOS" }, new() { Name = "Ikeja", Type = "City", Population = 3_200_000, Code = "IKJ" }, new() { Name = "Epe", Type = "City", Population = 860_000, Code = "EPE" } } },
                        new() { Name = "Kano", Type = "State", Population = 13_000_000, Code = "KN", Children = new() { new() { Name = "Kano", Type = "City", Population = 4_100_000, Code = "KAN" }, new() { Name = "Wudil", Type = "City", Population = 310_000, Code = "WDL" } } },
                        new() { Name = "Rivers", Type = "State", Population = 7_300_000, Code = "RI", Children = new() { new() { Name = "Port Harcourt", Type = "City", Population = 3_200_000, Code = "PHC" }, new() { Name = "Obio-Akpor", Type = "City", Population = 878_000, Code = "OAK" } } },
                        new() { Name = "Oyo", Type = "State", Population = 8_400_000, Code = "OY", Children = new() { new() { Name = "Ibadan", Type = "City", Population = 3_600_000, Code = "IBD" }, new() { Name = "Ogbomosho", Type = "City", Population = 318_000, Code = "OGB" } } },
                        new() { Name = "Abuja FCT", Type = "Territory", Population = 3_600_000, Code = "FC", Children = new() { new() { Name = "Abuja", Type = "City", Population = 3_300_000, Code = "ABV" } } }
                    }
                },
                new()
                {
                    Name = "South Africa", Type = "Country", Population = 60_400_000, Code = "ZA",
                    Children = new()
                    {
                        new() { Name = "Gauteng", Type = "Province", Population = 16_100_000, Code = "GP", Children = new() { new() { Name = "Johannesburg", Type = "City", Population = 5_800_000, Code = "JNB" }, new() { Name = "Pretoria", Type = "City", Population = 2_600_000, Code = "PRY" }, new() { Name = "Ekurhuleni", Type = "City", Population = 3_900_000, Code = "EKU" }, new() { Name = "Soweto", Type = "City", Population = 1_300_000, Code = "SOW" } } },
                        new() { Name = "Western Cape", Type = "Province", Population = 7_400_000, Code = "WC", Children = new() { new() { Name = "Cape Town", Type = "City", Population = 4_800_000, Code = "CPT" }, new() { Name = "Stellenbosch", Type = "City", Population = 180_000, Code = "STB" } } },
                        new() { Name = "KwaZulu-Natal", Type = "Province", Population = 11_500_000, Code = "KZN", Children = new() { new() { Name = "Durban", Type = "City", Population = 3_900_000, Code = "DUR" }, new() { Name = "Pietermaritzburg", Type = "City", Population = 750_000, Code = "PMB" } } },
                        new() { Name = "Eastern Cape", Type = "Province", Population = 6_700_000, Code = "EC", Children = new() { new() { Name = "Gqeberha", Type = "City", Population = 1_300_000, Code = "PLZ" }, new() { Name = "East London", Type = "City", Population = 478_000, Code = "ELS" } } }
                    }
                },
                new()
                {
                    Name = "Egypt", Type = "Country", Population = 104_300_000, Code = "EG",
                    Children = new()
                    {
                        new() { Name = "Cairo", Type = "Governorate", Population = 10_100_000, Code = "C", Children = new() { new() { Name = "Cairo", Type = "City", Population = 9_500_000, Code = "CAI" } } },
                        new() { Name = "Alexandria", Type = "Governorate", Population = 5_400_000, Code = "ALX", Children = new() { new() { Name = "Alexandria", Type = "City", Population = 5_200_000, Code = "ALY" } } },
                        new() { Name = "Giza", Type = "Governorate", Population = 9_100_000, Code = "GZ", Children = new() { new() { Name = "Giza", Type = "City", Population = 4_200_000, Code = "GIZ" }, new() { Name = "6th of October City", Type = "City", Population = 1_100_000, Code = "6OC" } } }
                    }
                },
                new()
                {
                    Name = "Kenya", Type = "Country", Population = 54_000_000, Code = "KE",
                    Children = new()
                    {
                        new() { Name = "Nairobi", Type = "County", Population = 4_700_000, Code = "047", Children = new() { new() { Name = "Nairobi", Type = "City", Population = 4_400_000, Code = "NBO" } } },
                        new() { Name = "Mombasa", Type = "County", Population = 1_200_000, Code = "001", Children = new() { new() { Name = "Mombasa", Type = "City", Population = 1_200_000, Code = "MBA" } } },
                        new() { Name = "Kisumu", Type = "County", Population = 1_200_000, Code = "042", Children = new() { new() { Name = "Kisumu", Type = "City", Population = 610_000, Code = "KIS" } } }
                    }
                },
                new()
                {
                    Name = "Ethiopia", Type = "Country", Population = 126_500_000, Code = "ET",
                    Children = new()
                    {
                        new() { Name = "Addis Ababa", Type = "Region", Population = 5_300_000, Code = "AA", Children = new() { new() { Name = "Addis Ababa", Type = "City", Population = 5_200_000, Code = "ADD" } } },
                        new() { Name = "Oromia", Type = "Region", Population = 40_000_000, Code = "OR", Children = new() { new() { Name = "Adama", Type = "City", Population = 450_000, Code = "ADM" }, new() { Name = "Jimma", Type = "City", Population = 207_000, Code = "JIM" } } }
                    }
                },
                new()
                {
                    Name = "Ghana", Type = "Country", Population = 33_500_000, Code = "GH",
                    Children = new()
                    {
                        new() { Name = "Greater Accra", Type = "Region", Population = 5_400_000, Code = "AA", Children = new() { new() { Name = "Accra", Type = "City", Population = 2_600_000, Code = "ACC" }, new() { Name = "Tema", Type = "City", Population = 400_000, Code = "TEM" } } },
                        new() { Name = "Ashanti", Type = "Region", Population = 5_800_000, Code = "AH", Children = new() { new() { Name = "Kumasi", Type = "City", Population = 3_400_000, Code = "KMS" } } }
                    }
                },
                new()
                {
                    Name = "Tanzania", Type = "Country", Population = 65_500_000, Code = "TZ",
                    Children = new()
                    {
                        new() { Name = "Dar es Salaam", Type = "Region", Population = 5_800_000, Code = "DS", Children = new() { new() { Name = "Dar es Salaam", Type = "City", Population = 5_400_000, Code = "DAR" } } },
                        new() { Name = "Dodoma", Type = "Region", Population = 2_600_000, Code = "DO", Children = new() { new() { Name = "Dodoma", Type = "City", Population = 620_000, Code = "DOD" } } }
                    }
                },
                new()
                {
                    Name = "Morocco", Type = "Country", Population = 37_500_000, Code = "MA",
                    Children = new()
                    {
                        new() { Name = "Casablanca-Settat", Type = "Region", Population = 7_300_000, Code = "CS", Children = new() { new() { Name = "Casablanca", Type = "City", Population = 3_700_000, Code = "CAS" }, new() { Name = "Mohammedia", Type = "City", Population = 450_000, Code = "MOH" } } },
                        new() { Name = "Rabat-Salé-Kénitra", Type = "Region", Population = 4_800_000, Code = "RK", Children = new() { new() { Name = "Rabat", Type = "City", Population = 580_000, Code = "RBA" }, new() { Name = "Salé", Type = "City", Population = 980_000, Code = "SLE" } } },
                        new() { Name = "Marrakech-Safi", Type = "Region", Population = 4_700_000, Code = "MS", Children = new() { new() { Name = "Marrakech", Type = "City", Population = 1_000_000, Code = "RAK" } } }
                    }
                },
                new()
                {
                    Name = "Algeria", Type = "Country", Population = 45_600_000, Code = "DZ",
                    Children = new()
                    {
                        new() { Name = "Algiers", Type = "Province", Population = 3_300_000, Code = "16", Children = new() { new() { Name = "Algiers", Type = "City", Population = 2_800_000, Code = "ALG" } } },
                        new() { Name = "Oran", Type = "Province", Population = 1_600_000, Code = "31", Children = new() { new() { Name = "Oran", Type = "City", Population = 1_500_000, Code = "ORN" } } }
                    }
                },
                new()
                {
                    Name = "Uganda", Type = "Country", Population = 47_200_000, Code = "UG",
                    Children = new()
                    {
                        new() { Name = "Central", Type = "Region", Population = 11_000_000, Code = "C", Children = new() { new() { Name = "Kampala", Type = "City", Population = 1_700_000, Code = "KLA" } } }
                    }
                },
                new()
                {
                    Name = "Senegal", Type = "Country", Population = 17_200_000, Code = "SN",
                    Children = new()
                    {
                        new() { Name = "Dakar", Type = "Region", Population = 4_000_000, Code = "DK", Children = new() { new() { Name = "Dakar", Type = "City", Population = 1_200_000, Code = "DSS" } } }
                    }
                },
                new()
                {
                    Name = "Mozambique", Type = "Country", Population = 33_000_000, Code = "MZ",
                    Children = new()
                    {
                        new() { Name = "Maputo", Type = "Province", Population = 2_000_000, Code = "MPM", Children = new() { new() { Name = "Maputo", Type = "City", Population = 1_100_000, Code = "MPM" } } }
                    }
                }
            }
        },
        new()
        {
            Name = "Asia", Type = "Continent", Population = 4_750_000_000, Code = "AS",
            Children = new()
            {
                new()
                {
                    Name = "China", Type = "Country", Population = 1_425_000_000, Code = "CN",
                    Children = new()
                    {
                        new() { Name = "Beijing", Type = "Municipality", Population = 21_500_000, Code = "BJ", Children = new() { new() { Name = "Beijing", Type = "City", Population = 21_500_000, Code = "PEK" } } },
                        new() { Name = "Shanghai", Type = "Municipality", Population = 24_900_000, Code = "SH", Children = new() { new() { Name = "Shanghai", Type = "City", Population = 24_900_000, Code = "SHA" } } },
                        new() { Name = "Guangdong", Type = "Province", Population = 126_000_000, Code = "GD", Children = new() { new() { Name = "Guangzhou", Type = "City", Population = 18_700_000, Code = "CAN" }, new() { Name = "Shenzhen", Type = "City", Population = 17_600_000, Code = "SZX" }, new() { Name = "Dongguan", Type = "City", Population = 10_400_000, Code = "DGG" }, new() { Name = "Foshan", Type = "City", Population = 9_500_000, Code = "FOS" }, new() { Name = "Zhuhai", Type = "City", Population = 2_400_000, Code = "ZUH" } } },
                        new() { Name = "Sichuan", Type = "Province", Population = 83_700_000, Code = "SC", Children = new() { new() { Name = "Chengdu", Type = "City", Population = 16_300_000, Code = "CTU" }, new() { Name = "Mianyang", Type = "City", Population = 4_900_000, Code = "MIG" } } },
                        new() { Name = "Jiangsu", Type = "Province", Population = 85_000_000, Code = "JS", Children = new() { new() { Name = "Nanjing", Type = "City", Population = 9_300_000, Code = "NKG" }, new() { Name = "Suzhou", Type = "City", Population = 12_700_000, Code = "SZV" }, new() { Name = "Wuxi", Type = "City", Population = 7_500_000, Code = "WUX" } } },
                        new() { Name = "Zhejiang", Type = "Province", Population = 65_800_000, Code = "ZJ", Children = new() { new() { Name = "Hangzhou", Type = "City", Population = 12_200_000, Code = "HGH" }, new() { Name = "Ningbo", Type = "City", Population = 9_400_000, Code = "NGB" }, new() { Name = "Wenzhou", Type = "City", Population = 9_300_000, Code = "WNZ" } } },
                        new() { Name = "Hubei", Type = "Province", Population = 57_800_000, Code = "HB", Children = new() { new() { Name = "Wuhan", Type = "City", Population = 12_300_000, Code = "WUH" } } },
                        new() { Name = "Hunan", Type = "Province", Population = 66_400_000, Code = "HN", Children = new() { new() { Name = "Changsha", Type = "City", Population = 10_500_000, Code = "CSX" } } },
                        new() { Name = "Henan", Type = "Province", Population = 98_800_000, Code = "HA", Children = new() { new() { Name = "Zhengzhou", Type = "City", Population = 12_700_000, Code = "CGO" }, new() { Name = "Luoyang", Type = "City", Population = 7_000_000, Code = "LYA" } } },
                        new() { Name = "Shandong", Type = "Province", Population = 101_500_000, Code = "SD", Children = new() { new() { Name = "Jinan", Type = "City", Population = 9_200_000, Code = "TNA" }, new() { Name = "Qingdao", Type = "City", Population = 10_100_000, Code = "TAO" } } },
                        new() { Name = "Fujian", Type = "Province", Population = 41_500_000, Code = "FJ", Children = new() { new() { Name = "Fuzhou", Type = "City", Population = 8_300_000, Code = "FOC" }, new() { Name = "Xiamen", Type = "City", Population = 5_300_000, Code = "XMN" } } },
                        new() { Name = "Chongqing", Type = "Municipality", Population = 32_100_000, Code = "CQ", Children = new() { new() { Name = "Chongqing", Type = "City", Population = 32_100_000, Code = "CKG" } } }
                    }
                },
                new()
                {
                    Name = "India", Type = "Country", Population = 1_440_000_000, Code = "IN",
                    Children = new()
                    {
                        new() { Name = "Maharashtra", Type = "State", Population = 124_000_000, Code = "MH", Children = new() { new() { Name = "Mumbai", Type = "City", Population = 20_700_000, Code = "BOM" }, new() { Name = "Pune", Type = "City", Population = 7_400_000, Code = "PNQ" }, new() { Name = "Nagpur", Type = "City", Population = 2_900_000, Code = "NAG" }, new() { Name = "Nashik", Type = "City", Population = 2_100_000, Code = "NSK" } } },
                        new() { Name = "Uttar Pradesh", Type = "State", Population = 231_000_000, Code = "UP", Children = new() { new() { Name = "Lucknow", Type = "City", Population = 3_600_000, Code = "LKO" }, new() { Name = "Kanpur", Type = "City", Population = 3_100_000, Code = "KNU" }, new() { Name = "Agra", Type = "City", Population = 2_200_000, Code = "AGR" }, new() { Name = "Varanasi", Type = "City", Population = 1_500_000, Code = "VNS" }, new() { Name = "Prayagraj", Type = "City", Population = 1_400_000, Code = "IXD" } } },
                        new() { Name = "Karnataka", Type = "State", Population = 67_600_000, Code = "KA", Children = new() { new() { Name = "Bengaluru", Type = "City", Population = 13_200_000, Code = "BLR" }, new() { Name = "Mysuru", Type = "City", Population = 1_200_000, Code = "MYQ" } } },
                        new() { Name = "Tamil Nadu", Type = "State", Population = 77_800_000, Code = "TN", Children = new() { new() { Name = "Chennai", Type = "City", Population = 10_900_000, Code = "MAA" }, new() { Name = "Coimbatore", Type = "City", Population = 2_200_000, Code = "CJB" }, new() { Name = "Madurai", Type = "City", Population = 1_500_000, Code = "IXM" } } },
                        new() { Name = "Delhi", Type = "Territory", Population = 19_800_000, Code = "DL", Children = new() { new() { Name = "New Delhi", Type = "City", Population = 16_800_000, Code = "DEL" } } },
                        new() { Name = "Gujarat", Type = "State", Population = 63_900_000, Code = "GJ", Children = new() { new() { Name = "Ahmedabad", Type = "City", Population = 8_500_000, Code = "AMD" }, new() { Name = "Surat", Type = "City", Population = 7_800_000, Code = "STV" }, new() { Name = "Vadodara", Type = "City", Population = 2_200_000, Code = "BDQ" } } },
                        new() { Name = "West Bengal", Type = "State", Population = 99_100_000, Code = "WB", Children = new() { new() { Name = "Kolkata", Type = "City", Population = 15_100_000, Code = "CCU" }, new() { Name = "Howrah", Type = "City", Population = 1_500_000, Code = "HWH" } } },
                        new() { Name = "Rajasthan", Type = "State", Population = 79_500_000, Code = "RJ", Children = new() { new() { Name = "Jaipur", Type = "City", Population = 3_900_000, Code = "JAI" }, new() { Name = "Jodhpur", Type = "City", Population = 1_400_000, Code = "JDH" }, new() { Name = "Udaipur", Type = "City", Population = 600_000, Code = "UDR" } } },
                        new() { Name = "Telangana", Type = "State", Population = 38_000_000, Code = "TG", Children = new() { new() { Name = "Hyderabad", Type = "City", Population = 10_500_000, Code = "HYD" }, new() { Name = "Warangal", Type = "City", Population = 830_000, Code = "WGL" } } },
                        new() { Name = "Kerala", Type = "State", Population = 35_600_000, Code = "KL", Children = new() { new() { Name = "Thiruvananthapuram", Type = "City", Population = 1_700_000, Code = "TRV" }, new() { Name = "Kochi", Type = "City", Population = 2_100_000, Code = "COK" } } },
                        new() { Name = "Punjab", Type = "State", Population = 30_500_000, Code = "PB", Children = new() { new() { Name = "Ludhiana", Type = "City", Population = 1_800_000, Code = "LUH" }, new() { Name = "Amritsar", Type = "City", Population = 1_200_000, Code = "ATQ" } } },
                        new() { Name = "Madhya Pradesh", Type = "State", Population = 85_400_000, Code = "MP", Children = new() { new() { Name = "Indore", Type = "City", Population = 2_800_000, Code = "IDR" }, new() { Name = "Bhopal", Type = "City", Population = 2_400_000, Code = "BHO" } } }
                    }
                },
                new()
                {
                    Name = "Japan", Type = "Country", Population = 125_100_000, Code = "JP",
                    Children = new()
                    {
                        new() { Name = "Tokyo", Type = "Prefecture", Population = 14_000_000, Code = "13", Children = new() { new() { Name = "Tokyo", Type = "City", Population = 13_960_000, Code = "TYO" }, new() { Name = "Hachioji", Type = "City", Population = 577_000, Code = "HCJ" } } },
                        new() { Name = "Osaka", Type = "Prefecture", Population = 8_800_000, Code = "27", Children = new() { new() { Name = "Osaka", Type = "City", Population = 2_800_000, Code = "OSA" }, new() { Name = "Sakai", Type = "City", Population = 826_000, Code = "SKI" } } },
                        new() { Name = "Kanagawa", Type = "Prefecture", Population = 9_200_000, Code = "14", Children = new() { new() { Name = "Yokohama", Type = "City", Population = 3_700_000, Code = "YOK" }, new() { Name = "Kawasaki", Type = "City", Population = 1_500_000, Code = "KWS" } } },
                        new() { Name = "Aichi", Type = "Prefecture", Population = 7_500_000, Code = "23", Children = new() { new() { Name = "Nagoya", Type = "City", Population = 2_300_000, Code = "NGO" }, new() { Name = "Toyota", Type = "City", Population = 422_000, Code = "TYT" } } },
                        new() { Name = "Hokkaido", Type = "Prefecture", Population = 5_200_000, Code = "01", Children = new() { new() { Name = "Sapporo", Type = "City", Population = 1_970_000, Code = "CTS" }, new() { Name = "Asahikawa", Type = "City", Population = 329_000, Code = "AKJ" } } },
                        new() { Name = "Fukuoka", Type = "Prefecture", Population = 5_100_000, Code = "40", Children = new() { new() { Name = "Fukuoka", Type = "City", Population = 1_600_000, Code = "FUK" }, new() { Name = "Kitakyushu", Type = "City", Population = 939_000, Code = "KKJ" } } }
                    }
                },
                new()
                {
                    Name = "Indonesia", Type = "Country", Population = 277_500_000, Code = "ID",
                    Children = new()
                    {
                        new() { Name = "Jakarta", Type = "Province", Population = 10_600_000, Code = "JK", Children = new() { new() { Name = "Jakarta", Type = "City", Population = 10_600_000, Code = "JKT" } } },
                        new() { Name = "West Java", Type = "Province", Population = 49_000_000, Code = "JB", Children = new() { new() { Name = "Bandung", Type = "City", Population = 2_500_000, Code = "BDO" }, new() { Name = "Bekasi", Type = "City", Population = 2_500_000, Code = "BKS" }, new() { Name = "Depok", Type = "City", Population = 2_100_000, Code = "DPK" }, new() { Name = "Bogor", Type = "City", Population = 1_100_000, Code = "BOG" } } },
                        new() { Name = "East Java", Type = "Province", Population = 40_700_000, Code = "JI", Children = new() { new() { Name = "Surabaya", Type = "City", Population = 2_900_000, Code = "SUB" }, new() { Name = "Malang", Type = "City", Population = 900_000, Code = "MLG" } } },
                        new() { Name = "Central Java", Type = "Province", Population = 36_700_000, Code = "JT", Children = new() { new() { Name = "Semarang", Type = "City", Population = 1_700_000, Code = "SRG" }, new() { Name = "Solo", Type = "City", Population = 520_000, Code = "SOC" } } },
                        new() { Name = "Bali", Type = "Province", Population = 4_300_000, Code = "BA", Children = new() { new() { Name = "Denpasar", Type = "City", Population = 950_000, Code = "DPS" } } }
                    }
                },
                new()
                {
                    Name = "South Korea", Type = "Country", Population = 51_700_000, Code = "KR",
                    Children = new()
                    {
                        new() { Name = "Seoul", Type = "City", Population = 9_700_000, Code = "11", Children = new() { new() { Name = "Seoul", Type = "City", Population = 9_700_000, Code = "SEL" } } },
                        new() { Name = "Gyeonggi", Type = "Province", Population = 13_600_000, Code = "41", Children = new() { new() { Name = "Suwon", Type = "City", Population = 1_200_000, Code = "SWN" }, new() { Name = "Incheon", Type = "City", Population = 2_900_000, Code = "ICN" }, new() { Name = "Seongnam", Type = "City", Population = 930_000, Code = "SNM" } } },
                        new() { Name = "Busan", Type = "City", Population = 3_400_000, Code = "26", Children = new() { new() { Name = "Busan", Type = "City", Population = 3_400_000, Code = "PUS" } } },
                        new() { Name = "Daegu", Type = "City", Population = 2_400_000, Code = "27", Children = new() { new() { Name = "Daegu", Type = "City", Population = 2_400_000, Code = "TAE" } } }
                    }
                },
                new()
                {
                    Name = "Turkey", Type = "Country", Population = 85_300_000, Code = "TR",
                    Children = new()
                    {
                        new() { Name = "Istanbul", Type = "Province", Population = 15_800_000, Code = "34", Children = new() { new() { Name = "Istanbul", Type = "City", Population = 15_600_000, Code = "IST" } } },
                        new() { Name = "Ankara", Type = "Province", Population = 5_700_000, Code = "06", Children = new() { new() { Name = "Ankara", Type = "City", Population = 5_500_000, Code = "ANK" } } },
                        new() { Name = "Izmir", Type = "Province", Population = 4_400_000, Code = "35", Children = new() { new() { Name = "Izmir", Type = "City", Population = 4_400_000, Code = "IZM" } } },
                        new() { Name = "Antalya", Type = "Province", Population = 2_600_000, Code = "07", Children = new() { new() { Name = "Antalya", Type = "City", Population = 2_500_000, Code = "AYT" } } }
                    }
                },
                new()
                {
                    Name = "Thailand", Type = "Country", Population = 71_800_000, Code = "TH",
                    Children = new()
                    {
                        new() { Name = "Bangkok", Type = "Province", Population = 10_500_000, Code = "10", Children = new() { new() { Name = "Bangkok", Type = "City", Population = 10_500_000, Code = "BKK" } } },
                        new() { Name = "Chiang Mai", Type = "Province", Population = 1_800_000, Code = "50", Children = new() { new() { Name = "Chiang Mai", Type = "City", Population = 1_200_000, Code = "CNX" } } },
                        new() { Name = "Phuket", Type = "Province", Population = 416_000, Code = "83", Children = new() { new() { Name = "Phuket", Type = "City", Population = 400_000, Code = "HKT" } } }
                    }
                },
                new()
                {
                    Name = "Vietnam", Type = "Country", Population = 99_500_000, Code = "VN",
                    Children = new()
                    {
                        new() { Name = "Ho Chi Minh City", Type = "Municipality", Population = 9_300_000, Code = "SG", Children = new() { new() { Name = "Ho Chi Minh City", Type = "City", Population = 9_300_000, Code = "SGN" } } },
                        new() { Name = "Hanoi", Type = "Municipality", Population = 8_200_000, Code = "HN", Children = new() { new() { Name = "Hanoi", Type = "City", Population = 8_200_000, Code = "HAN" } } },
                        new() { Name = "Da Nang", Type = "Municipality", Population = 1_200_000, Code = "DN", Children = new() { new() { Name = "Da Nang", Type = "City", Population = 1_200_000, Code = "DAD" } } }
                    }
                },
                new()
                {
                    Name = "Philippines", Type = "Country", Population = 115_600_000, Code = "PH",
                    Children = new()
                    {
                        new() { Name = "Metro Manila", Type = "Region", Population = 13_900_000, Code = "NCR", Children = new() { new() { Name = "Manila", Type = "City", Population = 1_800_000, Code = "MNL" }, new() { Name = "Quezon City", Type = "City", Population = 2_900_000, Code = "QEZ" }, new() { Name = "Makati", Type = "City", Population = 630_000, Code = "MKT" } } },
                        new() { Name = "Cebu", Type = "Province", Population = 5_000_000, Code = "CEB", Children = new() { new() { Name = "Cebu City", Type = "City", Population = 1_000_000, Code = "CEB" } } },
                        new() { Name = "Davao del Sur", Type = "Province", Population = 1_000_000, Code = "DVO", Children = new() { new() { Name = "Davao City", Type = "City", Population = 1_800_000, Code = "DVO" } } }
                    }
                },
                new()
                {
                    Name = "Pakistan", Type = "Country", Population = 231_400_000, Code = "PK",
                    Children = new()
                    {
                        new() { Name = "Punjab", Type = "Province", Population = 127_000_000, Code = "PB", Children = new() { new() { Name = "Lahore", Type = "City", Population = 13_000_000, Code = "LHE" }, new() { Name = "Faisalabad", Type = "City", Population = 3_600_000, Code = "LYP" }, new() { Name = "Rawalpindi", Type = "City", Population = 2_200_000, Code = "RWP" } } },
                        new() { Name = "Sindh", Type = "Province", Population = 55_700_000, Code = "SD", Children = new() { new() { Name = "Karachi", Type = "City", Population = 16_800_000, Code = "KHI" }, new() { Name = "Hyderabad", Type = "City", Population = 2_000_000, Code = "HDD" } } },
                        new() { Name = "Islamabad", Type = "Territory", Population = 1_200_000, Code = "IS", Children = new() { new() { Name = "Islamabad", Type = "City", Population = 1_200_000, Code = "ISB" } } }
                    }
                },
                new()
                {
                    Name = "Bangladesh", Type = "Country", Population = 172_000_000, Code = "BD",
                    Children = new()
                    {
                        new() { Name = "Dhaka", Type = "Division", Population = 44_000_000, Code = "C", Children = new() { new() { Name = "Dhaka", Type = "City", Population = 22_000_000, Code = "DAC" } } },
                        new() { Name = "Chittagong", Type = "Division", Population = 34_000_000, Code = "B", Children = new() { new() { Name = "Chittagong", Type = "City", Population = 5_200_000, Code = "CGP" } } }
                    }
                },
                new()
                {
                    Name = "Saudi Arabia", Type = "Country", Population = 36_900_000, Code = "SA",
                    Children = new()
                    {
                        new() { Name = "Riyadh", Type = "Region", Population = 8_600_000, Code = "01", Children = new() { new() { Name = "Riyadh", Type = "City", Population = 7_700_000, Code = "RUH" } } },
                        new() { Name = "Makkah", Type = "Region", Population = 9_000_000, Code = "02", Children = new() { new() { Name = "Jeddah", Type = "City", Population = 4_700_000, Code = "JED" }, new() { Name = "Makkah", Type = "City", Population = 2_000_000, Code = "MKH" } } }
                    }
                }
            }
        },
        new()
        {
            Name = "Europe", Type = "Continent", Population = 750_000_000, Code = "EU",
            Children = new()
            {
                new()
                {
                    Name = "Germany", Type = "Country", Population = 84_400_000, Code = "DE",
                    Children = new()
                    {
                        new() { Name = "Bavaria", Type = "State", Population = 13_200_000, Code = "BY", Children = new() { new() { Name = "Munich", Type = "City", Population = 1_500_000, Code = "MUC" }, new() { Name = "Nuremberg", Type = "City", Population = 520_000, Code = "NUE" }, new() { Name = "Augsburg", Type = "City", Population = 300_000, Code = "AGB" }, new() { Name = "Regensburg", Type = "City", Population = 157_000, Code = "RBG" } } },
                        new() { Name = "North Rhine-Westphalia", Type = "State", Population = 18_100_000, Code = "NW", Children = new() { new() { Name = "Cologne", Type = "City", Population = 1_080_000, Code = "CGN" }, new() { Name = "Düsseldorf", Type = "City", Population = 620_000, Code = "DUS" }, new() { Name = "Dortmund", Type = "City", Population = 588_000, Code = "DTM" }, new() { Name = "Essen", Type = "City", Population = 582_000, Code = "ESS" }, new() { Name = "Duisburg", Type = "City", Population = 498_000, Code = "DUI" }, new() { Name = "Bonn", Type = "City", Population = 330_000, Code = "BNJ" } } },
                        new() { Name = "Baden-Württemberg", Type = "State", Population = 11_100_000, Code = "BW", Children = new() { new() { Name = "Stuttgart", Type = "City", Population = 635_000, Code = "STR" }, new() { Name = "Mannheim", Type = "City", Population = 310_000, Code = "MHG" }, new() { Name = "Karlsruhe", Type = "City", Population = 308_000, Code = "KAE" }, new() { Name = "Freiburg", Type = "City", Population = 230_000, Code = "FBG" }, new() { Name = "Heidelberg", Type = "City", Population = 160_000, Code = "HDB" } } },
                        new() { Name = "Berlin", Type = "State", Population = 3_700_000, Code = "BE", Children = new() { new() { Name = "Berlin", Type = "City", Population = 3_700_000, Code = "BER" } } },
                        new() { Name = "Hamburg", Type = "State", Population = 1_900_000, Code = "HH", Children = new() { new() { Name = "Hamburg", Type = "City", Population = 1_900_000, Code = "HAM" } } },
                        new() { Name = "Hesse", Type = "State", Population = 6_300_000, Code = "HE", Children = new() { new() { Name = "Frankfurt", Type = "City", Population = 760_000, Code = "FRA" }, new() { Name = "Wiesbaden", Type = "City", Population = 280_000, Code = "WIE" } } },
                        new() { Name = "Lower Saxony", Type = "State", Population = 8_000_000, Code = "NI", Children = new() { new() { Name = "Hanover", Type = "City", Population = 535_000, Code = "HAJ" }, new() { Name = "Braunschweig", Type = "City", Population = 249_000, Code = "BWE" } } },
                        new() { Name = "Saxony", Type = "State", Population = 4_100_000, Code = "SN", Children = new() { new() { Name = "Leipzig", Type = "City", Population = 600_000, Code = "LEJ" }, new() { Name = "Dresden", Type = "City", Population = 560_000, Code = "DRS" } } }
                    }
                },
                new()
                {
                    Name = "United Kingdom", Type = "Country", Population = 67_700_000, Code = "GB",
                    Children = new()
                    {
                        new() { Name = "England", Type = "Country", Population = 56_500_000, Code = "ENG", Children = new() { new() { Name = "London", Type = "City", Population = 8_800_000, Code = "LON" }, new() { Name = "Birmingham", Type = "City", Population = 1_100_000, Code = "BHX" }, new() { Name = "Manchester", Type = "City", Population = 550_000, Code = "MAN" }, new() { Name = "Leeds", Type = "City", Population = 503_000, Code = "LDS" }, new() { Name = "Liverpool", Type = "City", Population = 486_000, Code = "LPL" }, new() { Name = "Bristol", Type = "City", Population = 467_000, Code = "BRS" }, new() { Name = "Sheffield", Type = "City", Population = 556_000, Code = "SHF" }, new() { Name = "Newcastle", Type = "City", Population = 300_000, Code = "NCL" }, new() { Name = "Nottingham", Type = "City", Population = 321_000, Code = "NQT" }, new() { Name = "Leicester", Type = "City", Population = 368_000, Code = "LCR" }, new() { Name = "Brighton", Type = "City", Population = 290_000, Code = "BSH" }, new() { Name = "Cambridge", Type = "City", Population = 145_000, Code = "CBG" } } },
                        new() { Name = "Scotland", Type = "Country", Population = 5_500_000, Code = "SCT", Children = new() { new() { Name = "Edinburgh", Type = "City", Population = 530_000, Code = "EDI" }, new() { Name = "Glasgow", Type = "City", Population = 635_000, Code = "GLA" }, new() { Name = "Aberdeen", Type = "City", Population = 200_000, Code = "ABZ" } } },
                        new() { Name = "Wales", Type = "Country", Population = 3_100_000, Code = "WLS", Children = new() { new() { Name = "Cardiff", Type = "City", Population = 362_000, Code = "CWL" }, new() { Name = "Swansea", Type = "City", Population = 246_000, Code = "SWA" } } },
                        new() { Name = "Northern Ireland", Type = "Province", Population = 1_900_000, Code = "NIR", Children = new() { new() { Name = "Belfast", Type = "City", Population = 343_000, Code = "BFS" } } }
                    }
                },
                new()
                {
                    Name = "France", Type = "Country", Population = 68_000_000, Code = "FR",
                    Children = new()
                    {
                        new() { Name = "Île-de-France", Type = "Region", Population = 12_300_000, Code = "IDF", Children = new() { new() { Name = "Paris", Type = "City", Population = 2_100_000, Code = "CDG" }, new() { Name = "Boulogne-Billancourt", Type = "City", Population = 120_000, Code = "BOU" }, new() { Name = "Versailles", Type = "City", Population = 85_000, Code = "VER" } } },
                        new() { Name = "Auvergne-Rhône-Alpes", Type = "Region", Population = 8_100_000, Code = "ARA", Children = new() { new() { Name = "Lyon", Type = "City", Population = 522_000, Code = "LYS" }, new() { Name = "Grenoble", Type = "City", Population = 158_000, Code = "GNB" }, new() { Name = "Saint-Étienne", Type = "City", Population = 173_000, Code = "EBU" } } },
                        new() { Name = "Provence-Alpes-Côte d'Azur", Type = "Region", Population = 5_100_000, Code = "PAC", Children = new() { new() { Name = "Marseille", Type = "City", Population = 870_000, Code = "MRS" }, new() { Name = "Nice", Type = "City", Population = 342_000, Code = "NCE" }, new() { Name = "Toulon", Type = "City", Population = 180_000, Code = "TLN" } } },
                        new() { Name = "Occitanie", Type = "Region", Population = 5_900_000, Code = "OCC", Children = new() { new() { Name = "Toulouse", Type = "City", Population = 493_000, Code = "TLS" }, new() { Name = "Montpellier", Type = "City", Population = 290_000, Code = "MPL" } } },
                        new() { Name = "Nouvelle-Aquitaine", Type = "Region", Population = 6_000_000, Code = "NAQ", Children = new() { new() { Name = "Bordeaux", Type = "City", Population = 257_000, Code = "BOD" }, new() { Name = "Limoges", Type = "City", Population = 130_000, Code = "LIG" } } },
                        new() { Name = "Hauts-de-France", Type = "Region", Population = 6_000_000, Code = "HDF", Children = new() { new() { Name = "Lille", Type = "City", Population = 233_000, Code = "LIL" }, new() { Name = "Amiens", Type = "City", Population = 135_000, Code = "AMI" } } }
                    }
                },
                new()
                {
                    Name = "Italy", Type = "Country", Population = 59_000_000, Code = "IT",
                    Children = new()
                    {
                        new() { Name = "Lombardy", Type = "Region", Population = 10_100_000, Code = "25", Children = new() { new() { Name = "Milan", Type = "City", Population = 1_400_000, Code = "MXP" }, new() { Name = "Bergamo", Type = "City", Population = 122_000, Code = "BGY" }, new() { Name = "Brescia", Type = "City", Population = 196_000, Code = "VBS" } } },
                        new() { Name = "Lazio", Type = "Region", Population = 5_700_000, Code = "62", Children = new() { new() { Name = "Rome", Type = "City", Population = 2_800_000, Code = "FCO" } } },
                        new() { Name = "Campania", Type = "Region", Population = 5_700_000, Code = "72", Children = new() { new() { Name = "Naples", Type = "City", Population = 960_000, Code = "NAP" } } },
                        new() { Name = "Veneto", Type = "Region", Population = 4_900_000, Code = "34", Children = new() { new() { Name = "Venice", Type = "City", Population = 260_000, Code = "VCE" }, new() { Name = "Verona", Type = "City", Population = 258_000, Code = "VRN" }, new() { Name = "Padua", Type = "City", Population = 214_000, Code = "QPA" } } },
                        new() { Name = "Tuscany", Type = "Region", Population = 3_700_000, Code = "52", Children = new() { new() { Name = "Florence", Type = "City", Population = 382_000, Code = "FLR" }, new() { Name = "Pisa", Type = "City", Population = 91_000, Code = "PSA" } } },
                        new() { Name = "Piedmont", Type = "Region", Population = 4_300_000, Code = "21", Children = new() { new() { Name = "Turin", Type = "City", Population = 870_000, Code = "TRN" } } }
                    }
                },
                new()
                {
                    Name = "Spain", Type = "Country", Population = 47_400_000, Code = "ES",
                    Children = new()
                    {
                        new() { Name = "Community of Madrid", Type = "Community", Population = 6_800_000, Code = "MD", Children = new() { new() { Name = "Madrid", Type = "City", Population = 3_300_000, Code = "MAD" }, new() { Name = "Móstoles", Type = "City", Population = 210_000, Code = "MOS" } } },
                        new() { Name = "Catalonia", Type = "Community", Population = 7_800_000, Code = "CT", Children = new() { new() { Name = "Barcelona", Type = "City", Population = 1_600_000, Code = "BCN" }, new() { Name = "Tarragona", Type = "City", Population = 132_000, Code = "TAR" }, new() { Name = "Girona", Type = "City", Population = 103_000, Code = "GRO" } } },
                        new() { Name = "Andalusia", Type = "Community", Population = 8_500_000, Code = "AN", Children = new() { new() { Name = "Seville", Type = "City", Population = 685_000, Code = "SVQ" }, new() { Name = "Málaga", Type = "City", Population = 578_000, Code = "AGP" }, new() { Name = "Granada", Type = "City", Population = 232_000, Code = "GRX" }, new() { Name = "Córdoba", Type = "City", Population = 325_000, Code = "ODB" } } },
                        new() { Name = "Valencian Community", Type = "Community", Population = 5_100_000, Code = "VC", Children = new() { new() { Name = "Valencia", Type = "City", Population = 800_000, Code = "VLC" }, new() { Name = "Alicante", Type = "City", Population = 337_000, Code = "ALC" } } },
                        new() { Name = "Basque Country", Type = "Community", Population = 2_200_000, Code = "PV", Children = new() { new() { Name = "Bilbao", Type = "City", Population = 346_000, Code = "BIO" }, new() { Name = "San Sebastián", Type = "City", Population = 188_000, Code = "EAS" } } }
                    }
                },
                new()
                {
                    Name = "Poland", Type = "Country", Population = 37_700_000, Code = "PL",
                    Children = new()
                    {
                        new() { Name = "Masovia", Type = "Voivodeship", Population = 5_500_000, Code = "MZ", Children = new() { new() { Name = "Warsaw", Type = "City", Population = 1_800_000, Code = "WAW" } } },
                        new() { Name = "Lesser Poland", Type = "Voivodeship", Population = 3_400_000, Code = "MA", Children = new() { new() { Name = "Kraków", Type = "City", Population = 800_000, Code = "KRK" } } },
                        new() { Name = "Lower Silesia", Type = "Voivodeship", Population = 2_900_000, Code = "DS", Children = new() { new() { Name = "Wrocław", Type = "City", Population = 673_000, Code = "WRO" } } },
                        new() { Name = "Greater Poland", Type = "Voivodeship", Population = 3_500_000, Code = "WP", Children = new() { new() { Name = "Poznań", Type = "City", Population = 534_000, Code = "POZ" } } },
                        new() { Name = "Pomerania", Type = "Voivodeship", Population = 2_300_000, Code = "PM", Children = new() { new() { Name = "Gdańsk", Type = "City", Population = 486_000, Code = "GDN" } } }
                    }
                },
                new()
                {
                    Name = "Netherlands", Type = "Country", Population = 17_800_000, Code = "NL",
                    Children = new()
                    {
                        new() { Name = "North Holland", Type = "Province", Population = 2_900_000, Code = "NH", Children = new() { new() { Name = "Amsterdam", Type = "City", Population = 920_000, Code = "AMS" }, new() { Name = "Haarlem", Type = "City", Population = 162_000, Code = "HRL" } } },
                        new() { Name = "South Holland", Type = "Province", Population = 3_700_000, Code = "ZH", Children = new() { new() { Name = "Rotterdam", Type = "City", Population = 655_000, Code = "RTM" }, new() { Name = "The Hague", Type = "City", Population = 548_000, Code = "HAG" }, new() { Name = "Leiden", Type = "City", Population = 124_000, Code = "LID" } } },
                        new() { Name = "North Brabant", Type = "Province", Population = 2_600_000, Code = "NB", Children = new() { new() { Name = "Eindhoven", Type = "City", Population = 238_000, Code = "EIN" }, new() { Name = "Tilburg", Type = "City", Population = 222_000, Code = "TLB" } } },
                        new() { Name = "Utrecht", Type = "Province", Population = 1_400_000, Code = "UT", Children = new() { new() { Name = "Utrecht", Type = "City", Population = 361_000, Code = "UTC" } } }
                    }
                },
                new()
                {
                    Name = "Sweden", Type = "Country", Population = 10_500_000, Code = "SE",
                    Children = new()
                    {
                        new() { Name = "Stockholm", Type = "County", Population = 2_400_000, Code = "AB", Children = new() { new() { Name = "Stockholm", Type = "City", Population = 980_000, Code = "ARN" } } },
                        new() { Name = "Västra Götaland", Type = "County", Population = 1_700_000, Code = "O", Children = new() { new() { Name = "Gothenburg", Type = "City", Population = 590_000, Code = "GOT" } } },
                        new() { Name = "Skåne", Type = "County", Population = 1_400_000, Code = "M", Children = new() { new() { Name = "Malmö", Type = "City", Population = 350_000, Code = "MMA" } } }
                    }
                },
                new()
                {
                    Name = "Portugal", Type = "Country", Population = 10_300_000, Code = "PT",
                    Children = new()
                    {
                        new() { Name = "Lisbon", Type = "District", Population = 2_800_000, Code = "11", Children = new() { new() { Name = "Lisbon", Type = "City", Population = 545_000, Code = "LIS" } } },
                        new() { Name = "Porto", Type = "District", Population = 1_700_000, Code = "13", Children = new() { new() { Name = "Porto", Type = "City", Population = 249_000, Code = "OPO" } } }
                    }
                },
                new()
                {
                    Name = "Switzerland", Type = "Country", Population = 8_800_000, Code = "CH",
                    Children = new()
                    {
                        new() { Name = "Zürich", Type = "Canton", Population = 1_600_000, Code = "ZH", Children = new() { new() { Name = "Zürich", Type = "City", Population = 420_000, Code = "ZRH" }, new() { Name = "Winterthur", Type = "City", Population = 115_000, Code = "WIN" } } },
                        new() { Name = "Bern", Type = "Canton", Population = 1_050_000, Code = "BE", Children = new() { new() { Name = "Bern", Type = "City", Population = 134_000, Code = "BRN" } } },
                        new() { Name = "Geneva", Type = "Canton", Population = 510_000, Code = "GE", Children = new() { new() { Name = "Geneva", Type = "City", Population = 203_000, Code = "GVA" } } }
                    }
                },
                new()
                {
                    Name = "Austria", Type = "Country", Population = 9_100_000, Code = "AT",
                    Children = new()
                    {
                        new() { Name = "Vienna", Type = "State", Population = 1_980_000, Code = "9", Children = new() { new() { Name = "Vienna", Type = "City", Population = 1_980_000, Code = "VIE" } } },
                        new() { Name = "Upper Austria", Type = "State", Population = 1_500_000, Code = "4", Children = new() { new() { Name = "Linz", Type = "City", Population = 207_000, Code = "LNZ" } } },
                        new() { Name = "Tyrol", Type = "State", Population = 760_000, Code = "7", Children = new() { new() { Name = "Innsbruck", Type = "City", Population = 132_000, Code = "INN" } } }
                    }
                },
                new()
                {
                    Name = "Norway", Type = "Country", Population = 5_500_000, Code = "NO",
                    Children = new()
                    {
                        new() { Name = "Oslo", Type = "County", Population = 700_000, Code = "03", Children = new() { new() { Name = "Oslo", Type = "City", Population = 700_000, Code = "OSL" } } },
                        new() { Name = "Vestland", Type = "County", Population = 640_000, Code = "46", Children = new() { new() { Name = "Bergen", Type = "City", Population = 285_000, Code = "BGO" } } }
                    }
                }
            }
        },
        new()
        {
            Name = "North America", Type = "Continent", Population = 580_000_000, Code = "NA",
            Children = new()
            {
                new()
                {
                    Name = "United States", Type = "Country", Population = 334_000_000, Code = "US",
                    Children = new()
                    {
                        new() { Name = "California", Type = "State", Population = 39_000_000, Code = "CA", Children = new() { new() { Name = "Los Angeles", Type = "City", Population = 3_900_000, Code = "LAX" }, new() { Name = "San Francisco", Type = "City", Population = 870_000, Code = "SFO" }, new() { Name = "San Diego", Type = "City", Population = 1_400_000, Code = "SAN" }, new() { Name = "San Jose", Type = "City", Population = 1_000_000, Code = "SJC" }, new() { Name = "Sacramento", Type = "City", Population = 525_000, Code = "SMF" }, new() { Name = "Fresno", Type = "City", Population = 542_000, Code = "FAT" }, new() { Name = "Oakland", Type = "City", Population = 430_000, Code = "OAK" }, new() { Name = "Long Beach", Type = "City", Population = 466_000, Code = "LGB" }, new() { Name = "Bakersfield", Type = "City", Population = 403_000, Code = "BFL" }, new() { Name = "Anaheim", Type = "City", Population = 350_000, Code = "ANA" }, new() { Name = "Santa Ana", Type = "City", Population = 310_000, Code = "SNA" }, new() { Name = "Riverside", Type = "City", Population = 314_000, Code = "RIV" } } },
                        new() { Name = "Texas", Type = "State", Population = 30_000_000, Code = "TX", Children = new() { new() { Name = "Houston", Type = "City", Population = 2_300_000, Code = "IAH" }, new() { Name = "San Antonio", Type = "City", Population = 1_500_000, Code = "SAT" }, new() { Name = "Dallas", Type = "City", Population = 1_300_000, Code = "DFW" }, new() { Name = "Austin", Type = "City", Population = 1_000_000, Code = "AUS" }, new() { Name = "Fort Worth", Type = "City", Population = 958_000, Code = "FTW" }, new() { Name = "El Paso", Type = "City", Population = 678_000, Code = "ELP" }, new() { Name = "Arlington", Type = "City", Population = 394_000, Code = "ARL" }, new() { Name = "Corpus Christi", Type = "City", Population = 317_000, Code = "CRP" }, new() { Name = "Plano", Type = "City", Population = 289_000, Code = "PLN" }, new() { Name = "Laredo", Type = "City", Population = 255_000, Code = "LRD" }, new() { Name = "Lubbock", Type = "City", Population = 264_000, Code = "LBB" } } },
                        new() { Name = "New York", Type = "State", Population = 19_700_000, Code = "NY", Children = new() { new() { Name = "New York City", Type = "City", Population = 8_300_000, Code = "NYC" }, new() { Name = "Buffalo", Type = "City", Population = 278_000, Code = "BUF" }, new() { Name = "Rochester", Type = "City", Population = 211_000, Code = "ROC" }, new() { Name = "Syracuse", Type = "City", Population = 148_000, Code = "SYR" }, new() { Name = "Albany", Type = "City", Population = 100_000, Code = "ALB" }, new() { Name = "Yonkers", Type = "City", Population = 211_000, Code = "YNK" } } },
                        new() { Name = "Florida", Type = "State", Population = 22_200_000, Code = "FL", Children = new() { new() { Name = "Miami", Type = "City", Population = 442_000, Code = "MIA" }, new() { Name = "Tampa", Type = "City", Population = 392_000, Code = "TPA" }, new() { Name = "Orlando", Type = "City", Population = 307_000, Code = "MCO" }, new() { Name = "Jacksonville", Type = "City", Population = 950_000, Code = "JAX" }, new() { Name = "St. Petersburg", Type = "City", Population = 258_000, Code = "PIE" }, new() { Name = "Fort Lauderdale", Type = "City", Population = 182_000, Code = "FLL" }, new() { Name = "Tallahassee", Type = "City", Population = 197_000, Code = "TLH" } } },
                        new() { Name = "Illinois", Type = "State", Population = 12_500_000, Code = "IL", Children = new() { new() { Name = "Chicago", Type = "City", Population = 2_700_000, Code = "ORD" }, new() { Name = "Aurora", Type = "City", Population = 180_000, Code = "AUZ" }, new() { Name = "Naperville", Type = "City", Population = 149_000, Code = "NPV" }, new() { Name = "Springfield", Type = "City", Population = 114_000, Code = "SPI" } } },
                        new() { Name = "Pennsylvania", Type = "State", Population = 13_000_000, Code = "PA", Children = new() { new() { Name = "Philadelphia", Type = "City", Population = 1_600_000, Code = "PHL" }, new() { Name = "Pittsburgh", Type = "City", Population = 302_000, Code = "PIT" }, new() { Name = "Allentown", Type = "City", Population = 126_000, Code = "ABE" } } },
                        new() { Name = "Ohio", Type = "State", Population = 11_800_000, Code = "OH", Children = new() { new() { Name = "Columbus", Type = "City", Population = 906_000, Code = "CMH" }, new() { Name = "Cleveland", Type = "City", Population = 372_000, Code = "CLE" }, new() { Name = "Cincinnati", Type = "City", Population = 309_000, Code = "CVG" }, new() { Name = "Toledo", Type = "City", Population = 270_000, Code = "TOL" } } },
                        new() { Name = "Georgia", Type = "State", Population = 10_900_000, Code = "GA", Children = new() { new() { Name = "Atlanta", Type = "City", Population = 499_000, Code = "ATL" }, new() { Name = "Augusta", Type = "City", Population = 202_000, Code = "AGS" }, new() { Name = "Savannah", Type = "City", Population = 147_000, Code = "SAV" } } },
                        new() { Name = "Washington", Type = "State", Population = 7_700_000, Code = "WA", Children = new() { new() { Name = "Seattle", Type = "City", Population = 737_000, Code = "SEA" }, new() { Name = "Spokane", Type = "City", Population = 228_000, Code = "GEG" }, new() { Name = "Tacoma", Type = "City", Population = 220_000, Code = "TCM" } } },
                        new() { Name = "Massachusetts", Type = "State", Population = 7_000_000, Code = "MA", Children = new() { new() { Name = "Boston", Type = "City", Population = 675_000, Code = "BOS" }, new() { Name = "Worcester", Type = "City", Population = 206_000, Code = "ORH" }, new() { Name = "Cambridge", Type = "City", Population = 118_000, Code = "CBR" } } },
                        new() { Name = "Arizona", Type = "State", Population = 7_300_000, Code = "AZ", Children = new() { new() { Name = "Phoenix", Type = "City", Population = 1_600_000, Code = "PHX" }, new() { Name = "Tucson", Type = "City", Population = 542_000, Code = "TUS" }, new() { Name = "Mesa", Type = "City", Population = 504_000, Code = "MSC" } } },
                        new() { Name = "Colorado", Type = "State", Population = 5_800_000, Code = "CO", Children = new() { new() { Name = "Denver", Type = "City", Population = 715_000, Code = "DEN" }, new() { Name = "Colorado Springs", Type = "City", Population = 478_000, Code = "COS" }, new() { Name = "Aurora", Type = "City", Population = 386_000, Code = "AUO" }, new() { Name = "Boulder", Type = "City", Population = 105_000, Code = "BLD" } } },
                        new() { Name = "Michigan", Type = "State", Population = 10_000_000, Code = "MI", Children = new() { new() { Name = "Detroit", Type = "City", Population = 639_000, Code = "DTW" }, new() { Name = "Grand Rapids", Type = "City", Population = 198_000, Code = "GRR" }, new() { Name = "Ann Arbor", Type = "City", Population = 123_000, Code = "ARB" } } },
                        new() { Name = "North Carolina", Type = "State", Population = 10_600_000, Code = "NC", Children = new() { new() { Name = "Charlotte", Type = "City", Population = 879_000, Code = "CLT" }, new() { Name = "Raleigh", Type = "City", Population = 467_000, Code = "RDU" }, new() { Name = "Durham", Type = "City", Population = 283_000, Code = "RDU" } } },
                        new() { Name = "Virginia", Type = "State", Population = 8_600_000, Code = "VA", Children = new() { new() { Name = "Virginia Beach", Type = "City", Population = 459_000, Code = "ORF" }, new() { Name = "Norfolk", Type = "City", Population = 238_000, Code = "ORF" }, new() { Name = "Richmond", Type = "City", Population = 227_000, Code = "RIC" } } },
                        new() { Name = "Minnesota", Type = "State", Population = 5_700_000, Code = "MN", Children = new() { new() { Name = "Minneapolis", Type = "City", Population = 429_000, Code = "MSP" }, new() { Name = "Saint Paul", Type = "City", Population = 311_000, Code = "STP" } } },
                        new() { Name = "Oregon", Type = "State", Population = 4_200_000, Code = "OR", Children = new() { new() { Name = "Portland", Type = "City", Population = 652_000, Code = "PDX" }, new() { Name = "Eugene", Type = "City", Population = 176_000, Code = "EUG" }, new() { Name = "Salem", Type = "City", Population = 175_000, Code = "SLE" } } },
                        new() { Name = "Tennessee", Type = "State", Population = 7_000_000, Code = "TN", Children = new() { new() { Name = "Nashville", Type = "City", Population = 689_000, Code = "BNA" }, new() { Name = "Memphis", Type = "City", Population = 633_000, Code = "MEM" }, new() { Name = "Knoxville", Type = "City", Population = 190_000, Code = "TYS" } } },
                        new() { Name = "Missouri", Type = "State", Population = 6_200_000, Code = "MO", Children = new() { new() { Name = "Kansas City", Type = "City", Population = 508_000, Code = "MCI" }, new() { Name = "St. Louis", Type = "City", Population = 293_000, Code = "STL" } } },
                        new() { Name = "Maryland", Type = "State", Population = 6_200_000, Code = "MD", Children = new() { new() { Name = "Baltimore", Type = "City", Population = 585_000, Code = "BWI" } } }
                    }
                },
                new()
                {
                    Name = "Canada", Type = "Country", Population = 40_000_000, Code = "CA",
                    Children = new()
                    {
                        new() { Name = "Ontario", Type = "Province", Population = 15_100_000, Code = "ON", Children = new() { new() { Name = "Toronto", Type = "City", Population = 2_800_000, Code = "YYZ" }, new() { Name = "Ottawa", Type = "City", Population = 1_000_000, Code = "YOW" }, new() { Name = "Mississauga", Type = "City", Population = 720_000, Code = "YMQ" }, new() { Name = "Hamilton", Type = "City", Population = 570_000, Code = "YHM" }, new() { Name = "Brampton", Type = "City", Population = 656_000, Code = "BMP" }, new() { Name = "London", Type = "City", Population = 422_000, Code = "YXU" }, new() { Name = "Kitchener", Type = "City", Population = 270_000, Code = "YKF" } } },
                        new() { Name = "Quebec", Type = "Province", Population = 8_700_000, Code = "QC", Children = new() { new() { Name = "Montreal", Type = "City", Population = 1_800_000, Code = "YUL" }, new() { Name = "Quebec City", Type = "City", Population = 549_000, Code = "YQB" }, new() { Name = "Laval", Type = "City", Population = 438_000, Code = "LAV" }, new() { Name = "Gatineau", Type = "City", Population = 291_000, Code = "YND" } } },
                        new() { Name = "British Columbia", Type = "Province", Population = 5_300_000, Code = "BC", Children = new() { new() { Name = "Vancouver", Type = "City", Population = 662_000, Code = "YVR" }, new() { Name = "Surrey", Type = "City", Population = 568_000, Code = "SRY" }, new() { Name = "Victoria", Type = "City", Population = 92_000, Code = "YYJ" }, new() { Name = "Burnaby", Type = "City", Population = 249_000, Code = "BNB" } } },
                        new() { Name = "Alberta", Type = "Province", Population = 4_600_000, Code = "AB", Children = new() { new() { Name = "Calgary", Type = "City", Population = 1_300_000, Code = "YYC" }, new() { Name = "Edmonton", Type = "City", Population = 1_000_000, Code = "YEG" }, new() { Name = "Red Deer", Type = "City", Population = 100_000, Code = "YQF" } } },
                        new() { Name = "Manitoba", Type = "Province", Population = 1_400_000, Code = "MB", Children = new() { new() { Name = "Winnipeg", Type = "City", Population = 749_000, Code = "YWG" } } },
                        new() { Name = "Saskatchewan", Type = "Province", Population = 1_200_000, Code = "SK", Children = new() { new() { Name = "Saskatoon", Type = "City", Population = 317_000, Code = "YXE" }, new() { Name = "Regina", Type = "City", Population = 228_000, Code = "YQR" } } },
                        new() { Name = "Nova Scotia", Type = "Province", Population = 1_000_000, Code = "NS", Children = new() { new() { Name = "Halifax", Type = "City", Population = 440_000, Code = "YHZ" } } }
                    }
                },
                new()
                {
                    Name = "Mexico", Type = "Country", Population = 128_900_000, Code = "MX",
                    Children = new()
                    {
                        new() { Name = "Mexico City", Type = "Federal District", Population = 9_200_000, Code = "CMX", Children = new() { new() { Name = "Mexico City", Type = "City", Population = 9_200_000, Code = "MEX" } } },
                        new() { Name = "State of Mexico", Type = "State", Population = 16_900_000, Code = "MEX", Children = new() { new() { Name = "Ecatepec", Type = "City", Population = 1_600_000, Code = "ECA" }, new() { Name = "Nezahualcóyotl", Type = "City", Population = 1_100_000, Code = "NEZ" }, new() { Name = "Toluca", Type = "City", Population = 910_000, Code = "TLC" }, new() { Name = "Naucalpan", Type = "City", Population = 830_000, Code = "NAU" } } },
                        new() { Name = "Jalisco", Type = "State", Population = 8_300_000, Code = "JAL", Children = new() { new() { Name = "Guadalajara", Type = "City", Population = 1_500_000, Code = "GDL" }, new() { Name = "Zapopan", Type = "City", Population = 1_400_000, Code = "ZPN" }, new() { Name = "Tlaquepaque", Type = "City", Population = 688_000, Code = "TLQ" } } },
                        new() { Name = "Nuevo León", Type = "State", Population = 5_600_000, Code = "NLE", Children = new() { new() { Name = "Monterrey", Type = "City", Population = 1_100_000, Code = "MTY" }, new() { Name = "Guadalupe", Type = "City", Population = 682_000, Code = "GPE" }, new() { Name = "San Nicolás", Type = "City", Population = 430_000, Code = "SNI" } } },
                        new() { Name = "Puebla", Type = "State", Population = 6_600_000, Code = "PUE", Children = new() { new() { Name = "Puebla", Type = "City", Population = 1_700_000, Code = "PBC" }, new() { Name = "Tehuacán", Type = "City", Population = 320_000, Code = "THN" } } },
                        new() { Name = "Guanajuato", Type = "State", Population = 6_200_000, Code = "GTO", Children = new() { new() { Name = "León", Type = "City", Population = 1_600_000, Code = "BJX" }, new() { Name = "Irapuato", Type = "City", Population = 580_000, Code = "IRP" } } },
                        new() { Name = "Chihuahua", Type = "State", Population = 3_700_000, Code = "CHH", Children = new() { new() { Name = "Juárez", Type = "City", Population = 1_500_000, Code = "CJS" }, new() { Name = "Chihuahua", Type = "City", Population = 925_000, Code = "CUU" } } },
                        new() { Name = "Veracruz", Type = "State", Population = 8_100_000, Code = "VER", Children = new() { new() { Name = "Veracruz", Type = "City", Population = 600_000, Code = "VER" }, new() { Name = "Xalapa", Type = "City", Population = 480_000, Code = "JAL" } } },
                        new() { Name = "Yucatán", Type = "State", Population = 2_300_000, Code = "YUC", Children = new() { new() { Name = "Mérida", Type = "City", Population = 1_000_000, Code = "MID" } } },
                        new() { Name = "Baja California", Type = "State", Population = 3_800_000, Code = "BCN", Children = new() { new() { Name = "Tijuana", Type = "City", Population = 1_900_000, Code = "TIJ" }, new() { Name = "Mexicali", Type = "City", Population = 1_000_000, Code = "MXL" } } },
                        new() { Name = "Quintana Roo", Type = "State", Population = 1_900_000, Code = "ROO", Children = new() { new() { Name = "Cancún", Type = "City", Population = 889_000, Code = "CUN" } } }
                    }
                }
            }
        },
        new()
        {
            Name = "South America", Type = "Continent", Population = 430_000_000, Code = "SA",
            Children = new()
            {
                new()
                {
                    Name = "Brazil", Type = "Country", Population = 216_000_000, Code = "BR",
                    Children = new()
                    {
                        new() { Name = "São Paulo", Type = "State", Population = 46_600_000, Code = "SP", Children = new() { new() { Name = "São Paulo", Type = "City", Population = 12_300_000, Code = "GRU" }, new() { Name = "Guarulhos", Type = "City", Population = 1_400_000, Code = "GUA" }, new() { Name = "Campinas", Type = "City", Population = 1_200_000, Code = "VCP" }, new() { Name = "São Bernardo do Campo", Type = "City", Population = 844_000, Code = "SBC" }, new() { Name = "Santo André", Type = "City", Population = 721_000, Code = "STA" }, new() { Name = "Osasco", Type = "City", Population = 698_000, Code = "OSC" }, new() { Name = "Sorocaba", Type = "City", Population = 687_000, Code = "SOD" }, new() { Name = "Ribeirão Preto", Type = "City", Population = 711_000, Code = "RAO" }, new() { Name = "Santos", Type = "City", Population = 433_000, Code = "SSZ" }, new() { Name = "São José dos Campos", Type = "City", Population = 729_000, Code = "SJK" }, new() { Name = "Piracicaba", Type = "City", Population = 410_000, Code = "PIR" } } },
                        new() { Name = "Rio de Janeiro", Type = "State", Population = 17_500_000, Code = "RJ", Children = new() { new() { Name = "Rio de Janeiro", Type = "City", Population = 6_700_000, Code = "GIG" }, new() { Name = "Niterói", Type = "City", Population = 515_000, Code = "NIT" }, new() { Name = "Nova Iguaçu", Type = "City", Population = 820_000, Code = "NIG" }, new() { Name = "Duque de Caxias", Type = "City", Population = 924_000, Code = "DQC" }, new() { Name = "São Gonçalo", Type = "City", Population = 1_090_000, Code = "SGO" } } },
                        new() { Name = "Minas Gerais", Type = "State", Population = 21_400_000, Code = "MG", Children = new() { new() { Name = "Belo Horizonte", Type = "City", Population = 2_500_000, Code = "CNF" }, new() { Name = "Uberlândia", Type = "City", Population = 706_000, Code = "UDI" }, new() { Name = "Contagem", Type = "City", Population = 668_000, Code = "CTG" }, new() { Name = "Juiz de Fora", Type = "City", Population = 577_000, Code = "JDF" } } },
                        new() { Name = "Bahia", Type = "State", Population = 14_900_000, Code = "BA", Children = new() { new() { Name = "Salvador", Type = "City", Population = 2_900_000, Code = "SSA" }, new() { Name = "Feira de Santana", Type = "City", Population = 619_000, Code = "FDS" } } },
                        new() { Name = "Paraná", Type = "State", Population = 11_500_000, Code = "PR", Children = new() { new() { Name = "Curitiba", Type = "City", Population = 1_900_000, Code = "CWB" }, new() { Name = "Londrina", Type = "City", Population = 580_000, Code = "LDB" }, new() { Name = "Maringá", Type = "City", Population = 430_000, Code = "MGF" } } },
                        new() { Name = "Rio Grande do Sul", Type = "State", Population = 11_400_000, Code = "RS", Children = new() { new() { Name = "Porto Alegre", Type = "City", Population = 1_500_000, Code = "POA" }, new() { Name = "Caxias do Sul", Type = "City", Population = 517_000, Code = "CXJ" } } },
                        new() { Name = "Pernambuco", Type = "State", Population = 9_600_000, Code = "PE", Children = new() { new() { Name = "Recife", Type = "City", Population = 1_600_000, Code = "REC" }, new() { Name = "Jaboatão dos Guararapes", Type = "City", Population = 706_000, Code = "JGR" } } },
                        new() { Name = "Ceará", Type = "State", Population = 9_200_000, Code = "CE", Children = new() { new() { Name = "Fortaleza", Type = "City", Population = 2_700_000, Code = "FOR" } } },
                        new() { Name = "Federal District", Type = "Territory", Population = 3_000_000, Code = "DF", Children = new() { new() { Name = "Brasília", Type = "City", Population = 3_000_000, Code = "BSB" } } },
                        new() { Name = "Goiás", Type = "State", Population = 7_100_000, Code = "GO", Children = new() { new() { Name = "Goiânia", Type = "City", Population = 1_500_000, Code = "GYN" }, new() { Name = "Aparecida de Goiânia", Type = "City", Population = 590_000, Code = "APG" } } },
                        new() { Name = "Pará", Type = "State", Population = 8_700_000, Code = "PA", Children = new() { new() { Name = "Belém", Type = "City", Population = 1_500_000, Code = "BEL" }, new() { Name = "Ananindeua", Type = "City", Population = 535_000, Code = "ANA" } } }
                    }
                },
                new()
                {
                    Name = "Colombia", Type = "Country", Population = 52_000_000, Code = "CO",
                    Children = new()
                    {
                        new() { Name = "Bogotá D.C.", Type = "Capital District", Population = 7_900_000, Code = "DC", Children = new() { new() { Name = "Bogotá", Type = "City", Population = 7_900_000, Code = "BOG" } } },
                        new() { Name = "Antioquia", Type = "Department", Population = 6_700_000, Code = "ANT", Children = new() { new() { Name = "Medellín", Type = "City", Population = 2_500_000, Code = "MDE" }, new() { Name = "Bello", Type = "City", Population = 533_000, Code = "BEO" } } },
                        new() { Name = "Valle del Cauca", Type = "Department", Population = 4_800_000, Code = "VAC", Children = new() { new() { Name = "Cali", Type = "City", Population = 2_200_000, Code = "CLO" }, new() { Name = "Buenaventura", Type = "City", Population = 431_000, Code = "BUN" } } },
                        new() { Name = "Atlántico", Type = "Department", Population = 2_700_000, Code = "ATL", Children = new() { new() { Name = "Barranquilla", Type = "City", Population = 1_200_000, Code = "BAQ" } } },
                        new() { Name = "Santander", Type = "Department", Population = 2_200_000, Code = "SAN", Children = new() { new() { Name = "Bucaramanga", Type = "City", Population = 581_000, Code = "BGA" } } }
                    }
                },
                new()
                {
                    Name = "Argentina", Type = "Country", Population = 46_000_000, Code = "AR",
                    Children = new()
                    {
                        new() { Name = "Buenos Aires", Type = "Province", Population = 17_500_000, Code = "BA", Children = new() { new() { Name = "Buenos Aires", Type = "City", Population = 3_100_000, Code = "EZE" }, new() { Name = "La Plata", Type = "City", Population = 900_000, Code = "LPG" }, new() { Name = "Mar del Plata", Type = "City", Population = 650_000, Code = "MDQ" }, new() { Name = "Quilmes", Type = "City", Population = 582_000, Code = "QUI" } } },
                        new() { Name = "Córdoba", Type = "Province", Population = 3_800_000, Code = "CB", Children = new() { new() { Name = "Córdoba", Type = "City", Population = 1_500_000, Code = "COR" }, new() { Name = "Villa María", Type = "City", Population = 100_000, Code = "VMA" } } },
                        new() { Name = "Santa Fe", Type = "Province", Population = 3_500_000, Code = "SF", Children = new() { new() { Name = "Rosario", Type = "City", Population = 1_200_000, Code = "ROS" }, new() { Name = "Santa Fe", Type = "City", Population = 525_000, Code = "SFN" } } },
                        new() { Name = "Mendoza", Type = "Province", Population = 2_000_000, Code = "MZ", Children = new() { new() { Name = "Mendoza", Type = "City", Population = 1_000_000, Code = "MDZ" } } },
                        new() { Name = "Tucumán", Type = "Province", Population = 1_700_000, Code = "TM", Children = new() { new() { Name = "San Miguel de Tucumán", Type = "City", Population = 900_000, Code = "TUC" } } }
                    }
                },
                new()
                {
                    Name = "Chile", Type = "Country", Population = 19_500_000, Code = "CL",
                    Children = new()
                    {
                        new() { Name = "Santiago Metropolitan", Type = "Region", Population = 8_100_000, Code = "RM", Children = new() { new() { Name = "Santiago", Type = "City", Population = 5_600_000, Code = "SCL" }, new() { Name = "Puente Alto", Type = "City", Population = 570_000, Code = "PAL" }, new() { Name = "Maipú", Type = "City", Population = 520_000, Code = "MPU" } } },
                        new() { Name = "Valparaíso", Type = "Region", Population = 1_900_000, Code = "VS", Children = new() { new() { Name = "Viña del Mar", Type = "City", Population = 334_000, Code = "VDM" }, new() { Name = "Valparaíso", Type = "City", Population = 296_000, Code = "VAP" } } },
                        new() { Name = "Biobío", Type = "Region", Population = 1_600_000, Code = "BI", Children = new() { new() { Name = "Concepción", Type = "City", Population = 224_000, Code = "CCP" } } }
                    }
                },
                new()
                {
                    Name = "Peru", Type = "Country", Population = 34_000_000, Code = "PE",
                    Children = new()
                    {
                        new() { Name = "Lima", Type = "Region", Population = 10_100_000, Code = "LIM", Children = new() { new() { Name = "Lima", Type = "City", Population = 9_800_000, Code = "LIM" } } },
                        new() { Name = "Arequipa", Type = "Region", Population = 1_500_000, Code = "ARE", Children = new() { new() { Name = "Arequipa", Type = "City", Population = 1_000_000, Code = "AQP" } } },
                        new() { Name = "La Libertad", Type = "Region", Population = 2_000_000, Code = "LAL", Children = new() { new() { Name = "Trujillo", Type = "City", Population = 919_000, Code = "TRU" } } }
                    }
                },
                new()
                {
                    Name = "Venezuela", Type = "Country", Population = 28_400_000, Code = "VE",
                    Children = new()
                    {
                        new() { Name = "Capital District", Type = "District", Population = 2_100_000, Code = "DC", Children = new() { new() { Name = "Caracas", Type = "City", Population = 2_100_000, Code = "CCS" } } },
                        new() { Name = "Zulia", Type = "State", Population = 4_300_000, Code = "ZUL", Children = new() { new() { Name = "Maracaibo", Type = "City", Population = 1_600_000, Code = "MAR" } } },
                        new() { Name = "Miranda", Type = "State", Population = 3_300_000, Code = "MIR", Children = new() { new() { Name = "Guarenas", Type = "City", Population = 370_000, Code = "GRN" } } }
                    }
                },
                new()
                {
                    Name = "Ecuador", Type = "Country", Population = 18_000_000, Code = "EC",
                    Children = new()
                    {
                        new() { Name = "Guayas", Type = "Province", Population = 4_400_000, Code = "G", Children = new() { new() { Name = "Guayaquil", Type = "City", Population = 2_700_000, Code = "GYE" } } },
                        new() { Name = "Pichincha", Type = "Province", Population = 3_200_000, Code = "P", Children = new() { new() { Name = "Quito", Type = "City", Population = 2_800_000, Code = "UIO" } } }
                    }
                }
            }
        },
        new()
        {
            Name = "Oceania", Type = "Continent", Population = 45_000_000, Code = "OC",
            Children = new()
            {
                new()
                {
                    Name = "Australia", Type = "Country", Population = 26_400_000, Code = "AU",
                    Children = new()
                    {
                        new() { Name = "New South Wales", Type = "State", Population = 8_200_000, Code = "NSW", Children = new() { new() { Name = "Sydney", Type = "City", Population = 5_300_000, Code = "SYD" }, new() { Name = "Newcastle", Type = "City", Population = 322_000, Code = "NTL" }, new() { Name = "Wollongong", Type = "City", Population = 306_000, Code = "WOL" } } },
                        new() { Name = "Victoria", Type = "State", Population = 6_700_000, Code = "VIC", Children = new() { new() { Name = "Melbourne", Type = "City", Population = 5_000_000, Code = "MEL" }, new() { Name = "Geelong", Type = "City", Population = 264_000, Code = "GEX" }, new() { Name = "Ballarat", Type = "City", Population = 113_000, Code = "BLT" } } },
                        new() { Name = "Queensland", Type = "State", Population = 5_400_000, Code = "QLD", Children = new() { new() { Name = "Brisbane", Type = "City", Population = 2_500_000, Code = "BNE" }, new() { Name = "Gold Coast", Type = "City", Population = 680_000, Code = "OOL" }, new() { Name = "Sunshine Coast", Type = "City", Population = 350_000, Code = "MCY" }, new() { Name = "Cairns", Type = "City", Population = 156_000, Code = "CNS" }, new() { Name = "Townsville", Type = "City", Population = 180_000, Code = "TSV" } } },
                        new() { Name = "Western Australia", Type = "State", Population = 2_800_000, Code = "WA", Children = new() { new() { Name = "Perth", Type = "City", Population = 2_100_000, Code = "PER" }, new() { Name = "Mandurah", Type = "City", Population = 97_000, Code = "MAN" } } },
                        new() { Name = "South Australia", Type = "State", Population = 1_800_000, Code = "SA", Children = new() { new() { Name = "Adelaide", Type = "City", Population = 1_400_000, Code = "ADL" } } },
                        new() { Name = "Tasmania", Type = "State", Population = 570_000, Code = "TAS", Children = new() { new() { Name = "Hobart", Type = "City", Population = 238_000, Code = "HBA" } } },
                        new() { Name = "Australian Capital Territory", Type = "Territory", Population = 460_000, Code = "ACT", Children = new() { new() { Name = "Canberra", Type = "City", Population = 460_000, Code = "CBR" } } }
                    }
                },
                new()
                {
                    Name = "New Zealand", Type = "Country", Population = 5_100_000, Code = "NZ",
                    Children = new()
                    {
                        new() { Name = "Auckland", Type = "Region", Population = 1_700_000, Code = "AUK", Children = new() { new() { Name = "Auckland", Type = "City", Population = 1_500_000, Code = "AKL" } } },
                        new() { Name = "Wellington", Type = "Region", Population = 530_000, Code = "WGN", Children = new() { new() { Name = "Wellington", Type = "City", Population = 215_000, Code = "WLG" }, new() { Name = "Lower Hutt", Type = "City", Population = 112_000, Code = "LHT" } } },
                        new() { Name = "Canterbury", Type = "Region", Population = 640_000, Code = "CAN", Children = new() { new() { Name = "Christchurch", Type = "City", Population = 380_000, Code = "CHC" } } }
                    }
                },
                new()
                {
                    Name = "Papua New Guinea", Type = "Country", Population = 10_100_000, Code = "PG",
                    Children = new()
                    {
                        new() { Name = "National Capital District", Type = "District", Population = 400_000, Code = "NCD", Children = new() { new() { Name = "Port Moresby", Type = "City", Population = 400_000, Code = "POM" } } }
                    }
                }
            }
        }
    };
}
