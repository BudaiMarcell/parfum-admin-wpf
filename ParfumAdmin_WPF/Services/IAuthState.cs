using System;

namespace ParfumAdmin_WPF.Services
{
    /// <summary>
    /// Singleton bus used to broadcast authentication state changes from the
    /// HTTP layer to the UI. The <see cref="AuthDelegatingHandler"/> raises
    /// <see cref="OnSessionExpired"/> when an admin endpoint returns 401 (or
    /// when the user logs out). MainWindow subscribes and navigates back to
    /// the login view.
    /// </summary>
    public interface IAuthState
    {
        /// <summary>
        /// Fired when the current session is no longer valid. Subscribers
        /// MUST marshal back to the UI thread before touching any visual
        /// state — the event may be raised from a background HTTP thread.
        /// </summary>
        event EventHandler? SessionExpired;

        /// <summary>
        /// Raise the <see cref="SessionExpired"/> event. Called by the HTTP
        /// handler on 401 and by the auth service on explicit logout.
        /// </summary>
        void NotifySessionExpired();
    }

    public class AuthState : IAuthState
    {
        public event EventHandler? SessionExpired;

        public void NotifySessionExpired()
        {
            SessionExpired?.Invoke(this, EventArgs.Empty);
        }
    }
}
