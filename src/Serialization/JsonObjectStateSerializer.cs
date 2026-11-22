using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json;

namespace Minimal.Mvvm.Serialization;

/// <summary>
/// Provides methods for serializing and deserializing object state as JSON.
/// </summary>
public static class JsonObjectStateSerializer
{
    private static readonly JsonDocumentOptions s_documentOptions = new()
    {
        CommentHandling = JsonCommentHandling.Skip,
    };

    /// <summary>
    /// Deserializes JSON object properties and assigns their values to the specified target.
    /// </summary>
    /// <param name="json">The JSON string containing the object state.</param>
    /// <param name="target">The object whose properties receive the deserialized values.</param>
    /// <param name="options">Options for JSON deserialization.</param>
    /// <returns>
    /// A dictionary containing properties the target does not define, or <see langword="null"/>
    /// if all properties were recognized.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="target"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">
    /// A deserialized property value could not be assigned.
    /// </exception>
    /// <exception cref="JsonException">
    /// The JSON is invalid, does not represent an object, or a property value cannot be deserialized
    /// to its expected type.
    /// </exception>
    public static Dictionary<string, JsonElement>? Deserialize(string json, IReflectableProperties target, JsonSerializerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(target);

        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        Dictionary<string, JsonElement>? additionalData = null;

        using var document = JsonDocument.Parse(json, s_documentOptions);
        JsonElement root = document.RootElement;

        foreach (JsonProperty property in root.EnumerateObject())
        {
            string name = property.Name;
            var value = property.Value;

            if (!target.TryGetPropertyType(name, out Type? type))
            {
                additionalData ??= [];
                additionalData[name] = value.Clone();
                continue;
            }

            var propertyValue = value.Deserialize(type, options);
            if (!target.TrySetProperty(name, propertyValue))
            {
                throw new InvalidOperationException($"Failed to set property '{name}'.");
            }
        }

        return additionalData;
    }

    /// <summary>
    /// Serializes an object's runtime state to a JSON string.
    /// </summary>
    /// <typeparam name="T">The declared type of the object.</typeparam>
    /// <param name="source">The object to serialize.</param>
    /// <param name="options">Options for JSON serialization.</param>
    /// <returns>A JSON string, or <see langword="null"/> if <paramref name="source"/> is <see langword="null"/>.</returns>
    public static string? Serialize<T>(T? source, JsonSerializerOptions? options = null)
    {
        if (source is null)
        {
            return null;
        }

        return JsonSerializer.Serialize(source, source.GetType(), options);
    }
}
