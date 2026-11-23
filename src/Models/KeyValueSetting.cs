using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;

namespace Minimal.Mvvm;

/// <summary>
/// Represents a single named setting with a raw value and a value type discriminator.
/// </summary>
public partial class KeyValueSetting : ChangeTrackingBase
{
    /// <summary>
    /// Setting name.
    /// </summary>
    [Notify]
    public required partial string Name { get; init; }

    /// <summary>
    /// Setting value.
    /// </summary>
    [Notify(CallbackName = nameof(OnValueChanged))]
    public required partial string Value { get; set; }

    /// <summary>
    /// Setting value type.
    /// </summary>
    [Notify]
    public required partial string ValueType { get; init; }

    /// <summary>
    /// Gets or sets the value as a string.
    /// </summary>
    public string ValueAsString
    {
        get => DeserializeSettingValue<string>(Value, ValueType);
        set => SetValue(value, SettingValueTypes.String);
    }

    /// <summary>
    /// Gets or sets the value as an <see cref="int"/>.
    /// </summary>
    public int ValueAsInt32
    {
        get => DeserializeSettingValue<int>(Value, ValueType);
        set => SetValue(value, SettingValueTypes.Integer);
    }

    /// <summary>
    /// Gets or sets the value as a <see cref="long"/>.
    /// </summary>
    public long ValueAsInt64
    {
        get => DeserializeSettingValue<long>(Value, ValueType);
        set => SetValue(value, SettingValueTypes.Integer);
    }

    /// <summary>
    /// Gets or sets the value as a <see cref="double"/>.
    /// </summary>
    public double ValueAsDouble
    {
        get => DeserializeSettingValue<double>(Value, ValueType);
        set => SetValue(value, SettingValueTypes.Real);
    }

    /// <summary>
    /// Gets or sets the value as a <see cref="bool"/>.
    /// </summary>
    public bool ValueAsBoolean
    {
        get => DeserializeSettingValue<bool>(Value, ValueType);
        set => SetValue(value, SettingValueTypes.Boolean);
    }

#if NET6_0_OR_GREATER
    /// <summary>
    /// Gets or sets the value as a <see cref="DateOnly"/>.
    /// </summary>
    public DateOnly ValueAsDate
    {
        get => DeserializeSettingValue<DateOnly>(Value, ValueType);
        set => SetValue(value, SettingValueTypes.Date);
    }
#else
    /// <summary>
    /// Gets or sets the value as a <see cref="DateTime"/>.
    /// </summary>
    public DateTime ValueAsDate
    {
        get => DeserializeSettingValue<DateTime>(Value, ValueType);
        set => SetValue(value, SettingValueTypes.Date);
    }
#endif

    /// <summary>
    /// Gets or sets the value as a <see cref="DateTimeOffset"/>.
    /// </summary>
    public DateTimeOffset ValueAsDateTime
    {
        get => DeserializeSettingValue<DateTimeOffset>(Value, ValueType);
        set => SetValue(value, SettingValueTypes.DateTime);
    }

    /// <summary>
    /// Gets or sets the value as a <see cref="JsonNode"/>.
    /// </summary>
    public JsonNode ValueAsJson
    {
        get => DeserializeSettingValue<JsonNode>(Value, ValueType);
        set => SetValue(value, SettingValueTypes.Json);
    }

    private void OnValueChanged()
    {
        if (!IsInitialized) return;

        Debug.Assert(!string.IsNullOrWhiteSpace(ValueType), "ValueType should not be null or whitespace.");
        switch (ValueType)
        {
            case SettingValueTypes.String:
                OnPropertyChanged(nameof(ValueAsString));
                break;
            case SettingValueTypes.Integer:
                RaisePropertiesChanged(nameof(ValueAsInt32), nameof(ValueAsInt64));
                break;
            case SettingValueTypes.Real:
                OnPropertyChanged(nameof(ValueAsDouble));
                break;
            case SettingValueTypes.Boolean:
                OnPropertyChanged(nameof(ValueAsBoolean));
                break;
            case SettingValueTypes.Date:
                OnPropertyChanged(nameof(ValueAsDate));
                break;
            case SettingValueTypes.DateTime:
                OnPropertyChanged(nameof(ValueAsDateTime));
                break;
            case SettingValueTypes.Json:
                OnPropertyChanged(nameof(ValueAsJson));
                break;
            default:
                throw new InvalidOperationException($"Unsupported setting value type '{ValueType}'.");
        }
    }

    /// <summary>
    /// Serializes the specified value to its raw string representation for the given value type.
    /// </summary>
    /// <param name="value">The value to serialize.</param>
    /// <param name="valueType">The value type discriminator.</param>
    /// <returns>The raw string representation of the value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="valueType"/> is not supported.</exception>
    public static string SerializeSettingValue(object value, string valueType)
    {
        ArgumentNullException.ThrowIfNull(value);

        return valueType switch
        {
            SettingValueTypes.String => (string)value,
            SettingValueTypes.Integer => Convert.ToInt64(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture),
            SettingValueTypes.Real => Convert.ToDouble(value, CultureInfo.InvariantCulture).ToString("R", CultureInfo.InvariantCulture),
            SettingValueTypes.Boolean => Convert.ToBoolean(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture),
#if NET6_0_OR_GREATER
            SettingValueTypes.Date => ((DateOnly)value).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
#else
            SettingValueTypes.Date => ((DateTime)value).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
#endif
            SettingValueTypes.DateTime => ((DateTimeOffset)value).ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
            SettingValueTypes.Json => ((JsonNode)value).ToJsonString(),
            _ => throw new ArgumentOutOfRangeException(nameof(valueType), valueType, "Unsupported setting value type.")
        };
    }

    /// <summary>
    /// Deserializes the raw string value to the specified type for the given value type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the value to.</typeparam>
    /// <param name="value">The raw string value.</param>
    /// <param name="valueType">The value type discriminator.</param>
    /// <returns>The deserialized value.</returns>
    /// <exception cref="InvalidOperationException">The value cannot be read as <typeparamref name="T"/>.</exception>
    /// <exception cref="FormatException">The JSON value is <see langword="null"/>.</exception>
    public static T DeserializeSettingValue<T>(string value, string valueType)
    {
        object result = valueType switch
        {
            SettingValueTypes.String when typeof(T) == typeof(string) => value,
            SettingValueTypes.Integer when typeof(T) == typeof(long) => long.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture),
            SettingValueTypes.Integer when typeof(T) == typeof(int) => int.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture),
            SettingValueTypes.Real when typeof(T) == typeof(double) => double.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture),
            SettingValueTypes.Boolean when typeof(T) == typeof(bool) => bool.Parse(value),
#if NET6_0_OR_GREATER
            SettingValueTypes.Date when typeof(T) == typeof(DateOnly) => DateOnly.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture),
#else
            SettingValueTypes.Date when typeof(T) == typeof(DateTime) => DateTime.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture),
#endif
            SettingValueTypes.DateTime when typeof(T) == typeof(DateTimeOffset) => DateTimeOffset.ParseExact(value, "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
            SettingValueTypes.Json when typeof(T) == typeof(JsonNode) => JsonNode.Parse(value)
                                                                  ?? throw new FormatException("The JSON setting value cannot be null."),
            _ => throw new InvalidOperationException($"Setting value type '{valueType}' cannot be read as '{typeof(T).Name}'.")
        };

        return (T)result;
    }

    private void SetValue(object value, string expectedValueType, [CallerMemberName] string? propertyName = null)
    {
        EnsureValueType(expectedValueType);

        if (SetProperty(ref _value, SerializeSettingValue(value, ValueType), propertyName))
        {
            OnPropertyChanged(EventArgsCache.ValuePropertyChanged);
        }
    }

    private void EnsureValueType(string expectedValueType)
    {
        if (!StringComparer.Ordinal.Equals(ValueType, expectedValueType))
        {
            throw new InvalidOperationException($"Setting value type is '{ValueType}', but '{expectedValueType}' was requested.");
        }
    }
}
