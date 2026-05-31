using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Domain.Domain.Base
{
    public abstract class BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string JsonData<T>()
        {
            return JsonConvert.SerializeObject(typeof(T));
        }

        public override string ToString()
        {
            return JsonData<BaseEntity>();
        }

        public abstract bool Equals(BaseEntity other);

        private string? _additionalPropertiesJson { get; set; }

        //public void Add<T>() where T : BaseEntity;
        [NotMapped]
        public Dictionary<string, object?> AdditionalProperties
        {
            get => string.IsNullOrWhiteSpace(_additionalPropertiesJson)
                ? new Dictionary<string, object?>()
                : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object?>>(_additionalPropertiesJson, JsonOptions)
                  ?? new Dictionary<string, object?>();
            private set => _additionalPropertiesJson = value == null || value.Count == 0
                ? null
                : System.Text.Json.JsonSerializer.Serialize(value, JsonOptions);
        }

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        #region Additional Properties Methods

        /// <summary>
        /// Sets or updates an additional property value.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <param name="value">The property value. Must be JSON-serializable.</param>
        public void SetAdditionalProperty(string key, object? value)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));
            var props = AdditionalProperties;
            props[key] = value;
            AdditionalProperties = props;
        }

        /// <summary>
        /// Gets an additional property value by key.
        /// </summary>
        /// <typeparam name="TValue">The expected type of the value.</typeparam>
        /// <param name="key">The property key.</param>
        /// <param name="defaultValue">Default value if key not found or conversion fails.</param>
        /// <returns>The property value or default.</returns>
        public TValue? GetAdditionalProperty<TValue>(string key, TValue? defaultValue = default)
        {
            if (string.IsNullOrWhiteSpace(key))
                return defaultValue;

            var props = AdditionalProperties;
            if (!props.TryGetValue(key, out var value) || value == null)
                return defaultValue;

            try
            {
                // Handle JsonElement from deserialization
                if (value is JsonElement jsonElement)
                {
                    return System.Text.Json.JsonSerializer.Deserialize<TValue>(jsonElement.GetRawText(), JsonOptions);
                }

                // Direct cast if types match
                if (value is TValue typedValue)
                    return typedValue;

                // Try conversion for primitives
                return (TValue)Convert.ChangeType(value, typeof(TValue));
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Checks if an additional property exists.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <returns>True if the property exists.</returns>
        public bool HasAdditionalProperty(string key)
        {
            return !string.IsNullOrWhiteSpace(key) && AdditionalProperties.ContainsKey(key);
        }

        /// <summary>
        /// Removes an additional property by key.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <returns>True if the property was removed.</returns>
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

        /// <summary>
        /// Clears all additional properties.
        /// </summary>
        public void ClearAdditionalProperties()
        {
            AdditionalProperties = new Dictionary<string, object?>();
        }

        /// <summary>
        /// Sets multiple additional properties at once.
        /// </summary>
        /// <param name="properties">Dictionary of properties to set.</param>
        public void SetAdditionalProperties(Dictionary<string, object?> properties)
        {
            if (properties == null || properties.Count == 0)
                return;

            var props = AdditionalProperties;
            foreach (var kvp in properties)
            {
                if (!string.IsNullOrWhiteSpace(kvp.Key))
                    props[kvp.Key] = kvp.Value;
            }
            AdditionalProperties = props;
        }

        #endregion
    }
}
