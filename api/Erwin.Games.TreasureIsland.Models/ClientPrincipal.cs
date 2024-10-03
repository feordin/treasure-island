using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Erwin.Games.TreasureIsland.Models
{
    public class ClientPrincipal
    {
        public string? IdentityProvider { get; set; }
        public string? UserId { get; set; }
        public string? UserDetails { get; set; }
        public IEnumerable<string>? UserRoles { get; set; }

        private static ClientPrincipal? _instance;

        public static ClientPrincipal? Instance
        {
            get => _instance;
            set => _instance = value;
        }

        public static ClientPrincipal? Parse(HttpRequest req)
        {
            var principal = new ClientPrincipal();

            if (req.Headers.TryGetValue("x-ms-client-principal", out var header))
            {
                var data = header[0];
                if (!string.IsNullOrEmpty(data))
                {
                    var decoded = Convert.FromBase64String(data);
                    var json = Encoding.UTF8.GetString(decoded);
                    principal = JsonSerializer.Deserialize<ClientPrincipal>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }

                return principal;
            }

            return new ClientPrincipal() { UserDetails = "TestPlayer", IdentityProvider = "TestProvider", UserId = "TestUser", UserRoles = new List<string> { "TestRole" } };
        }
    }
}