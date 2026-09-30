using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GPC.Converter.CivilNx
{
    public sealed class CivilNxResponse
    {
        public string Endpoint { get; }
        public string Json { get; }
        public string Sha256 { get; }
        public CivilNxResponse(string endpoint, string json)
        {
            Endpoint = endpoint; Json = json ?? throw new ArgumentNullException(nameof(json));
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json))) Sha256 = SourceEvidence.Sha256(stream);
        }
    }
    public sealed class CivilNxSnapshot
    {
        public IReadOnlyDictionary<string, CivilNxResponse> Responses { get; }
        public CivilNxSnapshot(IDictionary<string, CivilNxResponse> responses)
        { Responses = new ReadOnlyDictionary<string, CivilNxResponse>(new Dictionary<string, CivilNxResponse>(responses, StringComparer.Ordinal)); }
    }

    /// <summary>Read-only MIDAS transport. No model-editing or analysis commands. Keys are supplied at runtime and never saved in responses.</summary>
    public sealed class CivilNxApiClient : IDisposable
    {
        private readonly HttpClient _http;
        private readonly Uri _baseUri;
        private readonly Func<string> _key;
        private const int MaxResponseBytes = 32 * 1024 * 1024;

        public CivilNxApiClient(Uri baseUri, Func<string> apiKey, HttpMessageHandler handler = null)
        {
            if (baseUri == null || !baseUri.IsAbsoluteUri || (baseUri.Scheme != Uri.UriSchemeHttps && !(baseUri.Scheme == Uri.UriSchemeHttp && baseUri.IsLoopback))
                || baseUri.UserInfo.Length != 0 || baseUri.Query.Length != 0 || baseUri.Fragment.Length != 0) throw new ArgumentException("An HTTPS base URL (or local HTTP endpoint) without embedded credentials is required.");
            _baseUri = new Uri(baseUri.AbsoluteUri.TrimEnd('/') + "/"); _key = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            // Redirects must not forward MAPI-Key to another host. Custom handlers are for caller-controlled transport/testing.
            _http = new HttpClient(handler ?? new HttpClientHandler { AllowAutoRedirect = false }) { Timeout = TimeSpan.FromSeconds(60) };
        }

        public Task<CivilNxResponse> ReadDatabaseAsync(string table, CancellationToken cancellationToken = default)
        {
            // Database names are identifiers, never URLs or executable operations. GET cannot alter the model.
            if (string.IsNullOrWhiteSpace(table) || table.Length > 64) throw new ArgumentException("Invalid database identifier.");
            foreach (char c in table) if (!((c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '_' || c == '-')) throw new ArgumentException("Invalid database identifier.");
            return ReadAsync("db/" + table, null, cancellationToken);
        }

        public async Task<CivilNxSnapshot> ReadGeometrySnapshotAsync(CancellationToken cancellationToken = default)
        {
            var values = new Dictionary<string, CivilNxResponse>();
            foreach (var table in new[] { "UNIT", "NODE", "ELEM" }) values.Add(table, await ReadDatabaseAsync(table, cancellationToken).ConfigureAwait(false));
            // The API offers no transaction here. Detect changed records over two passes, never claim a solver transaction.
            foreach (var table in new[] { "UNIT", "NODE", "ELEM" })
                if ((await ReadDatabaseAsync(table, cancellationToken).ConfigureAwait(false)).Sha256 != values[table].Sha256)
                    throw new InvalidOperationException("CivilNxModelChangedDuringRead");
            return new CivilNxSnapshot(values);
        }

        public Task<CivilNxResponse> ReadResultTableAsync(CivilNxTableRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return ReadAsync("post/table", request.ToJson(), cancellationToken);
        }

        private async Task<CivilNxResponse> ReadAsync(string endpoint, string body, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using (var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                deadline.CancelAfter(TimeSpan.FromSeconds(60));
                cancellationToken = deadline.Token;
                using (var request = new HttpRequestMessage(body == null ? HttpMethod.Get : HttpMethod.Post, new Uri(_baseUri, endpoint)))
                {
                    var key = _key();
                    if (string.IsNullOrWhiteSpace(key) || key.Contains("\r") || key.Contains("\n")) throw new ArgumentException("A valid runtime MAPI-Key is required.");
                    request.Headers.Add("MAPI-Key", key);
                    if (body != null) request.Content = new StringContent(body, Encoding.UTF8, "application/json");
                    using (var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false))
                    {
                        if (!response.IsSuccessStatusCode) throw new HttpRequestException("CivilNxHttpStatus:" + (int)response.StatusCode);
                        if (response.Content.Headers.ContentLength > MaxResponseBytes) throw new InvalidDataException("CivilNxResponseTooLarge");
                        using (var input = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                        using (var memory = new MemoryStream())
                        {
                            var buffer = new byte[8192]; int read;
                            while ((read = await input.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) > 0)
                            {
                                if (memory.Length + read > MaxResponseBytes) throw new InvalidDataException("CivilNxResponseTooLarge");
                                memory.Write(buffer, 0, read);
                            }
                            return new CivilNxResponse(endpoint, new UTF8Encoding(false, true).GetString(memory.ToArray()));
                        }
                    }
                }
            }
        }
        public void Dispose() => _http.Dispose();
    }
}
