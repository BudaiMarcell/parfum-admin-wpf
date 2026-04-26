using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ParfumAdmin_WPF.Services
{
    /// <summary>
    /// Persists the bearer token between app launches using Windows DPAPI
    /// (CurrentUser scope). The encrypted blob lives at
    /// <c>%LOCALAPPDATA%\ParfumAdmin\token.bin</c>; only the same Windows
    /// account that wrote it can decrypt it. Loss case (different user, OS
    /// reinstall, profile reset) is acceptable — the user just signs in again.
    ///
    /// We hold an in-memory copy after <see cref="Load"/> so that the
    /// <see cref="AuthDelegatingHandler"/> can attach the token on every
    /// outgoing request without hitting disk + DPAPI in the hot path.
    /// </summary>
    public class TokenStore
    {
        private static readonly string AppFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ParfumAdmin");

        private static readonly string FilePath = Path.Combine(AppFolder, "token.bin");

        // Optional entropy: tiny extra mix to limit cross-app DPAPI sharing.
        // It's NOT a secret — the Windows user account is the only real boundary.
        private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("ParfumAdmin/token");

        private readonly object _gate = new();
        private string? _cachedToken;

        /// <summary>
        /// Returns the decrypted token from memory if present, otherwise
        /// from disk. Returns null when no token has been saved yet, or when
        /// the on-disk blob has been tampered with / encrypted under a
        /// different user profile.
        /// </summary>
        public string? Load()
        {
            lock (_gate)
            {
                if (_cachedToken != null) return _cachedToken;

                if (!File.Exists(FilePath)) return null;

                try
                {
                    var encrypted = File.ReadAllBytes(FilePath);
                    var plain = ProtectedData.Unprotect(encrypted, Entropy, DataProtectionScope.CurrentUser);
                    _cachedToken = Encoding.UTF8.GetString(plain);
                    return _cachedToken;
                }
                catch (Exception)
                {
                    // Corrupt blob, wrong user profile, or DPAPI mishap.
                    // Treat as "no token". Do NOT log the exception — Serilog
                    // could surface it in a file we want to keep clean of
                    // anything that hints at credential material.
                    return null;
                }
            }
        }

        /// <summary>
        /// Encrypts and writes the token to disk, and updates the in-memory
        /// cache so subsequent <see cref="Load"/> calls return immediately.
        /// </summary>
        public void Save(string token)
        {
            if (string.IsNullOrEmpty(token)) return;

            lock (_gate)
            {
                Directory.CreateDirectory(AppFolder);
                var plain = Encoding.UTF8.GetBytes(token);
                var encrypted = ProtectedData.Protect(plain, Entropy, DataProtectionScope.CurrentUser);
                File.WriteAllBytes(FilePath, encrypted);
                _cachedToken = token;
            }
        }

        /// <summary>
        /// Removes the token from memory and from disk. Safe to call when
        /// nothing is stored.
        /// </summary>
        public void Clear()
        {
            lock (_gate)
            {
                _cachedToken = null;
                try
                {
                    if (File.Exists(FilePath)) File.Delete(FilePath);
                }
                catch (Exception)
                {
                    // Best effort — if the file is locked we'll overwrite on
                    // next Save anyway.
                }
            }
        }
    }
}
