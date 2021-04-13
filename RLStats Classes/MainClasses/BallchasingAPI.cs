using RLStats_Classes.MainClasses.Interfaces;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RLStats_Classes.MainClasses
{
    public class BallchasingApi
    {
        public IAuthTokenInfo TokenInfo { get; }
        private readonly Stopwatch _stopWatch = new Stopwatch();
        private readonly HttpClient _client;
        public BallchasingApi(IAuthTokenInfo tokenInfo)
        {
            TokenInfo = tokenInfo ?? throw new ArgumentNullException(nameof(tokenInfo));
            _client = GetClientWithToken();
        }

        ~BallchasingApi()
        {
            _client.Dispose();
        }

        private HttpClient GetClientWithToken()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", TokenInfo.Token);
            return client;
        }

        public async Task<HttpResponseMessage> GetAsync(string url)
        {
            if (_stopWatch.IsRunning)
            {
                double speed = TokenInfo.GetSpeed();
                var timeToWait = (1000 / speed);
                var hasToWait = _stopWatch.ElapsedMilliseconds < timeToWait;
                if (hasToWait)
                {
                    var actualTimeToWait = Math.Round(timeToWait, MidpointRounding.ToPositiveInfinity) - _stopWatch.ElapsedMilliseconds;
                    Thread.Sleep((int)actualTimeToWait);
                }
                _stopWatch.Stop();
            }
            _stopWatch.Restart();
            var response = await _client.GetAsync(url);
            return response;
        }
    }
}
