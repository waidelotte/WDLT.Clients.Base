using System;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;
using WDLT.Clients.Base.Exceptions;

namespace WDLT.Clients.Base
{
    public abstract class BaseClient
    {
        public string DefaultUserAgent { get; protected set; }

        protected readonly RestClient _client;

        private readonly string _baseHost;

        protected BaseClient(string userAgent)
        {
            DefaultUserAgent = userAgent;
            _client = CreateClient(null, userAgent);
        }

        protected BaseClient(string baseHost, string userAgent)
        {
            _baseHost = baseHost;
            DefaultUserAgent = userAgent;
            _client = CreateClient(baseHost, userAgent);
        }

        protected static RestClient CreateClient(string baseHost, string userAgent)
        {
            var client = string.IsNullOrWhiteSpace(baseHost) ? new RestClient() : new RestClient(baseHost);

            client.UserAgent = userAgent;
            client.AllowMultipleDefaultParametersWithSameName = false;
            client.Timeout = 10000;

            client.AddDefaultHeader("Accept", "application/json, text/plain, */*");
            client.AddDefaultHeader("Accept-Language", "en-GB,en-US;q=0.9,en,ru;q=0.8,uk;q=0.7");
            client.AddDefaultHeader("Cache-Control", "no-cache");

            client.ConfigureWebRequest(r =>
            {
                r.KeepAlive = false;
                r.ServicePoint.Expect100Continue = false;
                r.ServicePoint.ConnectionLimit = int.MaxValue;
                r.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                r.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
            });

            return client;
        }

        public void SetUserAgent(string value)
        {
            _client.UserAgent = value;
        }

        protected async Task<T> RequestAsync<T>(IRestRequest request, Proxy proxy = null)
        {
            var response = await RequestRawAsync(request, proxy);
            return JsonConvert.DeserializeObject<T>(response.Content);
        }

        protected virtual void OnBeforeRequest(RestClient client, IRestRequest request, Proxy proxy = null)
        {

        }

        protected virtual void OnAfterRequest(RestClient client, IRestResponse response, Proxy proxy = null)
        {

        }

        public Task<IRestResponse> RequestRawAsync(IRestRequest request, Proxy proxy = null)
        {
            return RequestRawAsync(_client, request, proxy);
        }

        public async Task<IRestResponse> RequestRawAsync(RestClient client, IRestRequest request, Proxy proxy = null)
        {
            OnBeforeRequest(client, request, proxy);

            SetProxy(client, proxy);

            var response = await client.ExecuteAsync(request);
            proxy?.Request();

            HandleResponse(response, proxy);
            OnAfterRequest(client, response, proxy);

            return response;
        }

        public static async Task<string> GetStringAsync(IRestRequest request, Proxy proxy = null)
        {
            var client = CreateClient(null, null);

            SetProxy(client, proxy);

            var response = await client.ExecuteAsync(request);
            proxy?.Request();

            HandleResponse(response, proxy);

            return response.Content;
        }

        protected Task DownloadAsync(Uri uri, string saveTo)
        {
            var bytes = _client.DownloadData(new RestRequest(uri));
            return File.WriteAllBytesAsync(saveTo, bytes);
        }

        protected static void SetProxy(IRestClient client, Proxy proxy)
        {
            if (proxy != null)
            {
                if (!string.IsNullOrWhiteSpace(proxy.Login) && !string.IsNullOrWhiteSpace(proxy.Password))
                {
                    client.Proxy = new WebProxy(proxy.Address, proxy.Port)
                    {
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential(proxy.Login, proxy.Password)
                    };
                }
                else
                {
                    client.Proxy = new WebProxy(proxy.Address, proxy.Port);
                }
            }
        }

        protected static void HandleResponse(IRestResponse response, Proxy proxy)
        {
            if (response.StatusCode != HttpStatusCode.OK)
            {
                proxy?.Error();
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    proxy?.Ban();
                    throw new HttpTimeoutException(response);
                }

                throw new ClientRequestException(response);
            }

            proxy?.SuccessRequest();
        }
    }
}
