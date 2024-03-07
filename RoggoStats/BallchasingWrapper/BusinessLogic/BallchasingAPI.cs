using System.Diagnostics;
using BallchasingWrapper.Interfaces;
using BallchasingWrapper.Models;

namespace BallchasingWrapper.BusinessLogic
{
    public class BallchasingApi : IBallchasingApi
    {
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

        public async Task<HttpResponseMessage> GetAsync(string url, CancellationToken cancellationToken)
        {
            return await Task.Run(async () =>
            {
                WaitForYourTurn(cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                    return new HttpResponseMessage(System.Net.HttpStatusCode.Locked);
                lock (_calls)
                    _calls.Add(url);
                return await _client.GetAsync(url, cancellationToken);
            }, cancellationToken);
        }

        private void WaitForYourTurn(CancellationToken cancellationToken)
        {
            lock (_stopWatch)
            {
                if (_stopWatch.IsRunning)
                {
                    var speed = TokenInfo.GetSpeed();
                    if (speed > MaximumCallsPerSecond)
                        speed = MaximumCallsPerSecond;
                    var timeToWait = 1000d / speed * 1.1; // I use 1.1. That 0.1 extra is a safety buffer.
                    var hasToWait = _stopWatch.ElapsedMilliseconds < timeToWait;
                    if (hasToWait)
                    {
                        var actualTimeToWait = Math.Round(timeToWait, MidpointRounding.ToPositiveInfinity) -
                                               _stopWatch.ElapsedMilliseconds;
                        var task = Task.Run(() => Task.Delay((int)actualTimeToWait, cancellationToken), cancellationToken);
                        task.Wait(cancellationToken);
                    }

                    _stopWatch.Stop();
                }

                _stopWatch.Restart();
            }
        }

        public async Task<ApiDataPack> GetApiDataPackAsync(string url, CancellationToken cancellationToken)
        {
            var response = await GetAsync(url,cancellationToken);
            if (!response.IsSuccessStatusCode)
                return new ApiDataPack { Success = false };
            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync(cancellationToken));
            var dataString = await reader.ReadToEndAsync(cancellationToken);
            var pack = GetApiDataFromString(dataString);
            return pack;
        }

        private static ApiDataPack GetApiDataFromString(string dataString)
        {
            try
            {
                var dataPack = JsonConvert.DeserializeObject<ApiDataPack>(dataString);
                FillInitialTeamSizes(dataPack);
                dataPack.Success = true;
                return dataPack;
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

        private static void FillInitialTeamSizes(ApiDataPack dataPack)
        {
            foreach (var replay in dataPack.Replays)
            {
                var teamSize = replay.TeamBlue.Players.Count < replay.TeamOrange.Players.Count
                    ? replay.TeamBlue.Players.Count
                    : replay.TeamOrange.Players.Count;
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