using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Minimal.Mvvm.Reflection;

/// <summary>
/// Provides reflection-based access to the serializable properties of objects.
/// </summary>
/// <remarks>
/// The property map is cached per concrete runtime type. Properties marked with
/// <see cref="JsonIgnoreAttribute"/> or <see cref="JsonExtensionDataAttribute"/> are excluded.
/// </remarks>
public static class PropertyHelper
{
    private static readonly ConcurrentDictionary<Type, IReadOnlyDictionary<string, PropertyInfo>> s_typeProperties = new();
    private static readonly Func<Type, Type, IReadOnlyDictionary<string, PropertyInfo>> s_typePropertiesFactory = GetProperties;

    /// <summary>
    /// Builds the serializable property map for the specified type.
    /// </summary>
    /// <param name="baseType">The type up to which the property hierarchy is walked.</param>
    /// <param name="type">The type to get properties for.</param>
    /// <returns>A dictionary containing property information.</returns>
    private static IReadOnlyDictionary<string, PropertyInfo> GetProperties(Type baseType, Type type)
    {
        var properties = type.GetAllProperties(baseType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
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
    /// Gets the serializable properties of the specified object.
    /// </summary>
    /// <typeparam name="T">The type up to which the property hierarchy is walked.</typeparam>
    /// <param name="obj">The object to inspect.</param>
    /// <returns>A dictionary containing property information.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="obj"/> is <see langword="null"/>.</exception>
    public static IReadOnlyDictionary<string, PropertyInfo> GetProperties<T>(T obj)
    {
        ArgumentNullException.ThrowIfNull(obj);
        return s_typeProperties.GetOrAdd(obj.GetType(), type => s_typePropertiesFactory(typeof(T), type));
    }

    /// <summary>
    /// Attempts to get the type of the named property of the specified object.
    /// </summary>
    /// <typeparam name="T">The type up to which the property hierarchy is walked.</typeparam>
    /// <param name="obj">The object to inspect.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="propertyType">The type of the property when the method returns <see langword="true"/>.</param>
    /// <returns><see langword="true"/> if the property is known; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="obj"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="propertyName"/> is <see langword="null"/> or empty.</exception>
    public static bool TryGetPropertyType<T>(T obj, string propertyName, [NotNullWhen(true)] out Type? propertyType)
    {
        ArgumentNullException.ThrowIfNull(obj);
        ArgumentException.ThrowIfNullOrEmpty(propertyName);

        if (GetProperties(obj).TryGetValue(propertyName, out PropertyInfo? property))
        {
            propertyType = property.PropertyType;
            return true;
        }

        propertyType = null;
        return false;
    }

    /// <summary>
    /// Attempts to get the value of the named property of the specified object.
    /// </summary>
    /// <typeparam name="T">The type up to which the property hierarchy is walked.</typeparam>
    /// <param name="obj">The object to read the value from.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="value">The value of the property when the method returns <see langword="true"/>.</param>
    /// <returns><see langword="true"/> if the value was read; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="obj"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="propertyName"/> is <see langword="null"/> or empty.</exception>
    public static bool TryGetProperty<T>(T obj, string propertyName, out object? value)
    {
        ArgumentNullException.ThrowIfNull(obj);
        ArgumentException.ThrowIfNullOrEmpty(propertyName);

        if (GetProperties(obj).TryGetValue(propertyName, out PropertyInfo? property) && property.CanRead)
        {
            value = property.GetValue(obj);
            return true;
        }

        value = null;
        return false;
    }

    /// <summary>
    /// Attempts to assign a value to the named property of the specified object.
    /// </summary>
    /// <typeparam name="T">The type up to which the property hierarchy is walked.</typeparam>
    /// <param name="obj">The object to write the value to.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="value">The value to assign.</param>
    /// <returns><see langword="true"/> if the value was assigned; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="obj"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="propertyName"/> is <see langword="null"/> or empty.</exception>
    public static bool TrySetProperty<T>(T obj, string propertyName, object? value)
    {
        ArgumentNullException.ThrowIfNull(obj);
        ArgumentException.ThrowIfNullOrEmpty(propertyName);

        if (!GetProperties(obj).TryGetValue(propertyName, out PropertyInfo? property) || !property.CanWrite)
            return false;

        property.SetValue(obj, value);
        return true;
    }
}
