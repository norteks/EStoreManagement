using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace EStoreManagement.Services
{
    public class AuthService
    {
        private readonly string _tokenSecret;

        public AuthService(string tokenSecret)
        {
            _tokenSecret = tokenSecret;
        }

        // Generate JWT token
        public string GenerateToken(string username)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_tokenSecret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, username) }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        // Validate password (dummy implementation)
        public bool ValidatePassword(string inputPassword, string storedPasswordHash)
        {
            // Here you would normally hash the inputPassword and compare it with storedPasswordHash
            return inputPassword == storedPasswordHash; // For demonstration purposes
        }

        // Authenticate user
        public bool Authenticate(string username, string password)
        {
            // Implement user lookup and password validation
            string storedPasswordHash = "passwordHashHere"; // Replace with actual user password hash lookup
            if (username == "validUser" && ValidatePassword(password, storedPasswordHash))
            {
                return true;
            }
            return false;
        }
    }
}