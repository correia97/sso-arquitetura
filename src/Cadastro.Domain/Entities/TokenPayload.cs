using System.Collections.Generic;

namespace Cadastro.Domain.Entities
{
    public class TokenPayload
    {
        public int exp { get; set; }
        public int iat { get; set; }
        public int auth_time { get; set; }
        public string jti { get; set; }
        public string iss { get; set; }
        public string aud { get; set; }
        public string sub { get; set; }
        public string typ { get; set; }
        public string azp { get; set; }
        public string nonce { get; set; }
        public string session_state { get; set; }
        public List<string> allowedorigins { get; set; }
        public Realm_Access realm_access { get; set; }
        public Resource_Access resource_access { get; set; }
        public string scope { get; set; }
        public string sid { get; set; }
        public bool email_verified { get; set; }
        public string name { get; set; }
        public string preferred_username { get; set; }
        public string given_name { get; set; }
        public string family_name { get; set; }
        public string email { get; set; }
        public List<string> group { get; set; }
        public string clientHost { get; set; }
        public string clientId { get; set; }
        public string clientAddress { get; set; }
    }

    public class Realm_Access
    {
        public List<string> roles { get; set; }
    }

    public class Account
    {
        public List<string> roles { get; set; }
    }

    public class Resource_Access
    {
        public Broker broker { get; set; }
        public Account account { get; set; }
    }

    public class Broker
    {
        public List<string> roles { get; set; }
    }



}
