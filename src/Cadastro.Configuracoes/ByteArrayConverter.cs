using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cadastro.Configuracoes
{
    //https://stackoverflow.com/questions/61565947/the-json-value-could-not-be-converted-to-system-byte
    [ExcludeFromCodeCoverage]
    public class ByteArrayConverter : JsonConverter<byte[]>
    {
        public override byte[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            short[] sByteArray = JsonSerializer.Deserialize<short[]>(ref reader);
            byte[] value = new byte[sByteArray.Length];
            for (int i = 0; i < sByteArray.Length; i++)
            {
                value[i] = (byte)sByteArray[i];
            }

            return value;
        }

        public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();

            foreach (var val in value)
            {
                writer.WriteNumberValue(val);
            }

            writer.WriteEndArray();
        }
    }
}
