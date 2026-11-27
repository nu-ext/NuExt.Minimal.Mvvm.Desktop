using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Unicode;

namespace Minimal.Mvvm;

/// <summary>
/// Represents a single named setting with a raw value and a value type discriminator.
/// </summary>
public partial class KeyValueSetting : ChangeTrackingBase
{
    /// <summary>
    /// Keeps non-ASCII text (file paths, localized names) readable in the stored value instead of escaping it.
    /// </summary>
    private static readonly JsonSerializerOptions s_jsonSerializerOptions = new()
    {
        // Unicode characters are stored as is; the markup-sensitive ones (<, >, &) stay escaped.
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
    };

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
    [Notify(CallbackName = nameof(OnValueTypeChanged))]
    public required partial Type ValueType { get; init; }

    /// <summary>
    /// Gets or sets the value as a string.
    /// </summary>
    public string ValueAsString
    {
        get => DeserializeSettingValue<string>(Value, ValueType);
        set => SetValue(value, typeof(string));
    }

    /// <summary>
    /// Gets or sets the value as an <see cref="int"/>.
    /// </summary>
    public int ValueAsInt32
    {
        get => DeserializeSettingValue<int>(Value, ValueType);
        set => SetValue(value, typeof(long));
    }

    /// <summary>
    /// Gets or sets the value as a <see cref="long"/>.
    /// </summary>
    public long ValueAsInt64
    {
        get => DeserializeSettingValue<long>(Value, ValueType);
        set => SetValue(value, typeof(long));
    }

    /// <summary>
    /// Gets or sets the value as a <see cref="double"/>.
    /// </summary>
    public double ValueAsDouble
    {
        get => DeserializeSettingValue<double>(Value, ValueType);
        set => SetValue(value, typeof(double));
    }

    /// <summary>
    /// Gets or sets the value as a <see cref="bool"/>.
    /// </summary>
    public bool ValueAsBoolean
    {
        get => DeserializeSettingValue<bool>(Value, ValueType);
        set => SetValue(value, typeof(bool));
    }

#if NET6_0_OR_GREATER
    /// <summary>
    /// Gets or sets the value as a <see cref="DateOnly"/>.
    /// </summary>
    public DateOnly ValueAsDate
    {
        get => DeserializeSettingValue<DateOnly>(Value, ValueType);
        set => SetValue(value, typeof(DateOnly));
    }
#else
    /// <summary>
    /// Gets or sets the value as a <see cref="DateTime"/>.
    /// </summary>
    public DateTime ValueAsDate
    {
        get => DeserializeSettingValue<DateTime>(Value, ValueType);
        set => SetValue(value, typeof(DateTime));
    }
#endif

    /// <summary>
    /// Gets or sets the value as a <see cref="DateTimeOffset"/>.
    /// </summary>
    public DateTimeOffset ValueAsDateTime
    {
        get => DeserializeSettingValue<DateTimeOffset>(Value, ValueType);
        set => SetValue(value, typeof(DateTimeOffset));
    }

    /// <summary>
    /// Gets or sets the value as a <see cref="JsonNode"/>.
    /// </summary>
    public JsonNode ValueAsJson
    {
        get => DeserializeSettingValue<JsonNode>(Value, ValueType);
        set => SetValue(value, typeof(JsonNode));
    }

    /// <summary>
    /// Gets the value deserialized to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the value to.</typeparam>
    /// <returns>The deserialized value.</returns>
    /// <exception cref="InvalidOperationException">The value cannot be read as <typeparamref name="T"/>.</exception>
    public T GetValue<T>() => DeserializeSettingValue<T>(Value, ValueType);

    private void OnValueChanged()
    {
        if (!IsInitialized) return;

        Type valueType = ValueType;
        if (valueType == typeof(string))
        {
            OnPropertyChanged(nameof(ValueAsString));
        }
        else if (valueType == typeof(int) || valueType == typeof(long))
        {
            RaisePropertiesChanged(nameof(ValueAsInt32), nameof(ValueAsInt64));
        }
        else if (valueType == typeof(double))
        {
            OnPropertyChanged(nameof(ValueAsDouble));
        }
        else if (valueType == typeof(bool))
        {
            OnPropertyChanged(nameof(ValueAsBoolean));
        }
#if NET6_0_OR_GREATER
        else if (valueType == typeof(DateOnly))
#else
        else if (valueType == typeof(DateTime))
#endif
        {
            OnPropertyChanged(nameof(ValueAsDate));
        }
        else if (valueType == typeof(DateTimeOffset))
        {
            OnPropertyChanged(nameof(ValueAsDateTime));
        }
        else if (valueType == typeof(JsonNode))
        {
            OnPropertyChanged(nameof(ValueAsJson));
        }
    }

    private void OnValueTypeChanged()
    {
        Type valueType = ValueType;
        if (valueType == typeof(string) ||
            valueType == typeof(int) ||
            valueType == typeof(long) ||
            valueType == typeof(double) ||
            valueType == typeof(bool) ||
#if NET6_0_OR_GREATER
            valueType == typeof(DateOnly) ||
#else
            valueType == typeof(DateTime) ||
#endif
            valueType == typeof(DateTimeOffset) ||
            valueType == typeof(JsonNode))
        {
            return;
        }

        throw new ArgumentOutOfRangeException(nameof(ValueType), valueType, "Unsupported setting value type.");
    }

    /// <summary>
    /// Serializes the specified value to its raw string representation for the given value type.
    /// </summary>
    /// <param name="value">The value to serialize.</param>
    /// <param name="valueType">The value type.</param>
    /// <returns>The raw string representation of the value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> or <paramref name="valueType"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="valueType"/> is not supported.</exception>
    public static string SerializeSettingValue(object value, Type valueType)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(valueType);

        if (valueType == typeof(string))
        {
            return (string)value;
        }

        if (valueType == typeof(int) || valueType == typeof(long))
        {
            return Convert.ToInt64(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
        }

        if (valueType == typeof(double))
        {
            return Convert.ToDouble(value, CultureInfo.InvariantCulture).ToString("R", CultureInfo.InvariantCulture);
        }

        if (valueType == typeof(bool))
        {
            return Convert.ToBoolean(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
        }

#if NET6_0_OR_GREATER
        if (valueType == typeof(DateOnly))
        {
            return ((DateOnly)value).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }
#else
        if (valueType == typeof(DateTime))
        {
            return ((DateTime)value).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }
#endif

        if (valueType == typeof(DateTimeOffset))
        {
            return ((DateTimeOffset)value).ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);
        }

        if (valueType == typeof(JsonNode))
        {
            return ((JsonNode)value).ToJsonString(s_jsonSerializerOptions);
        }

        throw new ArgumentOutOfRangeException(nameof(valueType), valueType, "Unsupported setting value type.");
    }

    /// <summary>
    /// Deserializes the raw string value to the specified type for the given value type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the value to.</typeparam>
    /// <param name="value">The raw string value.</param>
    /// <param name="valueType">The value type.</param>
    /// <returns>The deserialized value.</returns>
    /// <exception cref="InvalidOperationException">The value cannot be read as <typeparamref name="T"/>.</exception>
    /// <exception cref="FormatException">The JSON value is <see langword="null"/>.</exception>
    public static T DeserializeSettingValue<T>(string value, Type valueType)
    {
        if (valueType == typeof(string) && typeof(T) == typeof(string))
        {
            return (T)(object)value;
        }

        if (valueType == typeof(int) || valueType == typeof(long))
        {
            long longValue = long.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);

            if (typeof(T) == typeof(long))
            {
                return (T)(object)longValue;
            }

            if (typeof(T) == typeof(int))
            {
                return (T)(object)checked((int)longValue);
            }

            if (typeof(T) == typeof(short))
            {
                return (T)(object)checked((short)longValue);
            }

            if (typeof(T) == typeof(byte))
            {
                return (T)(object)checked((byte)longValue);
            }
        }

        if (valueType == typeof(double) && typeof(T) == typeof(double))
        {
            return (T)(object)double.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
        }

        if (valueType == typeof(bool) && typeof(T) == typeof(bool))
        {
            return (T)(object)bool.Parse(value);
        }

#if NET6_0_OR_GREATER
        if (valueType == typeof(DateOnly) && typeof(T) == typeof(DateOnly))
        {
            return (T)(object)DateOnly.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        }
#else
        if (valueType == typeof(DateTime) && typeof(T) == typeof(DateTime))
        {
            return (T)(object)DateTime.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        }
#endif

        if (valueType == typeof(DateTimeOffset) && typeof(T) == typeof(DateTimeOffset))
        {
            return (T)(object)DateTimeOffset.ParseExact(value, "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        }

        if (valueType == typeof(JsonNode) && typeof(T) == typeof(JsonNode))
        {
            return (T)(object)(JsonNode.Parse(value)
                               ?? throw new FormatException("The JSON setting value cannot be null."));
        }

        throw new InvalidOperationException($"Setting value type '{valueType}' cannot be read as '{typeof(T).Name}'.");
    }

    private void SetValue(object value, Type expectedValueType, [CallerMemberName] string? propertyName = null)
    {
        EnsureValueType(expectedValueType);

        if (SetProperty(ref _value, SerializeSettingValue(value, ValueType), propertyName))
        {
            OnPropertyChanged(EventArgsCache.ValuePropertyChanged);
        }
    }

    private void EnsureValueType(Type expectedValueType)
    {
        if (ValueType != expectedValueType)
        {
            throw new InvalidOperationException($"Setting value type is '{ValueType}', but '{expectedValueType}' was requested.");
        }
    }
}