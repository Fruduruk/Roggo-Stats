using Microsoft.Extensions.Logging;

using RLStats_Classes.Interfaces;
using RLStats_Classes.Models;

using System;
using System.ComponentModel;

namespace RLStats_Classes
{
    public class ReplayProviderBase : IReplayProviderBase
    {
        protected ProgressState ProgressState { get; private set; }
        public event EventHandler<ProgressState> DownloadProgressUpdated;
        protected ILogger Logger { get; private set; }
        protected BallchasingApi Api { get; }

        public ReplayProviderBase(IAuthTokenInfo tokenInfo, ILogger logger)
        {
            Logger = logger;
            if (tokenInfo is null)
                throw new ArgumentNullException(nameof(tokenInfo));

            if (BallchasingApi.Instance is null)
            {
                BallchasingApi.CreateInstance(tokenInfo);
                Logger.LogInformation("Connected to the ballchasing.com api.");
            }
            Api = BallchasingApi.Instance;
            InitializeNewProgress();
        }

        protected void InitializeNewProgress()
        {
            if (ProgressState is not null)
                ProgressState.SomethingChanged -= OnDownloadProgressUpdate;
            ProgressState = new ProgressState();
            ProgressState.SomethingChanged += OnDownloadProgressUpdate;
            void OnDownloadProgressUpdate(object sender, PropertyChangedEventArgs e)
            {
                DownloadProgressUpdated?.Invoke(this, ProgressState);
            }
            ProgressState.Initial = true;
            OnDownloadProgressUpdate(this, new PropertyChangedEventArgs("Initial"));
            ProgressState.Initial = false;
        }

        protected void LastUpdateCall(string message, int count, int falsePartCount = 0)
        {
            ProgressState.FalsePartCount = falsePartCount;
            ProgressState.PartCount = count;
            ProgressState.TotalCount = count;
            ProgressState.CurrentMessage = message;
            ProgressState.LastCall = true;
        }

        public string[] GetApiCalls() => Api.GetCalls();
        public string[] GetAndDeleteApiCalls() => Api.GetAndDeleteCalls();
        public void DeleteApiCalls() => Api.DeleteCalls();
    }
}
