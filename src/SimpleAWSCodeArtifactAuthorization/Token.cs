using System;
using System.Text.Json.Serialization;

namespace SimpleAWSCodeArtifactAuthorization
{
    public class Token
    {
        public string AuthorizationToken { get; set; }
        public DateTimeOffset Expiration { get; set; }
    }

    // NOTE: This will enable source generators for System.Json.Text ... which (by default) will use
    //       reflection ... but that hurts self-contained 'trimming'.
    [JsonSerializable(typeof(Token))]
    internal partial class SourceGenerationContext : JsonSerializerContext
    {
    }
}
