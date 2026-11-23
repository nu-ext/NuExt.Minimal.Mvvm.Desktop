using System;
using System.Collections.Generic;

namespace Minimal.Mvvm;

/// <summary>
/// Provides content loading for <see cref="KeyValueSettingCollection"/>.
/// </summary>
public static class KeyValueSettingCollectionExtensions
{
    /// <summary>
    /// Replaces the collection content with the specified settings without treating the load as a change.
    /// </summary>
    /// <typeparam name="TCollection">The collection type.</typeparam>
    /// <param name="collection">The collection to fill.</param>
    /// <param name="settings">The settings to load.</param>
    /// <param name="callback">An optional callback invoked inside the suspended tracking scope, after the settings are added.</param>
    /// <exception cref="ArgumentNullException"><paramref name="collection"/> or <paramref name="settings"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// The collection is cleared and filled inside a single suspended change-tracking scope, so listeners
    /// do not observe intermediate states. The resulting state is accepted, so a freshly loaded collection
    /// is not marked as changed.
    /// </remarks>
    public static void Load<TCollection>(this TCollection collection, IEnumerable<KeyValueSetting> settings,
        Action<TCollection>? callback = null)
        where TCollection : KeyValueSettingCollection
    {
        ArgumentNullException.ThrowIfNull(collection);
        ArgumentNullException.ThrowIfNull(settings);

        using (collection.SuspendChangeTracking())
        {
            collection.Clear();

            foreach (var setting in settings)
            {
                collection.Add(setting);
            }

            callback?.Invoke(collection);
        }

        collection.AcceptChanges();
    }
}
