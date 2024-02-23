
using TeamsAIssistant.Extensions;

namespace TeamsAIssistant.Handlers.Plugins.Governments.NL.Models;

internal class SchoolHoliday
{
    public string? Id { get; set; }

    public List<Content>? Content { get; set; }

    public string? Title
    {
        get
        {
            return Content != null ? string.Join(", ", Content
                .Select(a => a.Title)
                .Select(StringExtensions.Sanitize))
                : string.Empty;
        }
        set { }
    }

    public string? SchoolYear
    {
        get
        {
            return Content != null ? string.Join(", ", Content
                .Select(a => a.SchoolYear)
                .Select(StringExtensions.Sanitize))
                : string.Empty;
        }
        set { }
    }

    public string? Holidays
    {
        get
        {
            if (Content == null)
                return string.Empty;

            var types = Content.SelectMany(a => a.Vacations!)
                               .Where(v => v != null && v.Type != null)
                               .Select(v => v.Type!.Trim());

            return string.Join(", ", types);
        }
        set { }
    }

    private string? _notice;

    public string? Notice
    {
        get => _notice?.Sanitize();
        set => _notice = value;
    }

    public List<string>? Authorities { get; set; }

    public List<string>? Creators { get; set; }

    public string? License { get; set; }

    public List<string>? Rightsholders { get; set; }

    public string? Language { get; set; }

    public string? Location { get; set; }

    public DateTime? LastModified { get; set; }

    public IDictionary<string, object>? GetNLSchoolHolidaysBySchoolYear
    {
        get { return SchoolYear != null ? new Dictionary<string, object>() { { "schoolYear", SchoolYear } } : null; }
        set { }
    }
}

internal class Content
{
    public string? Title { get; set; }
    public string? SchoolYear { get; set; }
    public List<Vacation>? Vacations { get; set; }
}

internal class Vacation
{
    private string? _type;

    public string? Type
    {
        get => _type?.Sanitize();
        set => _type = value;
    }

    public string? CompulsoryDates { get; set; }

    public List<RegionData>? Regions { get; set; }
}

internal class RegionData
{
    public string? Region { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}


internal class VacationRegionData
{
    public string? Type { get; set; }

    public string? Region { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }
}