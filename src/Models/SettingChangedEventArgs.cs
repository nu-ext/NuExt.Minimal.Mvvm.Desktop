using System;

namespace Minimal.Mvvm;

/// <summary>
/// Provides data for the <see cref="KeyValueSettingCollection.SettingChanged"/> event.
/// </summary>
/// <remarks>
/// A <see langword="readonly record struct"/> is used on purpose: a setting value change is a
/// frequent, short-lived notification, so it should not allocate an argument object per raise.
/// </remarks>
public readonly record struct SettingChangedEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SettingChangedEventArgs"/> struct.
    /// </summary>
    /// <param name="setting">The setting whose value has changed.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="setting"/> is <see langword="null"/>.</exception>
    public SettingChangedEventArgs(KeyValueSetting setting)
    {
        ArgumentNullException.ThrowIfNull(setting);

        Setting = setting;
    }

    /// <summary>
    /// The setting whose value has changed.
    /// </summary>
    public KeyValueSetting Setting { get; }

    /// <summary>
    /// Name of the changed setting.
    /// </summary>
    public string Name => Setting.Name;

    /// <summary>
    /// New raw value of the changed setting.
    /// </summary>
    public string Value => Setting.Value;

    /// <summary>
    /// Type of the changed setting value.
    /// </summary>
    public Type ValueType => Setting.ValueType;
}
