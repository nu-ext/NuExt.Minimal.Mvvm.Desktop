namespace Minimal.Mvvm;

/// <summary>
/// Defines the supported setting value type discriminators.
/// </summary>
public static class SettingValueTypes
{
    /// <summary>
    /// A string value.
    /// </summary>
    public const string String = "string";

    /// <summary>
    /// An integer value.
    /// </summary>
    public const string Integer = "integer";

    /// <summary>
    /// A floating-point value.
    /// </summary>
    public const string Real = "real";

    /// <summary>
    /// A Boolean value.
    /// </summary>
    public const string Boolean = "boolean";

    /// <summary>
    /// A date value without a time component.
    /// </summary>
    public const string Date = "date";

    /// <summary>
    /// A date and time value.
    /// </summary>
    public const string DateTime = "datetime";

    /// <summary>
    /// A JSON value.
    /// </summary>
    public const string Json = "json";
}
