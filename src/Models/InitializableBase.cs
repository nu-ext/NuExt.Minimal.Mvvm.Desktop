using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Threading;

namespace Minimal.Mvvm;

/// <summary>
/// Provides a base class for objects with a deterministic synchronous lifecycle:
/// an idempotent initialization with a matching idempotent cleanup.
/// </summary>
/// <remarks>
/// A failed initialization leaves the object uninitialized, so a later call retries.
/// The core methods run under the lock and must not call back into <see cref="Initialize"/>
/// or <see cref="Uninitialize"/> on the same instance.
/// </remarks>
public abstract class InitializableBase : BindableBase, IInitializable
{
    private readonly Lock _initLock = new();
    private volatile bool _isInitialized;

    /// <summary>
    /// Gets a value indicating whether the object has been initialized.
    /// </summary>
    [JsonIgnore]
    public bool IsInitialized => _isInitialized;

    /// <inheritdoc />
    public void Initialize()
    {
        lock (_initLock)
        {
            if (_isInitialized) return;
            InitializeCore();
            _isInitialized = true;
        }

        OnPropertyChanged(EventArgsCache.IsInitializedPropertyChanged);
    }

    /// <summary>
    /// Releases the resources acquired by <see cref="Initialize"/>.
    /// </summary>
    /// <remarks>
    /// Calling this method on an uninitialized object does nothing.
    /// </remarks>
    public void Uninitialize()
    {
        lock (_initLock)
        {
            if (!_isInitialized) return;
            UninitializeCore();
            _isInitialized = false;
        }

        OnPropertyChanged(EventArgsCache.IsInitializedPropertyChanged);
    }

    /// <summary>
    /// Called by <see cref="Initialize"/> to perform the initialization.
    /// </summary>
    protected virtual void InitializeCore()
    {
    }

    /// <summary>
    /// Called by <see cref="Uninitialize"/> to release the resources acquired during initialization.
    /// </summary>
    protected virtual void UninitializeCore()
    {
    }
}

internal static partial class EventArgsCache
{
    internal static readonly PropertyChangedEventArgs IsInitializedPropertyChanged = new(nameof(InitializableBase.IsInitialized));
}
