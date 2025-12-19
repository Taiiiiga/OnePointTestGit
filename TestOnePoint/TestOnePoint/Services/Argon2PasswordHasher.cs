using System;
using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;

namespace TestOnePoint.Services
{
    public class Argon2PasswordHasher : IPasswordHasher
    {
        private readonly int _saltSize = 16; // bytes
        private readonly int _hashSize = 32; // bytes
        private readonly int _iterations = 3; // time cost
        private readonly int _memorySize = 1 << 16; // 65536 KB = 64 MB
        private readonly int _degreeOfParallelism = 4; // threads

        public string Hash(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password must not be empty.", nameof(password));

            var salt = RandomNumberGenerator.GetBytes(_saltSize);

            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                Iterations = _iterations,
                MemorySize = _memorySize,
                DegreeOfParallelism = _degreeOfParallelism
            };

            var hash = argon2.GetBytes(_hashSize);

            // PHC-like format: $argon2id$v=19$m=...,t=...,p=...$<saltB64>$<hashB64>
            var saltB64 = Convert.ToBase64String(salt);
            var hashB64 = Convert.ToBase64String(hash);
            var encoded = $"$argon2id$v=19$m={_memorySize},t={_iterations},p={_degreeOfParallelism}${saltB64}${hashB64}";
            return encoded;
        }

        public bool Verify(string password, string encodedHash)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(encodedHash))
                    return false;

                var parts = encodedHash.Split('$', StringSplitOptions.RemoveEmptyEntries);
                // Expect: ["argon2id", "v=19", "m=...,t=...,p=...", "<saltB64>", "<hashB64>"] or ["argon2id","v=19","m=...,t=...,p=...","<saltB64>","<hashB64>"]
                // Our format uses 4 meaningful parts after removing empty: ["argon2id","v=19$m=...,t=...,p=...","<saltB64>","<hashB64>"]
                if (parts.Length != 4 || parts[0] != "argon2id")
                    return false;

                var paramsPart = parts[1];
                var paramTokens = paramsPart.Split(',', StringSplitOptions.RemoveEmptyEntries);

                int memory = 0, iterations = 0, parallel = 0;
                foreach (var token in paramTokens)
                {
                    if (token.StartsWith("m=")) memory = int.Parse(token.Substring(2));
                    if (token.StartsWith("t=")) iterations = int.Parse(token.Substring(2));
                    if (token.StartsWith("p=")) parallel = int.Parse(token.Substring(2));
                }

                var salt = Convert.FromBase64String(parts[2]);
                var expectedHash = Convert.FromBase64String(parts[3]);

                var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
                {
                    Salt = salt,
                    Iterations = iterations,
                    MemorySize = memory,
                    DegreeOfParallelism = parallel
                };

                var computed = argon2.GetBytes(expectedHash.Length);

                return CryptographicOperations.FixedTimeEquals(computed, expectedHash);
            }
            catch
            {
                return false;
            }
        }
    }
}