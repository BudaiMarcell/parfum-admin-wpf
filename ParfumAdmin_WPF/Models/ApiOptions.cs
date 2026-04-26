namespace ParfumAdmin_WPF.Models
{
    /// <summary>
    /// Strongly-typed binding for the "Api" section of appsettings.json.
    /// Read via <c>IOptions&lt;ApiOptions&gt;</c> in the DI container.
    /// </summary>
    public class ApiOptions
    {
        /// <summary>
        /// Absolute URL to the Laravel API root, e.g.
        /// <c>http://api.buttercupperfumery.local/api/</c>. Trailing slash is
        /// required so that relative paths in the typed HttpClients resolve
        /// against this prefix instead of replacing it.
        /// </summary>
        public string BaseUrl { get; set; } = string.Empty;
    }
}
