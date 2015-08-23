using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace TCore.Live
{
    public class LiveUserInfo
    {
        private string m_sId;
        private string m_sName;
        private string m_sFirstName;
        private string m_sLastName;
        private string m_sLink;
        private LiveUserInfo_Emails m_luie;

        public string id { get { return m_sId; } set { m_sId = value; } }
        public string name { get { return m_sName; } set { m_sName = value; } }
        public string first_name { get { return m_sFirstName; } set { m_sFirstName = value; } }
        public string last_name { get { return m_sLastName; } set { m_sLastName = value; } }
        public string link { get { return m_sLink; } set { m_sLink = value; } }
        public LiveUserInfo_Emails emails { get { return m_luie; } set { m_luie = value; } }
    }

    public class LiveUserInfo_Emails
    {
        private string m_sPreferred;
        private string m_sAccount;

        public string preferred { get { return m_sPreferred; } set { m_sPreferred = value; } }
        public string account { get { return m_sAccount; } set { m_sAccount = value; } }
    }

    public class RestHelper
    {
        public static LiveUserInfo GetUserInfoFromAccessTokenSync(string sAccessToken)
        {
            Task<LiveUserInfo> tlui = GetUserInfoFromAccessToken(sAccessToken);
            tlui.Wait();

            return tlui.Result;
        }

        public static async Task<LiveUserInfo> GetUserInfoFromAccessToken(string sAccessToken)
        {
            LiveUserInfo lui = null;
#if DEBUG
            if (sAccessToken.StartsWith("#666#"))
                {
                // userinfo is encoded after second #
                lui = new LiveUserInfo();
                lui.emails = new LiveUserInfo_Emails();

                int ich = 5;
                int ichNext = sAccessToken.IndexOf('#', ich);

                if (ichNext == -1)
                    ichNext = sAccessToken.Length;

                lui.emails.account = sAccessToken.Substring(ich, ichNext - ich);
                lui.emails.preferred = lui.emails.account;

                ich = ichNext;
                ichNext = sAccessToken.IndexOf('#', ich);
                
                if (ichNext == -1)
                    ichNext = sAccessToken.Length;

                lui.name = sAccessToken.Substring(ich, ichNext - ich);

                return lui;
                }
#endif // DEBUG
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://apis.live.net/v5.0/me");

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
                                                    new MediaTypeWithQualityHeaderValue("application/json"));

            // List data response.
            HttpResponseMessage response = client.GetAsync(String.Format("?access_token={0}", sAccessToken)).Result;
                // Blocking call!
            if (response.IsSuccessStatusCode)
                {
                // Parse the response body. Blocking!
                string s = await response.Content.ReadAsStringAsync();
                JavaScriptSerializer jscript = new JavaScriptSerializer();
                lui = jscript.Deserialize<LiveUserInfo>(s);
                }
            else
                {
                lui = null;
                Console.WriteLine("{0} ({1})", (int) response.StatusCode, response.ReasonPhrase);
                }

            return lui;
        }

    }
}
