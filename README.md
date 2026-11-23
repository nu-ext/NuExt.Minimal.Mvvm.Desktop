# NuExt.Minimal.Mvvm.Desktop

A platform-agnostic base layer for desktop MVVM applications. It combines [NuExt.Minimal.Mvvm](https://github.com/nu-ext/NuExt.Minimal.Mvvm) with [NuExt.System](https://github.com/nu-ext/NuExt.System) and holds the parts that are useful for any UI stack - WPF, Avalonia, WinUI, MAUI - without referencing a single UI framework.

## Contents

| Type | Purpose |
|---|---|
| `InitializableBase` | Idempotent `Initialize` / `Uninitialize` lifecycle on top of `BindableBase`. |
| `ChangeTrackingBase` | Adds `IChangeTracking` with `AcceptChanges` and suspended tracking. |
| `SettingsBase` | Strongly typed settings object: change tracking, read-only mode, unknown-JSON preservation and reflection access by property name. |
| `ISettingsService` | Loads and saves `SettingsBase` to JSON files. |
| `KeyValueSetting` | A single named setting stored as a raw string with an explicit `Type`, typed accessors and `GetValue<T>()`. |
| `KeyValueSettingCollection` | Change-tracked collection of `KeyValueSetting` that raises `SettingChanged`. |
| `SettingValueTypes` | Persisted value-type tokens and `Type` to token mapping for storage layers. |
| `SettingChangedEventArgs` | Notification payload for a changed setting value. |
| `ExpandableBase` | `IExpandable` support. |
| `JsonObjectStateSerializer` | Serializes and restores object state as JSON, keeping unrecognized properties. |
| `EnvironmentServiceBase` | Application directories: base, config, app data, working, logs, settings. |

Two settings pipelines are intentionally separate. `SettingsBase` with `ISettingsService` persists a strongly typed object graph as JSON. `KeyValueSetting` with `KeyValueSettingCollection` models flat key/value rows: each value is a raw string converted on demand using `ValueType`, which is what a database-backed settings table stores. `SettingValueTypes` bridges `Type` and the persisted token.

## Targets

`netstandard2.0`, `netstandard2.1`, `net462`, `net471`, `net8.0`, `net9.0`, `net10.0`, `net11.0`.

No `-windows` suffix on purpose: the package contains no Windows-specific code, so a cross-platform application can reference it directly.

## License

MIT.
