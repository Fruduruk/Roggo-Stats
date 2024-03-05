using BallchasingWrapper.Models;

namespace BallchasingWrapper.BusinessLogic
{
    public class TokenInfoProvider
    {
        public static AuthTokenInfo GetTokenInfo(string token)
        {
            var t = Task.Run(async () =>
            {
                try
                {
                    HttpResponseMessage response;
                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("Authorization", token);
                        response = await client.GetAsync("https://ballchasing.com/api/");
                    }
                    string content;
                    using (var reader = new StreamReader(await response.Content.ReadAsStreamAsync()))
                    {
                        content = await reader.ReadToEndAsync();
                    }
                    return content;
                }
                catch
                {
                    return string.Empty;
                }
            });
            var dataString = t.Result;
            var info = new AuthTokenInfo(token);
            if (string.IsNullOrEmpty(dataString))
            {
                info.Except = new Exception("This token is does not work.");
                return info;
            }
            dynamic jData = JsonConvert.DeserializeObject(dataString);
            if (jData != null && jData.error is null)
            {
                if (jData.chaser != null)
                    info.Chaser = jData.chaser;
                if (jData.name != null)
                    info.Name = jData.name;
                if (jData.steam_id != null)
                    info.SteamId = jData.steam_id;
                if (jData.type != null)
                    info.Type = jData.type;
                return info;
            }

            if (jData != null)
                info.Except = new Exception(jData.error.ToString());
            return info;
        }
    }
}
