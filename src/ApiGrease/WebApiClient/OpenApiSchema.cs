//using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using JsonProperty = Newtonsoft.Json.JsonPropertyAttribute;
using NullValueHandling = Newtonsoft.Json.NullValueHandling;
namespace WebApiClient
{

    public class OpenApiSerializer
    {
        private static JsonSerializerOptions serializerOptions = null!;

        static OpenApiSerializer()
        {
            var opts = new JsonSerializerOptions();
            opts.Converters.Add(new ApiComponentSchemaConverter());
            //opts.Converters.Add(new ApiObjectPropertyConverter());

            serializerOptions = opts;
        }
        public static OpenApiSchema FromJson(string json)
        {
            var result = JsonSerializer.Deserialize<OpenApiSchema>(json, serializerOptions);
            return result;
        }

        public static string ToJson(OpenApiSchema schema)
        {
            var result = JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true });
            return result;
        }
    }

    public abstract class ApiPropertyConvert<T> : JsonConverter<T>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeof(T).IsAssignableFrom(typeToConvert);
        }
        public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
        {
            //if (value is null)
            //    return;
            var typeOf = value is null ? typeof(T) : value.GetType();
            JsonSerializer.Serialize(writer, value, typeOf, new JsonSerializerOptions() { WriteIndented = true });
        }
    }


    public class ApiArrayItemDefintionConverter : ApiPropertyConvert<ApiArrayItemDefintion>
    {


        public override ApiArrayItemDefintion? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (var doc = JsonDocument.ParseValue(ref reader))
            {
                var elementText = doc.RootElement.GetRawText();

                bool hasRef = doc.RootElement.TryGetProperty("$ref", out JsonElement refValue);
                if (hasRef)
                {

                    var refDef = JsonSerializer.Deserialize<ApiArrayItemRefDefintion>(doc.RootElement.GetRawText());
                    return (ApiArrayItemDefintion?)refDef;
                }
                else
                {
                    var itemDef = JsonSerializer.Deserialize<ApiArrayItemTypeDefintion>(doc.RootElement.GetRawText());
                    return (ApiArrayItemDefintion?)itemDef;
                }
            }
        }


    }

    internal class ApiObjectPropertyConverter : ApiPropertyConvert<ApiObjectProperty>
    {

        static JsonSerializerOptions childOptions;
        static ApiObjectPropertyConverter()
        {
            childOptions = new JsonSerializerOptions();
            childOptions.Converters.Add(new ApiArrayItemDefintionConverter());
        }
        public override ApiObjectProperty? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (var doc = JsonDocument.ParseValue(ref reader))
            {
                bool hasRef = doc.RootElement.TryGetProperty("$ref", out JsonElement refValue);
                if (hasRef)
                {

                    var result = new ApiRefProperty() { Ref = refValue.ToString() };
                    return result;

                    //var refDef = JsonSerializer.Deserialize<ApiArrayItemRefDefintion>(doc.RootElement.GetRawText());
                    //return (ApiArrayItemDefintion?)refDef;
                }

                var type = doc.RootElement.GetProperty("type").ToString();
                if (type == "array")
                {

                    var arrayDef = JsonSerializer.Deserialize<ApiArrayDefinition>(doc.RootElement.GetRawText(), childOptions);
                    return (ApiObjectProperty?)arrayDef;
                }
                else
                {
                    var objectDef = JsonSerializer.Deserialize<ApiObjectDefinition>(doc.RootElement.GetRawText(), childOptions);
                    return (ApiObjectProperty?)objectDef;
                }
            }
        }


    }

    internal class ApiComponentSchemaConverter : ApiPropertyConvert<ApiComponentSchema>
    {

        static JsonSerializerOptions childOptions;
        static ApiComponentSchemaConverter()
        {
            childOptions = new JsonSerializerOptions();
            childOptions.Converters.Add(new ApiObjectPropertyConverter());
        }

        public override ApiComponentSchema? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (var doc = JsonDocument.ParseValue(ref reader))
            {
                bool hasRef = doc.RootElement.TryGetProperty("enum", out JsonElement refValue);
                if (hasRef)
                {
                    var enumDef = JsonSerializer.Deserialize<ApiEnumComponent>(doc.RootElement.GetRawText(), childOptions);
                    return (ApiComponentSchema?)enumDef;
                }
                else
                {
                    var objectDef = JsonSerializer.Deserialize<ApiObjectComponent>(doc.RootElement.GetRawText(), childOptions);
                    return (ApiComponentSchema?)objectDef;
                }
            }
        }

    }

    public class OpenApiSchema
    {
        [JsonPropertyName("openapi")]
        [JsonProperty("openapi")]
        public string SchemanVersion { get; set; }

        [JsonPropertyName("info")]
        [JsonProperty("info")]
        public ApiInfo ApiInfo { get; set; }


        [JsonPropertyName("servers")]
        [JsonProperty("servers")]
        public ApiServers[] ApiServers { get; set; }
        [JsonPropertyName("paths")]
        [JsonProperty("paths")]
        public Dictionary<string, Dictionary<string, ApiPathSpec>> ApiPaths { get; set; }

        [JsonPropertyName("components")]
        [JsonProperty("components")]
        public ApiComponents ApiComponents { get; set; }
    }

    public class ApiInfo
    {
        public string title { get; set; }
        public string version { get; set; }
    }

    public class ApiPath
    {
        public Dictionary<string, ApiPathSpec> Endpoints { get; set; }
    }
    public class ApiPathSpec
    {
        [JsonPropertyName("tags")]
        [JsonProperty("tags", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Tags { get; set; }

        [JsonPropertyName("parameters")]
        [JsonProperty("parameters", NullValueHandling = NullValueHandling.Ignore)]
        public List<ApiParameter> Parameters { get; set; }
        [JsonPropertyName("requestBody")]
        [JsonProperty("requestBody", NullValueHandling = NullValueHandling.Ignore)]
        public ApiContentType? Requests { get; set; }
        [JsonPropertyName("responses")]
        [JsonProperty("responses", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, ApiResponse> Responses { get; set; }
    }

    public class ApiParameter
    {
        [JsonPropertyName("name")]
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }
        [JsonPropertyName("in")]
        [JsonProperty("in", NullValueHandling = NullValueHandling.Ignore)]
        public string In { get; set; }
        [JsonPropertyName("required")]
        [JsonProperty("required", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Required { get; set; }
        [JsonPropertyName("schema")]
        [JsonProperty("schema", NullValueHandling = NullValueHandling.Ignore)]
        public ApiParameterSchema Schema { get; set; }
    }
    public class ApiParameterSchema
    {
        [JsonPropertyName("$ref")]
        [JsonProperty("$ref", NullValueHandling = NullValueHandling.Ignore)]
        public string Ref { get; set; }
        [JsonPropertyName("type")]
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]

        public string Type { get; set; }
        [JsonPropertyName("format")]
        [JsonProperty("format", NullValueHandling = NullValueHandling.Ignore)]
        public string Format { get; set; }
    }

    public class ApiContentType
    {
        [JsonPropertyName("content")]
        [JsonProperty("content", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, ApiContentSchema> RequestSchema { get; set; }
    }
    public class ApiContentSchema
    {
        [JsonPropertyName("schema")]
        [JsonProperty("schema", NullValueHandling = NullValueHandling.Ignore)]
        public ApiContentSchemaRef RequestSchema { get; set; }
    }
    public class ApiContentSchemaRef
    {
        [JsonPropertyName("$ref")]
        [JsonProperty("$ref", NullValueHandling = NullValueHandling.Ignore)]
        public string Ref { get; set; }
    }

    public class ApiResponse
    {
        [JsonPropertyName("description")]
        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonPropertyName("content")]
        [JsonProperty("content", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, ApiContentSchema> ResponseContentTypes { get; set; }
    }

    public class ApiServers
    {
        public string url { get; set; }
    }

    public class ApiComponents
    {
        [JsonPropertyName("schemas")]
        [JsonProperty("schemas", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, ApiComponentSchema> ComponentSchemas { get; set; }

    }

    public class ApiComponentSchema
    {
        [JsonPropertyName("type")]
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore, Order = 0)]
        public string ComponentType { get; set; }
    }

    public class ApiEnumComponent : ApiComponentSchema
    {
        [JsonPropertyName("enum")]
        [JsonProperty("enum", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> EnumValues { get; set; }
    }
    public class ApiObjectComponent : ApiComponentSchema
    {
        [JsonPropertyName("properties")]
        [JsonProperty("properties", NullValueHandling = NullValueHandling.Ignore, Order = 1)]
        public Dictionary<string, ApiObjectProperty>? Properties { get; set; }
        [JsonPropertyName("additionalProperties")]
        [JsonProperty("additionalProperties", NullValueHandling = NullValueHandling.Ignore, Order = 2)]
        public bool AdditionalProperties { get; set; }
    }
    public class ApiObjectProperty
    {

    }
    public class ApiRefProperty : ApiObjectProperty
    {
        [JsonPropertyName("$ref")]
        [JsonProperty("$ref", NullValueHandling = NullValueHandling.Ignore)]
        public string Ref { get; set; }
    }
    public class ApiObjectDefinition : ApiObjectProperty
    {

        [JsonPropertyName("type")]
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore, Order = 0)]
        public string ObjectType { get; set; }

        [JsonPropertyName("format")]
        [JsonProperty("format", NullValueHandling = NullValueHandling.Ignore, Order = 1)]
        public string? Format { get; set; }

        [JsonPropertyName("nullable")]
        [JsonProperty("nullable", NullValueHandling = NullValueHandling.Ignore, Order = 2)]
        public bool? IsNullable { get; set; }
    }
    public class ApiArrayDefinition : ApiObjectDefinition
    {
        [JsonPropertyName("items")]
        [JsonProperty("items", Order = 1)]
        public ApiArrayItemDefintion ItemsDefinition { get; set; }

    }
    public class ApiArrayItemDefintion { }
    public class ApiArrayItemRefDefintion : ApiArrayItemDefintion
    {
        [JsonPropertyName("$ref")]
        [JsonProperty("$ref")]
        public string RefType { get; set; }
    }
    public class ApiArrayItemTypeDefintion : ApiArrayItemDefintion
    {
        [JsonPropertyName("type")]
        [JsonProperty("type", Order = 0)]
        public string ObjectType { get; set; }
    }
}
