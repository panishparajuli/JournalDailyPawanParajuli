using System.Security.Cryptography;
using System.Text;

namespace JournalDaily.Services
{
    /// <summary>
    /// Service for managing PIN-based authentication with secure storage.
    /// Handles PIN setup, verification, and storage using SecureStorage and Preferences.
    /// </summary>
    public class PinAuthService
    {
        private const string PIN_HASH_KEY = "pin_hash";
        private const string PIN_SETUP_KEY = "pin_setup_complete";
        private const string PIN_ATTEMPTS_KEY = "pin_attempts";
        private const string PIN_LOCKOUT_TIME_KEY = "pin_lockout_time";
        private const int MAX_ATTEMPTS = 5;
        private const int LOCKOUT_DURATION_MINUTES = 15;

        /// <summary>
        /// Checks if PIN setup has been completed.
        /// </summary>
        public bool IsPinSetupComplete()
        {
            return Preferences.Get(PIN_SETUP_KEY, false);
        }

        /// <summary>
        /// Sets up a new PIN (first-time setup only).
        /// </summary>
        public async Task<bool> SetupPinAsync(string pin)
        {
            // Validate PIN format
            if (string.IsNullOrEmpty(pin) || pin.Length != 4 || !pin.All(char.IsDigit))
            {
                return false;
            }

            try
            {
                // Hash the PIN
                string hashedPin = HashPin(pin);

                // Store hashed PIN in SecureStorage
                await SecureStorage.SetAsync(PIN_HASH_KEY, hashedPin);

                // Mark setup as complete
                Preferences.Set(PIN_SETUP_KEY, true);

                // Reset attempts on successful setup
                Preferences.Set(PIN_ATTEMPTS_KEY, 0);

                System.Diagnostics.Debug.WriteLine("[PinAuthService] PIN setup completed successfully.");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PinAuthService] Error setting up PIN: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Verifies a PIN entry against the stored hash.
        /// Returns true if correct, false if incorrect or locked out.
        /// </summary>
        public async Task<bool> VerifyPinAsync(string pin)
        {
            // Check if locked out
            if (IsLockedOut())
            {
                System.Diagnostics.Debug.WriteLine("[PinAuthService] PIN entry locked out due to max attempts.");
                return false;
            }

            if (string.IsNullOrEmpty(pin) || pin.Length != 4 || !pin.All(char.IsDigit))
            {
                IncrementFailedAttempts();
                return false;
            }

            try
            {
                // Retrieve hashed PIN from SecureStorage
                string? storedHash = await SecureStorage.GetAsync(PIN_HASH_KEY);

                if (string.IsNullOrEmpty(storedHash))
                {
                    System.Diagnostics.Debug.WriteLine("[PinAuthService] No PIN hash found in storage.");
                    return false;
                }

                // Hash the entered PIN
                string enteredHash = HashPin(pin);

                // Compare hashes using constant-time comparison to prevent timing attacks
                bool isMatch = ConstantTimeCompare(storedHash, enteredHash);

                if (isMatch)
                {
                    // Reset attempts on successful verification
                    Preferences.Set(PIN_ATTEMPTS_KEY, 0);
                    System.Diagnostics.Debug.WriteLine("[PinAuthService] PIN verified successfully.");
                    return true;
                }
                else
                {
                    IncrementFailedAttempts();
                    System.Diagnostics.Debug.WriteLine("[PinAuthService] PIN verification failed.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PinAuthService] Error verifying PIN: {ex.Message}");
                IncrementFailedAttempts();
                return false;
            }
        }

        /// <summary>
        /// Gets the number of remaining PIN attempts before lockout.
        /// </summary>
        public int GetRemainingAttempts()
        {
            if (IsLockedOut())
            {
                return 0;
            }

            int attempts = Preferences.Get(PIN_ATTEMPTS_KEY, 0);
            return Math.Max(0, MAX_ATTEMPTS - attempts);
        }

        /// <summary>
        /// Checks if PIN entry is currently locked out.
        /// </summary>
        public bool IsLockedOut()
        {
            int attempts = Preferences.Get(PIN_ATTEMPTS_KEY, 0);
            if (attempts < MAX_ATTEMPTS)
            {
                return false;
            }

            // Check if lockout period has expired
            long lockoutTimeTicks = Preferences.Get(PIN_LOCKOUT_TIME_KEY, 0L);
            if (lockoutTimeTicks == 0)
            {
                return true;
            }

            DateTime lockoutTime = new DateTime(lockoutTimeTicks);
            DateTime now = DateTime.UtcNow;

            if (now >= lockoutTime.AddMinutes(LOCKOUT_DURATION_MINUTES))
            {
                // Lockout period has expired, reset attempts
                Preferences.Set(PIN_ATTEMPTS_KEY, 0);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets remaining lockout time in seconds.
        /// </summary>
        public int GetLockoutRemainingSeconds()
        {
            long lockoutTimeTicks = Preferences.Get(PIN_LOCKOUT_TIME_KEY, 0L);
            if (lockoutTimeTicks == 0)
            {
                return 0;
            }

            DateTime lockoutTime = new DateTime(lockoutTimeTicks);
            DateTime now = DateTime.UtcNow;
            TimeSpan remaining = lockoutTime.AddMinutes(LOCKOUT_DURATION_MINUTES) - now;

            return Math.Max(0, (int)remaining.TotalSeconds);
        }

        /// <summary>
        /// Resets PIN setup (for testing or user reset functionality).
        /// </summary>
        public async Task ResetPinAsync()
        {
            try
            {
                // Clear the stored PIN hash
                SecureStorage.Remove(PIN_HASH_KEY);
                Preferences.Remove(PIN_SETUP_KEY);
                Preferences.Remove(PIN_ATTEMPTS_KEY);
                Preferences.Remove(PIN_LOCKOUT_TIME_KEY);
                System.Diagnostics.Debug.WriteLine("[PinAuthService] PIN reset successfully.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PinAuthService] Error resetting PIN: {ex.Message}");
            }
        }

        /// <summary>
        /// Hashes a PIN using SHA256.
        /// </summary>
        private string HashPin(string pin)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(pin));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        /// <summary>
        /// Performs constant-time string comparison to prevent timing attacks.
        /// </summary>
        private bool ConstantTimeCompare(string a, string b)
        {
            if (a.Length != b.Length)
            {
                return false;
            }

            int result = 0;
            for (int i = 0; i < a.Length; i++)
            {
                result |= a[i] ^ b[i];
            }

            return result == 0;
        }

        /// <summary>
        /// Increments failed PIN attempt counter and sets lockout if threshold reached.
        /// </summary>
        private void IncrementFailedAttempts()
        {
            int attempts = Preferences.Get(PIN_ATTEMPTS_KEY, 0);
            attempts++;

            Preferences.Set(PIN_ATTEMPTS_KEY, attempts);

            if (attempts >= MAX_ATTEMPTS)
            {
                // Set lockout time
                Preferences.Set(PIN_LOCKOUT_TIME_KEY, DateTime.UtcNow.Ticks);
                System.Diagnostics.Debug.WriteLine($"[PinAuthService] PIN locked out after {MAX_ATTEMPTS} failed attempts.");
            }
        }
    }
}
