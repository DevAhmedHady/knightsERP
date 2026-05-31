using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Knights.Domain.Common;

public abstract class BaseEntity : IEquatable<BaseEntity>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private string? _additionalPropertiesJson;

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; protected set; }

    [NotMapped]
    public Dictionary<string, object?> AdditionalProperties
    {
        get => string.IsNullOrWhiteSpace(_additionalPropertiesJson)
            ? new Dictionary<string, object?>()
            : JsonSerializer.Deserialize<Dictionary<string, object?>>(_additionalPropertiesJson, JsonOptions)
              ?? new Dictionary<string, object?>();
        private set => _additionalPropertiesJson = value.Count == 0
            ? null
            : JsonSerializer.Serialize(value, JsonOptions);
    }

    public abstract bool Equals(BaseEntity? other);

    public override bool Equals(object? obj)
    {
        return obj is BaseEntity entity && Equals(entity);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public void SetAdditionalProperty(string key, object? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));

        var props = AdditionalProperties;
        props[key] = value;
        AdditionalProperties = props;
    }

    public TValue? GetAdditionalProperty<TValue>(string key, TValue? defaultValue = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            return defaultValue;

        var props = AdditionalProperties;
        if (!props.TryGetValue(key, out var value) || value is null)
            return defaultValue;

        try
        {
            if (value is JsonElement jsonElement)
                return JsonSerializer.Deserialize<TValue>(jsonElement.GetRawText(), JsonOptions);

            if (value is TValue typedValue)
                return typedValue;

            return (TValue)Convert.ChangeType(value, typeof(TValue));
        }
        catch
        {
            return defaultValue;
        }
    }

    public bool HasAdditionalProperty(string key)
    {
        return !string.IsNullOrWhiteSpace(key) && AdditionalProperties.ContainsKey(key);
    }

    public bool RemoveAdditionalProperty(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return false;

        var props = AdditionalProperties;
        var removed = props.Remove(key);
        if (removed)
            AdditionalProperties = props;

        return removed;
    }

    public void ClearAdditionalProperties()
    {
        AdditionalProperties = new Dictionary<string, object?>();
    }
}
