using System;
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
        protected RestClient _client;

        protected BaseClient(string userAgent)
        {
            CreateClient(null, userAgent);
        }

        protected BaseClient(string baseHost, string userAgent)
        {
            CreateClient(baseHost, userAgent);
        }

        private void CreateClient(string baseHost, string userAgent)
        {
            if (string.IsNullOrWhiteSpace(baseHost))
            {
                _client = new RestClient();
            }
            else
            {
                _client = new RestClient(baseHost);
                _client.AddDefaultHeader("referer", baseHost);
            }

            _client.UserAgent = userAgent;
            _client.AllowMultipleDefaultParametersWithSameName = false;
            _client.Timeout = 10000;
            
            _client.AddDefaultHeader("Accept", "application/json, text/plain, */*");
            _client.AddDefaultHeader("Accept-Language", "en-GB,en-US;q=0.9,en,ru;q=0.8,uk;q=0.7");
            _client.AddDefaultHeader("Cache-Control", "no-cache");

            _client.ConfigureWebRequest(r =>
            {
                r.ServicePoint.Expect100Continue = false;
                r.ServicePoint.ConnectionLimit = int.MaxValue;
                r.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                r.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
            });
        }

        public void SetUserAgent(string value)
        {
            _client.UserAgent = value;
        }

        public async Task<T> RequestAsync<T>(IRestRequest request, Proxy proxy = null)
        {
            var response = await RequestRawAsync(request, proxy).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<T>(response.Content);
        }

        public async Task<IRestResponse> RequestRawAsync(IRestRequest request, Proxy proxy = null)
        {
            if (proxy != null)
            {
                if (!string.IsNullOrWhiteSpace(proxy.Login) && !string.IsNullOrWhiteSpace(proxy.Password))
                {
                    _client.Proxy = new WebProxy(proxy.Address, proxy.Port)
                    {
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential(proxy.Login, proxy.Password)
                    };
                }
                else
                {
                    _client.Proxy = new WebProxy(proxy.Address, proxy.Port);
                }
            }

            var response = await _client.ExecuteAsync(request).ConfigureAwait(false);
            proxy?.Request();

            if (response.StatusCode != HttpStatusCode.OK)
            {
                proxy?.Error();
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    proxy?.Ban();
                    throw new HttpTimeoutException("Too Many Requests");
                }

                throw new HttpRequestException($"Status code not OK. {response.ErrorMessage} {response.Content}");
            }

            proxy?.SuccessRequest();
            return response;
        }

    }
}
