using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Minimal.Mvvm.Reflection;

namespace Minimal.Mvvm;

/// <summary>
/// Provides a base class for settings objects. Change tracking is inherited from
/// <see cref="ChangeTrackingBase"/>; this class adds a read-only mode, preservation of unknown
/// JSON properties and reflection-based access to properties by name for JSON round-trips.
/// </summary>
public abstract class SettingsBase : ChangeTrackingBase, ISupportReadOnly, IReflectableProperties
{
    #region Properties

    /// <summary>
    /// Stores additional data not mapped to other properties.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData { get; set; }

    /// <summary>
    /// Gets a value indicating whether property writes are currently rejected.
    /// </summary>
    [JsonIgnore]
    public bool IsReadOnly
    {
        get;
        private set
        {
            if (field == value) return;
            field = value;
            OnPropertyChanged(EventArgsCache.IsReadOnlyPropertyChanged);
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
               !IsReadOnly && IsValidPropertyValue(propertyName, newValue);
    }

    /// <inheritdoc />
    protected override bool IsTrackedProperty(string? propertyName)
    {
        return base.IsTrackedProperty(propertyName) && propertyName != nameof(IsReadOnly);
    }

    /// <inheritdoc />
    public bool TryGetPropertyType(string propertyName, [NotNullWhen(true)] out Type? propertyType) =>
        PropertyHelper.TryGetPropertyType(this, propertyName, out propertyType);

    /// <inheritdoc />
    public bool TryGetProperty(string propertyName, out object? value) =>
        PropertyHelper.TryGetProperty(this, propertyName, out value);

    /// <inheritdoc />
    public bool TrySetProperty(string propertyName, object? value) =>
        PropertyHelper.TrySetProperty(this, propertyName, value);

    /// <summary>
    /// Switches the object to a read-only state so that property writes are rejected.
    /// </summary>
    /// <remarks>Every call must be paired with <see cref="EndReadOnly"/>.</remarks>
    public void BeginReadOnly()
    {
        IsReadOnly = true;
    }

    /// <summary>
    /// Switches the object back to a writable state.
    /// </summary>
    public void EndReadOnly()
    {
        IsReadOnly = false;
    }

    #endregion
}

internal static partial class EventArgsCache
{
    internal static readonly PropertyChangedEventArgs IsReadOnlyPropertyChanged = new(nameof(SettingsBase.IsReadOnly));
}
