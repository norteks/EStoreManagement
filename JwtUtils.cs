using System;
using System.Text;
using System.Security.Cryptography;
using System.Text.Json;

namespace EStoreManagementAPI;

public static class JwtUtils
{
    // Validates a compact JWT signed with HMACSHA256 and checks the exp claim (if present).
    public static bool ValidateJwt(string token, byte[] keyBytes)
    {
        try
        {
            var parts = token.Split('.');
            if (parts.Length != 3) return false;

            var signingInput = parts[0] + "." + parts[1];

            byte[] hash;
            using (var hmac = new HMACSHA256(keyBytes))
            {
                hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signingInput));
            }

            string computed = Convert.ToBase64String(hash).TrimEnd('=').Replace('+', '-').Replace('/', '_');
            if (!string.Equals(computed, parts[2], StringComparison.Ordinal)) return false;

            // check exp if present
            var payload = parts[1];
            var padded = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=').Replace('-', '+').Replace('_', '/');
            var payloadJson = Encoding.UTF8.GetString(Convert.FromBase64String(padded));
            using var doc = JsonDocument.Parse(payloadJson);
            if (doc.RootElement.TryGetProperty("exp", out var expEl) && expEl.ValueKind == JsonValueKind.Number)
            {
                var exp = expEl.GetInt64();
                var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                if (now >= exp) return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }
}
