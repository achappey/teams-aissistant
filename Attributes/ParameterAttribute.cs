namespace TeamsAIssistant.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class ParameterAttribute(string name, string type, string? description = null, bool required = false,
    bool multiline = false, bool readOnly = false, string[]? enumValues = null,
    int maxLength = 0, int minimum = int.MinValue, int maximum = int.MaxValue, string? format = null, bool visible = true) : Attribute
{
    public string Name { get; } = name;
    public string ParamType { get; } = type;
    public string? Description { get; } = description;
    public bool Required { get; } = required;
    public bool Multiline { get; } = multiline;
    public bool Visible { get; } = visible;
    public bool ReadOnly { get; } = readOnly;
    public string[]? EnumValues { get; } = enumValues;
    public int? MaxLength { get; } = maxLength;
    public int? Minimum { get; } = minimum != int.MinValue ? minimum : null;
    public int? Maximum { get; } = maximum != int.MaxValue ? maximum : null;
    public string? Format { get; } = format;
}
