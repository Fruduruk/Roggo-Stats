using System;
using RLStats_Classes.MainClasses.Interfaces;

namespace RLStats_Classes.MainClasses
{
    public class ReplayProviderBase
    {
        protected BallchasingApi Api { get; }

        public ReplayProviderBase(IAuthTokenInfo tokenInfo)
        {
            if (tokenInfo is null)
                throw new ArgumentNullException(nameof(tokenInfo));
            if (BallchasingApi.Instance is null)
                BallchasingApi.CreateInstance(tokenInfo);
            Api = BallchasingApi.Instance;
        }
    }
}
