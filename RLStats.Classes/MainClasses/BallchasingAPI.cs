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
        public static BallchasingApi Instance { get; private set; }
        public IAuthTokenInfo TokenInfo { get; }
        private readonly Stopwatch _stopWatch = new();
        private readonly HttpClient _client;
        private BallchasingApi(IAuthTokenInfo tokenInfo)
        {
            TokenInfo = tokenInfo ?? throw new ArgumentNullException(nameof(tokenInfo));
            _client = GetClientWithToken();
        }

        public static void CreateInstance(IAuthTokenInfo tokenInfo)
        {
            Instance = new BallchasingApi(tokenInfo);
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
            return await Task.Run(async () =>
            {
                WaitForYourTurn();
                return await _client.GetAsync(url);
            });
        }

        private void WaitForYourTurn()
        {
            lock (_stopWatch)
            {
                if (_stopWatch.IsRunning)
                {
                    double speed = TokenInfo.GetSpeed();
                    var timeToWait = (1000 / speed) * 1.1;
                    var hasToWait = _stopWatch.ElapsedMilliseconds < timeToWait;
                    if (hasToWait)
                    {
                        var actualTimeToWait = Math.Round(timeToWait, MidpointRounding.ToPositiveInfinity) -
                                               _stopWatch.ElapsedMilliseconds;
                        Thread.Sleep((int)actualTimeToWait);
                    }

                    _stopWatch.Stop();
                }

                _stopWatch.Restart();
            }
        }
    }
}
