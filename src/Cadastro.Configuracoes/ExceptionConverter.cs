using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cadastro.Configuracoes
{
    //https://github.com/dotnet/runtime/issues/43026
    [ExcludeFromCodeCoverage]
    public class ExceptionConverter : JsonConverter<Exception>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeof(Exception).IsAssignableFrom(typeToConvert);
        }

        public override Exception Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotSupportedException("Deserializing exceptions is not allowed");
        }

        public override void Write(Utf8JsonWriter writer, Exception value, JsonSerializerOptions options)
        {
            var serializableProperties = value.GetType()
                .GetProperties()
                .Select(uu => new { uu.Name, Value = uu.GetValue(value) })
                .Where(uu => uu.Name != nameof(Exception.TargetSite));

            if (options?.DefaultIgnoreCondition == JsonIgnoreCondition.WhenWritingNull)
            {
                serializableProperties = serializableProperties.Where(uu => uu.Value != null);
            }

            var propList = serializableProperties.ToList();

            if (propList.Count == 0)
            {
                // Nothing to write
                return;
            }

            writer.WriteStartObject();

            foreach (var prop in propList)
            {
                writer.WritePropertyName(prop.Name);
                JsonSerializer.Serialize(writer, prop.Value, options);
            }

            writer.WriteEndObject();
        }
    }
}
