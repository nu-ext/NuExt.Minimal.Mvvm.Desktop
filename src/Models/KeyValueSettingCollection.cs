using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Minimal.Mvvm;

/// <summary>
/// Represents a change-tracked collection of <see cref="KeyValueSetting"/> items.
/// </summary>
public abstract class KeyValueSettingCollection : ChangeTrackingBase, ICollection<KeyValueSetting>
{
    private readonly ObservableCollection<KeyValueSetting> _settings = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyValueSettingCollection"/> class.
    /// </summary>
    protected KeyValueSettingCollection()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyValueSettingCollection"/> class
    /// that contains settings copied from the specified collection.
    /// </summary>
    /// <param name="settings">The settings to copy.</param>
    /// <exception cref="ArgumentNullException"><paramref name="settings"/> is <see langword="null"/>.</exception>
    protected KeyValueSettingCollection(IEnumerable<KeyValueSetting> settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        foreach (KeyValueSetting setting in settings)
        {
            Add(setting);
        }
    }

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the collection.</returns>
    public IEnumerator<KeyValueSetting> GetEnumerator() => _settings.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Gets the number of settings in the collection.
    /// </summary>
    public int Count => _settings.Count;

    /// <summary>
    /// Gets a value indicating whether the collection is read-only.
    /// </summary>
    public bool IsReadOnly => false;

    /// <summary>
    /// Gets the setting with the specified name, or <see langword="null"/> if it does not exist.
    /// </summary>
    /// <param name="name">The setting name.</param>
    public KeyValueSetting? this[string name] => GetSettingOrDefault(name);

    /// <summary>
    /// Occurs when the value of a setting in the collection changes.
    /// </summary>
    public event EventHandler<SettingChangedEventArgs>? SettingChanged;

    /// <summary>
    /// Adds a setting to the collection.
    /// </summary>
    /// <param name="item">The setting to add.</param>
    /// <exception cref="ArgumentNullException"><paramref name="item"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">A setting with the same name already exists.</exception>
    public void Add(KeyValueSetting item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (_settings.Any(setting => StringComparer.Ordinal.Equals(setting.Name, item.Name)))
        {
            throw new ArgumentException($"A setting named '{item.Name}' already exists.", nameof(item));
        }

        _settings.Add(item);
    }

    /// <summary>
    /// Removes all settings from the collection.
    /// </summary>
    public void Clear()
    {
        foreach (KeyValueSetting setting in _settings)
        {
            UntrackSetting(setting);
        }

        _settings.Clear();
    }

    /// <summary>
    /// Determines whether the collection contains the specified setting.
    /// </summary>
    /// <param name="item">The setting to locate.</param>
    /// <returns><see langword="true"/> if the setting is found; otherwise, <see langword="false"/>.</returns>
    public bool Contains(KeyValueSetting item) => _settings.Contains(item);

    /// <summary>
    /// Determines whether the collection contains a setting with the specified name.
    /// </summary>
    /// <param name="name">The setting name.</param>
    /// <returns><see langword="true"/> if the setting is found; otherwise, <see langword="false"/>.</returns>
    public bool Contains(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return _settings.Any(setting => StringComparer.Ordinal.Equals(setting.Name, name));
    }

    /// <summary>
    /// Copies the settings to an array, starting at the specified array index.
    /// </summary>
    /// <param name="array">The destination array.</param>
    /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
    public void CopyTo(KeyValueSetting[] array, int arrayIndex) => _settings.CopyTo(array, arrayIndex);

    /// <summary>
    /// Removes the specified setting from the collection.
    /// </summary>
    /// <param name="item">The setting to remove.</param>
    /// <returns><see langword="true"/> if the setting was removed; otherwise, <see langword="false"/>.</returns>
    public bool Remove(KeyValueSetting item) => _settings.Remove(item);

    /// <summary>
    /// Gets the setting with the specified name.
    /// </summary>
    /// <param name="name">The setting name.</param>
    /// <returns>The setting with the specified name.</returns>
    /// <exception cref="KeyNotFoundException">A setting with the specified name was not found.</exception>
    public KeyValueSetting GetSetting(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return _settings.FirstOrDefault(setting => StringComparer.Ordinal.Equals(setting.Name, name))
               ?? throw new KeyNotFoundException($"A setting named '{name}' was not found.");
    }

    /// <summary>
    /// Gets the setting with the specified name, or the specified default value if it does not exist.
    /// </summary>
    /// <param name="name">The setting name.</param>
    /// <param name="defaultSetting">The value to return when the setting does not exist.</param>
    /// <returns>The setting with the specified name, or <paramref name="defaultSetting"/>.</returns>
    public KeyValueSetting? GetSettingOrDefault(string name, KeyValueSetting? defaultSetting = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return _settings.FirstOrDefault(setting => StringComparer.Ordinal.Equals(setting.Name, name)) ?? defaultSetting;
    }

    /// <summary>
    /// Gets the setting with the same name as the specified setting, or adds the specified setting.
    /// </summary>
    /// <param name="setting">The setting to get or add.</param>
    /// <returns>The existing setting, or <paramref name="setting"/> if it was added.</returns>
    public KeyValueSetting GetOrAdd(KeyValueSetting setting)
    {
        ArgumentNullException.ThrowIfNull(setting);

        KeyValueSetting? existingSetting = GetSettingOrDefault(setting.Name);
        if (existingSetting is not null)
        {
            return existingSetting;
        }

        Add(setting);
        return setting;
    }

    /// <summary>
    /// Gets the setting with the specified name, or creates and adds it using the specified factory.
    /// </summary>
    /// <param name="name">The setting name.</param>
    /// <param name="settingFactory">The factory that creates the setting.</param>
    /// <returns>The existing setting, or the setting created by <paramref name="settingFactory"/>.</returns>
    public KeyValueSetting GetOrAdd(string name, Func<string, KeyValueSetting> settingFactory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(settingFactory);

        KeyValueSetting? setting = GetSettingOrDefault(name);
        if (setting is not null)
        {
            return setting;
        }

        setting = settingFactory(name) ?? throw new InvalidOperationException("The setting factory returned null.");

        if (!StringComparer.Ordinal.Equals(setting.Name, name))
        {
            throw new InvalidOperationException($"The setting factory returned a setting named '{setting.Name}' instead of '{name}'.");
        }

        Add(setting);
        return setting;
    }

    /// <summary>
    /// Gets the setting with the specified name, or creates and adds it using the specified factory and state.
    /// </summary>
    /// <typeparam name="TState">The type of the state passed to the factory.</typeparam>
    /// <param name="name">The setting name.</param>
    /// <param name="settingFactory">The factory that creates the setting.</param>
    /// <param name="state">The state passed to the factory.</param>
    /// <returns>The existing setting, or the setting created by <paramref name="settingFactory"/>.</returns>
    public KeyValueSetting GetOrAdd<TState>(string name, Func<string, TState, KeyValueSetting> settingFactory, TState state)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(settingFactory);

        KeyValueSetting? setting = GetSettingOrDefault(name);
        if (setting is not null)
        {
            return setting;
        }

        setting = settingFactory(name, state) ?? throw new InvalidOperationException("The setting factory returned null.");

        if (!StringComparer.Ordinal.Equals(setting.Name, name))
        {
            throw new InvalidOperationException($"The setting factory returned a setting named '{setting.Name}' instead of '{name}'.");
        }

        Add(setting);
        return setting;
    }

    /// <summary>
    /// Attempts to get the setting with the specified name.
    /// </summary>
    /// <param name="name">The setting name.</param>
    /// <param name="setting">The setting when the method returns <see langword="true"/>.</param>
    /// <returns><see langword="true"/> if the setting is found; otherwise, <see langword="false"/>.</returns>
    public bool TryGetSetting(string name, [NotNullWhen(true)] out KeyValueSetting? setting)
    {
        setting = GetSettingOrDefault(name);
        return setting is not null;
    }

    /// <inheritdoc />
    protected override void InitializeCore()
    {
        base.InitializeCore();

        foreach (KeyValueSetting setting in _settings)
        {
            TrackSetting(setting);
        }

        _settings.CollectionChanged += OnSettingsCollectionChanged;
    }

    private void OnSettingsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is { Count: > 0 })
        {
            foreach (KeyValueSetting setting in e.OldItems)
            {
                UntrackSetting(setting);
            }
        }

        if (e.NewItems is { Count: > 0 })
        {
            foreach (KeyValueSetting setting in e.NewItems)
            {
                TrackSetting(setting);
            }
        }

        MarkChanged();
    }

    private void TrackSetting(KeyValueSetting setting)
    {
        setting.Initialize();
        setting.PropertyChanged -= OnSettingPropertyChanged;
        setting.PropertyChanged += OnSettingPropertyChanged;
    }

    private void UntrackSetting(KeyValueSetting setting)
    {
        setting.PropertyChanged -= OnSettingPropertyChanged;
        setting.Uninitialize();
    }

    private void OnSettingPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        var setting = (KeyValueSetting)sender!;

        if (e.PropertyName is nameof(setting.IsChanged))
        {
            if (setting.IsChanged)
            {
                MarkChanged();
            }

            return;
        }

        if (e.PropertyName is nameof(setting.Value))
        {
            SettingChanged?.Invoke(this, new SettingChangedEventArgs(setting));
        }
    }

    /// <inheritdoc />
    protected override void UninitializeCore()
    {
        _settings.CollectionChanged -= OnSettingsCollectionChanged;

        foreach (KeyValueSetting setting in _settings)
        {
            UntrackSetting(setting);
        }

        base.UninitializeCore();
    }

    /// <inheritdoc />
    protected override void AcceptChangesCore()
    {
        foreach (KeyValueSetting setting in _settings)
        {
            setting.AcceptChanges();
        }
    }
}
