using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Minimal.Mvvm;

/// <summary>
/// Provides a base class for settings objects. Change tracking is inherited from
/// <see cref="ChangeTrackingModelBase"/>; this class adds a read-only mode, preservation of unknown
/// JSON properties and reflection-based access to properties by name for JSON round-trips.
/// </summary>
public abstract class SettingsBase : ChangeTrackingModelBase
{
    private static readonly ConcurrentDictionary<Type, IReadOnlyDictionary<string, PropertyInfo>> s_typeProperties = new();

    private static readonly Func<Type, IReadOnlyDictionary<string, PropertyInfo>> s_typePropertiesFactory = GetProperties;

    #region Properties

    /// <summary>
    /// Stores additional data not mapped to other properties.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData { get; set; }

    /// <summary>
    /// Gets a value indicating whether property changes are currently suspended.
    /// </summary>
    [JsonIgnore]
    public bool IsSuspended
    {
        get;
        private set
        {
            if (field == value) return;
            field = value;
            OnPropertyChanged(EventArgsCache.IsSuspendedPropertyChanged);
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Validates the specified property value before it is set.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="propertyName">The name of the property being validated.</param>
    /// <param name="value">The value to validate.</param>
    /// <returns>True if the value is valid; otherwise, false.</returns>
    protected virtual bool IsValidPropertyValue<T>(string? propertyName, T value)
    {
        return true;
    }

    /// <inheritdoc />
    protected override bool CanSetProperty<T>(T oldValue, T newValue, [CallerMemberName] string? propertyName = null)
    {
        return base.CanSetProperty(oldValue, newValue, propertyName) &&
               !IsSuspended && IsValidPropertyValue(propertyName, newValue);
    }

    /// <inheritdoc />
    protected override bool IsTrackedProperty(string? propertyName)
    {
        return base.IsTrackedProperty(propertyName) && propertyName != nameof(IsSuspended);
    }

    /// <summary>
    /// Gets the properties of the current type that participate in serialization.
    /// </summary>
    /// <returns>A dictionary containing property information.</returns>
    protected IReadOnlyDictionary<string, PropertyInfo> GetProperties()
    {
        return s_typeProperties.GetOrAdd(GetType(), s_typePropertiesFactory);
    }

    /// <summary>
    /// Builds the serializable property map for the specified type.
    /// </summary>
    /// <param name="type">The type to get properties for.</param>
    /// <returns>A dictionary containing property information.</returns>
    private static IReadOnlyDictionary<string, PropertyInfo> GetProperties(Type type)
    {
        var properties = type.GetAllProperties(typeof(SettingsBase), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var result = new Dictionary<string, PropertyInfo>();

        foreach (PropertyInfo property in properties)
        {
            if (property.GetCustomAttribute<JsonIgnoreAttribute>() != null) continue;
            if (property.GetCustomAttribute<JsonExtensionDataAttribute>() != null) continue;

            if (result.TryAdd(property.Name, property))
                continue;

            Debug.Assert(property.GetMethod?.IsVirtual == true, $"Property '{property.Name}' is not virtual");
        }

        return new ReadOnlyDictionary<string, PropertyInfo>(result);
    }

    /// <summary>
    /// Gets the type of the specified property.
    /// </summary>
    /// <param name="propertyName">The name of the property.</param>
    /// <returns>The type of the property, or null if the property is not found.</returns>
    public Type? GetPropertyType(string propertyName)
    {
        Debug.Assert(!string.IsNullOrEmpty(propertyName), "propertyName is null or empty");
        ArgumentException.ThrowIfNullOrEmpty(propertyName);

        IReadOnlyDictionary<string, PropertyInfo> properties = GetProperties();

        Debug.Assert(properties.ContainsKey(propertyName), $"propertyName '{propertyName}' is not defined");

        return properties.TryGetValue(propertyName, out PropertyInfo? property) ? property.PropertyType : null;
    }

    /// <summary>
    /// Sets the value of the specified property.
    /// </summary>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="value">The value to set.</param>
    /// <returns>true if the property value was set successfully; otherwise, false.</returns>
    public bool SetProperty(string propertyName, object? value)
    {
        Debug.Assert(!string.IsNullOrEmpty(propertyName), "propertyName is null or empty");
        ArgumentException.ThrowIfNullOrEmpty(propertyName);

        IReadOnlyDictionary<string, PropertyInfo> properties = GetProperties();

        Debug.Assert(properties.ContainsKey(propertyName), $"propertyName '{propertyName}' is not defined");

        if (!properties.TryGetValue(propertyName, out PropertyInfo? property) || !property.CanWrite)
            return false;

        property.SetValue(this, value);
        return true;
    }

    /// <summary>
    /// Resumes property changes after they were suspended.
    /// </summary>
    public void ResumeChanges()
    {
        IsSuspended = false;
    }

    /// <summary>
    /// Suspends property changes.
    /// </summary>
    public void SuspendChanges()
    {
        IsSuspended = true;
    }

    #endregion
}

internal static partial class EventArgsCache
{
    internal static readonly PropertyChangedEventArgs IsSuspendedPropertyChanged = new(nameof(SettingsBase.IsSuspended));
}
