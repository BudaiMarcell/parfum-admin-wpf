using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace ParfumAdmin_WPF.Services
{
    /// <summary>
    /// HttpMessageHandler installed in front of the admin <see cref="ApiService"/>'s
    /// HttpClient. Two responsibilities:
    ///
    /// 1. <b>Inject</b> the bearer token from <see cref="TokenStore"/> on every
    ///    outgoing request, so individual call sites don't need to manage the
    ///    Authorization header.
    /// 2. <b>Intercept 401</b> responses, clear the token store, and notify
    ///    <see cref="IAuthState"/> so the UI can return to the login view.
    ///
    /// The handler is intentionally NOT installed on AuthService's HttpClient —
    /// the login endpoint is the only way to mint a new token, and we don't
    /// want the SessionExpired event firing during a normal failed-password
    /// attempt.
    /// </summary>
    public class AuthDelegatingHandler : DelegatingHandler
    {
        private readonly TokenStore _tokenStore;
        private readonly IAuthState _authState;

        public AuthDelegatingHandler(TokenStore tokenStore, IAuthState authState)
        {
            _tokenStore = tokenStore;
            _authState = authState;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Attach Authorization header from the cached token. We don't
            // overwrite an explicitly set header — that would be unusual but
            // could come from a future caller wanting to use a different token.
            if (request.Headers.Authorization == null)
            {
                var token = _tokenStore.Load();
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
            }

            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                // Token has expired or been revoked server-side. Wipe it
                // locally so subsequent requests don't keep sending a stale
                // header, and let the UI know to bounce the user back to
                // the login screen.
                _tokenStore.Clear();
                _authState.NotifySessionExpired();
            }

            return response;
        }
    }
}
