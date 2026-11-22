using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Minimal.Mvvm;

/// <summary>
/// Provides a base class for settings objects, extending the functionality of <see cref="ModelBase"/>.
/// Change tracking is inherited from <see cref="ChangeTrackingModelBase"/>; this class adds
/// a read-only mode and preservation of unknown JSON properties.
/// </summary>
public abstract class SettingsBase : ChangeTrackingModelBase
{
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
