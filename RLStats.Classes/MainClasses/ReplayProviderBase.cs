using RLStats_Classes.MainClasses.Interfaces;
using RLStats_Classes.Models;

using System;
using System.ComponentModel;

namespace RLStats_Classes.MainClasses
{
    public class ReplayProviderBase
    {
        protected ProgressState ProgressState { get; private set; }
        public event EventHandler<ProgressState> DownloadProgressUpdated;
        protected BallchasingApi Api { get; }

        public ReplayProviderBase(IAuthTokenInfo tokenInfo)
        {
            if (tokenInfo is null)
                throw new ArgumentNullException(nameof(tokenInfo));
            if (BallchasingApi.Instance is null)
                BallchasingApi.CreateInstance(tokenInfo);
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
    }
}
