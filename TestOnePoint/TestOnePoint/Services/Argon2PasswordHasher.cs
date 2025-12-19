using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;
using TestOnePoint.Services;


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

                // Split and ignore empty entries created by leading '$'
                var parts = encodedHash.Split('$', StringSplitOptions.RemoveEmptyEntries);

                // Minimal expected PHC format: ["argon2id", "v=19", "m=...,t=...,p=...", "<saltB64>", "<hashB64>"]
                if (parts.Length < 4)
                    return false;

                if (!parts[0].Equals("argon2id", StringComparison.OrdinalIgnoreCase))
                    return false;

                // Locate params part (the token that contains 'm=')
                int paramsIndex = -1;
                for (int i = 1; i < parts.Length - 2; i++)
                {
                    if (parts[i].Contains("m=", StringComparison.OrdinalIgnoreCase))
                    {
                        paramsIndex = i;
                        break;
                    }
                }

                if (paramsIndex == -1)
                    return false;

                var paramsPart = parts[paramsIndex];

                // Parse params: expect tokens like m=...,t=...,p=...
                int memory = 0, iterations = 0, parallel = 0;
                var paramTokens = paramsPart.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var token in paramTokens)
                {
                    var t = token.Trim();
                    if (t.StartsWith("m=", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!int.TryParse(t.Substring(2), out memory))
                            return false;
                    }
                    else if (t.StartsWith("t=", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!int.TryParse(t.Substring(2), out iterations))
                            return false;
                    }
                    else if (t.StartsWith("p=", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!int.TryParse(t.Substring(2), out parallel))
                            return false;
                    }
                }

                // Salt and hash are the last two parts
                var saltB64 = parts[^2];
                var hashB64 = parts[^1];

                var salt = Convert.FromBase64String(saltB64);
                var expectedHash = Convert.FromBase64String(hashB64);

                var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
                {
                    Salt = salt,
                    Iterations = iterations == 0 ? _iterations : iterations,
                    MemorySize = memory == 0 ? _memorySize : memory,
                    DegreeOfParallelism = parallel == 0 ? _degreeOfParallelism : parallel
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