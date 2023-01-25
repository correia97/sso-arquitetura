using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Cadastro.Domain.Entities
{
    public class TokenPayload
    {
        public int Exp { get; set; }
        public int Iat { get; set; }

        [JsonPropertyName("auth_time")]
        public int AuthTime { get; set; }
        public string Jti { get; set; }
        public string Iss { get; set; }
        public object Aud { get; set; }
        public string Sub { get; set; }
        public string Typ { get; set; }
        public string Azp { get; set; }
        public string Nonce { get; set; }

        [JsonPropertyName("session_state")]
        public string SessionState { get; set; }
        public List<string> Allowedorigins { get; set; }

        [JsonPropertyName("realm_access")]
        public RealmAccess RealmAccess { get; set; }

        [JsonPropertyName("Resource_Access")]
        public ResourceAccess ResourceAccess { get; set; }
        public string Scope { get; set; }
        public string Sid { get; set; }

        [JsonPropertyName("email_verified")]
        public bool EmailVerified { get; set; }
        public string Name { get; set; }

        [JsonPropertyName("preferred_username")]
        public string PreferredUsername { get; set; }

        [JsonPropertyName("given_name")]
        public string GivenName { get; set; }

        [JsonPropertyName("family_name")]
        public string FamilyName { get; set; }
        public string Email { get; set; }
        public List<string> Group { get; set; }
        public string ClientHost { get; set; }
        public string ClientId { get; set; }
        public string ClientAddress { get; set; }
    }

    public class RealmAccess
    {
        public List<string> Roles { get; set; }
    }

    public class Account
    {
        public List<string> Roles { get; set; }
    }


    public class ResourceAccess
    {
        public Broker Broker { get; set; }
        public Account Account { get; set; }
    }

    public class Broker
    {
        public List<string> Roles { get; set; }
    }
}
