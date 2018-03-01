using System;

namespace Lykke.Service.Decred.SignService.Models
{
    /// <summary>
    /// Returned as valid response for calls to
    /// GET api/isalive
    /// </summary>
    public class IsAliveResponse
    {
        public string Name { get; }
        public string Version { get; }
        public string Environment { get; }
        public bool IsDebug { get; }

        public IsAliveResponse(string name, string version, string env, bool isDebug)
        {
            IsDebug = isDebug;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Version = version ?? throw new ArgumentNullException(nameof(version));
            Environment = env ?? throw new ArgumentNullException(nameof(env));
        }
    }
}
