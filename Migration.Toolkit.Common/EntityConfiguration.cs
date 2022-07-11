using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Migration.Toolkit.Common.Helpers;

namespace Migration.Toolkit.Common;

public class EntityConfigurations : Dictionary<string, EntityConfiguration>
{
    public EntityConfigurations(): base(StringComparer.OrdinalIgnoreCase)
    {
        
    }
    
    public EntityConfiguration GetEntityConfiguration(string? tableName)
    {
        if (this.TryGetValue(tableName ?? "*", out var result))
        {
            return result;
        }

        return this.TryGetValue("*", out var @default) ? @default : new EntityConfiguration();
    }

    public EntityConfiguration GetEntityConfiguration<TModel>()
    {
        var tableName = ReflectionHelper<TModel>.GetFirstAttributeOrNull<TableAttribute>()?.Name;
        return GetEntityConfiguration(tableName);
    }

    public void SetEntityConfiguration<TModel>(EntityConfiguration config)
    {
        var tableName = ReflectionHelper<TModel>.GetFirstAttributeOrNull<TableAttribute>()?.Name;
        if (this.ContainsKey(tableName))
        {
            this[tableName] = config;
        }
        else
        {
            this.Add(tableName, config);
        }
    }
}

public class EntityConfiguration
{
    [JsonPropertyName("ExcludeCodeNames")]
    public string[] ExcludeCodeNames { get; set; } = Array.Empty<string>();
    
    [JsonPropertyName("ExplicitPrimaryKeyMapping")]
    public Dictionary<string, Dictionary<string, int?>> ExplicitPrimaryKeyMapping { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}