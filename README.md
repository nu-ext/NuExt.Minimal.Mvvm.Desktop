# NuExt.Minimal.Mvvm.Desktop

`NuExt.Minimal.Mvvm.Desktop` is a **platform-agnostic base layer for desktop MVVM applications**. It combines [NuExt.Minimal.Mvvm](https://github.com/nu-ext/NuExt.Minimal.Mvvm) with [NuExt.System](https://github.com/nu-ext/NuExt.System) and holds the parts that are useful for any UI stack - WPF, Avalonia, WinUI, MAUI - without referencing a single UI framework.

[![NuGet](https://img.shields.io/nuget/v/NuExt.Minimal.Mvvm.Desktop.svg)](https://www.nuget.org/packages/NuExt.Minimal.Mvvm.Desktop)
[![Ask DeepWiki](https://img.shields.io/badge/Ask-DeepWiki-blue)](https://deepwiki.com/nu-ext/NuExt.Minimal.Mvvm.Desktop)
[![Build](https://github.com/nu-ext/NuExt.Minimal.Mvvm.Desktop/actions/workflows/ci.yml/badge.svg)](https://github.com/nu-ext/NuExt.Minimal.Mvvm.Desktop/actions/workflows/ci.yml)
[![License](https://img.shields.io/github/license/nu-ext/NuExt.Minimal.Mvvm.Desktop?label=license)](https://github.com/nu-ext/NuExt.Minimal.Mvvm.Desktop/blob/main/LICENSE)
[![Downloads](https://img.shields.io/nuget/dt/NuExt.Minimal.Mvvm.Desktop.svg)](https://www.nuget.org/packages/NuExt.Minimal.Mvvm.Desktop)

## Why NuExt.Minimal.Mvvm.Desktop?

- **Deterministic lifecycle** - `InitializableBase` provides an idempotent `Initialize` / `Uninitialize` pair with matching cleanup.
- **Change tracking** - `ChangeTrackingBase` adds `IChangeTracking`, `AcceptChanges`, and a disposable suspended tracking scope.
- **Settings** - JSON settings objects (`SettingsBase` + `ISettingsService`) and flat key/value settings (`KeyValueSetting` + `KeyValueSettingCollection`) stay separate on purpose.
- **No UI framework** - no WPF, Avalonia, WinUI, or MAUI reference, so every desktop stack can share the same layer.
- **Allocation-conscious models** - change tracking, notifications, and settings conversion avoid hidden allocations in hot paths.

## API Overview

- **`InitializableBase`**: idempotent `Initialize` / `Uninitialize` lifecycle on top of `BindableBase`.
- **`ChangeTrackingBase`**: `IChangeTracking` with `AcceptChanges` and suspended tracking.
- **`SettingsBase`**: strongly typed settings object with change tracking, read-only mode, unknown-JSON preservation, and reflection access by property name.
- **`ISettingsService`**: loads and saves `SettingsBase` to JSON files.
- **`KeyValueSetting`**: a single named setting stored as a raw string with an explicit `Type`, typed accessors, and `GetValue<T>()`.
- **`KeyValueSettingCollection`**: change-tracked collection of `KeyValueSetting` that raises `SettingChanged`.
- **`SettingChangedEventArgs`**: notification payload for a changed setting value.
- **`ExpandableBase`**: `IExpandable` support.
- **`JsonObjectStateSerializer`**: serializes and restores object state as JSON, keeping unrecognized properties.
- **`EnvironmentServiceBase`**: application directories - base, config, app data, working, logs, settings.

## Settings

The two settings pipelines are intentionally separate.

- `SettingsBase` with `ISettingsService` persists a strongly typed object graph as JSON.
- `KeyValueSetting` with `KeyValueSettingCollection` models flat key/value rows: each value is a raw string converted on demand using `ValueType`, which is what a database-backed settings table stores.

## Compatibility

- **.NET Standard 2.0+, .NET 8/9/10/11, and .NET Framework 4.6.2+**
- Works with WPF, Avalonia, WinUI, and MAUI applications
- Does not reference any UI framework

## Installation

Via [NuGet](https://www.nuget.org/):

```shell
dotnet add package NuExt.Minimal.Mvvm.Desktop
```

Or via Visual Studio:

1. Go to `Tools -> NuGet Package Manager -> Manage NuGet Packages for Solution...`.
2. Search for `NuExt.Minimal.Mvvm.Desktop`.
3. Click "Install".

## Ecosystem

- [NuExt.System](https://github.com/nu-ext/NuExt.System)
- [NuExt.System.Data](https://github.com/nu-ext/NuExt.System.Data)
- [NuExt.System.Data.SQLite](https://github.com/nu-ext/NuExt.System.Data.SQLite)
- [NuExt.Minimal.Mvvm](https://github.com/nu-ext/NuExt.Minimal.Mvvm)
- [NuExt.Minimal.Mvvm.SourceGenerator](https://github.com/nu-ext/NuExt.Minimal.Mvvm.SourceGenerator)
- [NuExt.Minimal.Behaviors.Wpf](https://github.com/nu-ext/NuExt.Minimal.Behaviors.Wpf)
- [NuExt.Presentation.Wpf](https://github.com/nu-ext/NuExt.Presentation.Wpf)
- [NuExt.Minimal.Mvvm.Wpf](https://github.com/nu-ext/NuExt.Minimal.Mvvm.Wpf)
- [NuExt.Minimal.Mvvm.AvalonDock](https://github.com/nu-ext/NuExt.Minimal.Mvvm.AvalonDock)
- [NuExt.Minimal.Mvvm.MahApps.Metro](https://github.com/nu-ext/NuExt.Minimal.Mvvm.MahApps.Metro)
- [NuExt.Minimal.Mvvm.Desktop.Persistence](https://github.com/nu-ext/NuExt.Minimal.Mvvm.Desktop.Persistence)

## Contributing

Issues and PRs are welcome. Keep changes minimal and performance-conscious.

## License

MIT. See LICENSE.
