using System.Diagnostics;
using BallchasingWrapper.Interfaces;
using BallchasingWrapper.Models;

namespace BallchasingWrapper.BusinessLogic
{
    public class BallchasingApi
    {
        public CancellationToken StoppingToken { get; set; } = CancellationToken.None;
        public double MaximumCallsPerSecond { get; set; } = 1000;
        public IAuthTokenInfo TokenInfo { get; }
        private readonly Stopwatch _stopWatch = new();
        private readonly HttpClient _client;
        private List<string> _calls = new();
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
            return await Task.Run(async () =>
            {
                WaitForYourTurn();
                if (StoppingToken.IsCancellationRequested)
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
                    var timeToWait = 1000d / speed * 1.1; // I use 1.1. That 0.1 extra is a safety buffer.
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

        private static ApiDataPack GetApiDataFromString(string dataString)
        {
            try
            {
                var datapack = JsonConvert.DeserializeObject<ApiDataPack>(dataString);
                FillInitialTeamSizes(datapack);
                datapack.Success = true;
                return datapack;
            }
            catch (Exception ex)
            {
                return new ApiDataPack
                {
                    ReceivedString = ex.Message,
                    Success = false
                };
            }
        }

        private static void FillInitialTeamSizes(ApiDataPack datapack)
        {
            foreach (var replay in datapack.Replays)
            {
                var teamSize = replay.TeamBlue.Players.Count < replay.TeamOrange.Players.Count ?
                    replay.TeamBlue.Players.Count : replay.TeamOrange.Players.Count;
                replay.TeamBlue.InitialTeamSize = teamSize;
                replay.TeamOrange.InitialTeamSize = teamSize;
            }
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
