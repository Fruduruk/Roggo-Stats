using RLStats_Classes.MainClasses.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RLStats_Classes.Models;

namespace RLStats_Classes.MainClasses
{
    public class BallchasingApi
    {
        public static BallchasingApi Instance { get; private set; }
        public CancellationToken StoppingToken { get; set; } = CancellationToken.None;
        public double MaximumCallsPerSecond { get; set; } = 1000;
        public IAuthTokenInfo TokenInfo { get; }
        private readonly Stopwatch _stopWatch = new();
        private readonly HttpClient _client;
        private List<string> _calls = new();
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
                if(StoppingToken.IsCancellationRequested)
                    return new HttpResponseMessage(System.Net.HttpStatusCode.Locked);
                lock (_calls)
                    _calls.Add(url);
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
                    if (speed > MaximumCallsPerSecond)
                        speed = MaximumCallsPerSecond;
                    var timeToWait = (1000d / speed) * 1.1; // I use 1.1. That 0.1 extra is a safety buffer.
                    var hasToWait = _stopWatch.ElapsedMilliseconds < timeToWait;
                    if (hasToWait)
                    {
                        var actualTimeToWait = Math.Round(timeToWait, MidpointRounding.ToPositiveInfinity) -
                                               _stopWatch.ElapsedMilliseconds;
                        var task = Task.Run(() => Task.Delay((int)actualTimeToWait, StoppingToken));
                        task.Wait();
                    }

                    _stopWatch.Stop();
                }

                _stopWatch.Restart();
            }
        }

        public async Task<ApiDataPack> GetApiDataPack(string url)
        {
            var response = await GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
                var dataString = await reader.ReadToEndAsync();
                var pack = GetApiDataFromString(dataString);
                return pack;
            }
            return new ApiDataPack
            {
                Success = false
            };
        }

        public async Task<int> GetTotalReplayCountOfUrlAsync(string url)
        {
            var pack = await GetApiDataPack(url);
            if (pack.Success)
                return pack.ReplayCount;
            return 0;
        }

        private static ApiDataPack GetApiDataFromString(string dataString)
        {
            dynamic jData = JsonConvert.DeserializeObject(dataString);
            var replays = new List<Replay>();
            try
            {
                if (jData != null)
                    foreach (var r in jData.list)
                    {
                        replays.Add(new ReplayAssembler(r).Assemble());
                    }

                if (replays.Count == 0)
                    throw new Exception("No replay found");
            }
            catch
            {
                return new ApiDataPack()
                {
                    Success = false,
                    ReceivedString = dataString
                };
            }
            return new ApiDataPack()
            {
                Replays = replays,
                ReplayCount = jData.count,
                Next = jData.next,
                Success = true,
            };
        }

        public string[] GetCalls()
        {
            lock (_calls)
            {
                return _calls.ToArray();
            }
        }

        public string[] GetAndDeleteCalls()
        {
            lock (_calls)
            {
                var callArray = _calls.ToArray();
                _calls.Clear();
                return callArray;
            }
        }

        public void DeleteCalls()
        {
            lock (_calls)
            {
                _calls.Clear();
            }
        }
    }
}
