# NuExt.Minimal.Mvvm.Desktop

A platform-agnostic base layer for desktop MVVM applications. It combines [NuExt.Minimal.Mvvm](https://github.com/nu-ext/NuExt.Minimal.Mvvm) with [NuExt.System](https://github.com/nu-ext/NuExt.System) and holds the parts that are useful for any UI stack ? WPF, Avalonia, WinUI, MAUI ? without referencing a single UI framework.

## Contents

| Type | Purpose |
|---|---|
| `InitializableBase` | Idempotent `Initialize` / `Uninitialize` lifecycle on top of `BindableBase`. |
| `ChangeTrackingBase` | Adds `IChangeTracking` with `AcceptChanges` and suspended tracking. |
| `SettingsBase` | Change tracking plus a read-only mode, unknown-JSON preservation and reflection access by property name. |
| `ISettingsService` | Loads and saves `SettingsBase` to JSON files. |
| `ExpandableBase` | `IExpandable` support. |
| `JsonObjectStateSerializer` | Serializes and restores object state as JSON, keeping unrecognized properties. |
| `EnvironmentServiceBase` | Application directories: base, config, app data, working, logs, settings. |

## Targets

`netstandard2.0`, `netstandard2.1`, `net462`, `net471`, `net8.0`, `net9.0`, `net10.0`, `net11.0`.

No `-windows` suffix on purpose: the package contains no Windows-specific code, so a cross-platform application can reference it directly.

## License

MIT.
