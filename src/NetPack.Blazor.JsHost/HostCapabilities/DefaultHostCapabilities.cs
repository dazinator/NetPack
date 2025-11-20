using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;

namespace NetPack.Blazor.JsHost.HostCapabilities
{
    /// <summary>
    /// Default implementation of IHostCapabilities.
    /// </summary>
    public class DefaultHostCapabilities : IHostCapabilities
    {
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly IConfiguration _configuration;
        private readonly IPermissionChecker _permissionChecker;

        public event EventHandler<HostEventArgs> HostEventRaised;

        public DefaultHostCapabilities(
            HttpClient httpClient,
            AuthenticationStateProvider authStateProvider = null,
            IConfiguration configuration = null,
            IPermissionChecker permissionChecker = null)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _authStateProvider = authStateProvider;
            _configuration = configuration;
            _permissionChecker = permissionChecker;
        }

        public async Task<bool> CheckPermissionAsync(string permission)
        {
            if (_permissionChecker == null)
            {
                return false;
            }

            return await _permissionChecker.CheckPermissionAsync(permission);
        }

        public async Task<UserInfo> GetUserInfoAsync()
        {
            var userInfo = new UserInfo
            {
                IsAuthenticated = false
            };

            if (_authStateProvider != null)
            {
                var authState = await _authStateProvider.GetAuthenticationStateAsync();
                var user = authState.User;

                if (user != null && user.Identity != null && user.Identity.IsAuthenticated)
                {
                    userInfo.IsAuthenticated = true;
                    userInfo.UserName = user.Identity.Name;

                    foreach (var claim in user.Claims)
                    {
                        userInfo.Claims[claim.Type] = claim.Value;
                    }

                    if (user.Claims.Any(c => c.Type == "email"))
                    {
                        userInfo.Email = user.Claims.First(c => c.Type == "email").Value;
                    }
                }
            }

            return userInfo;
        }

        public Task<string> GetConfigAsync(string key)
        {
            if (_configuration == null)
            {
                return Task.FromResult<string>(null);
            }

            var value = _configuration[key];
            return Task.FromResult(value);
        }

        public Task RaiseHostEventAsync(string eventName, object eventData)
        {
            HostEventRaised?.Invoke(this, new HostEventArgs
            {
                EventName = eventName,
                EventData = eventData
            });

            return Task.CompletedTask;
        }

        public async Task<HttpResponse> FetchAsync(HttpRequest request)
        {
            try
            {
                var httpRequestMessage = new HttpRequestMessage
                {
                    Method = new HttpMethod(request.Method),
                    RequestUri = new Uri(request.Url, UriKind.RelativeOrAbsolute)
                };

                foreach (var header in request.Headers)
                {
                    httpRequestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }

                if (!string.IsNullOrEmpty(request.Body))
                {
                    httpRequestMessage.Content = new StringContent(request.Body, Encoding.UTF8, "application/json");
                }

                var response = await _httpClient.SendAsync(httpRequestMessage);

                var httpResponse = new HttpResponse
                {
                    Status = (int)response.StatusCode,
                    StatusText = response.ReasonPhrase,
                    Ok = response.IsSuccessStatusCode,
                    Body = await response.Content.ReadAsStringAsync()
                };

                foreach (var header in response.Headers)
                {
                    httpResponse.Headers[header.Key] = string.Join(", ", header.Value);
                }

                if (response.Content?.Headers != null)
                {
                    foreach (var header in response.Content.Headers)
                    {
                        httpResponse.Headers[header.Key] = string.Join(", ", header.Value);
                    }
                }

                return httpResponse;
            }
            catch (Exception ex)
            {
                return new HttpResponse
                {
                    Status = 500,
                    StatusText = "Internal Server Error",
                    Ok = false,
                    Body = ex.Message
                };
            }
        }
    }

    /// <summary>
    /// Event args for host events raised from JavaScript.
    /// </summary>
    public class HostEventArgs : EventArgs
    {
        public string EventName { get; set; }
        public object EventData { get; set; }
    }
}
