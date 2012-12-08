using System.Web.Mvc;
using CSharp.Elance.Api.Interfaces;
using CSharp.Elance.Connect;
using Spring.Json;
using Spring.Social.OAuth2;

namespace CSharp.Elance.MVC_3_Example.Controllers
{
    public class ElanceController : Controller
    {
        // Configure the Callback URL
        private const string CallbackUrl = "http://localhost:55002/Elance/Callback";

        // Set your consumer client id (key) and secret
        private const string ElanceApiClientId = "ENTER YOUR CLIENT ID HERE";
        private const string ElanceApiSecret = "ENTER YOUR SECRET HERE";

        readonly IOAuth2ServiceProvider<IElance> _elanceProvider = new ElanceServiceProvider(ElanceApiClientId, ElanceApiSecret);

        public ActionResult Index()
        {
            var accessGrant = Session["AccessGrant"] as AccessGrant;
            if (accessGrant != null)
            {
                //Until explict Elance endpoints have API bindings, the following
                //method can be employed to consume Elance endpoints
                var elanceClient = _elanceProvider.GetApi(accessGrant.AccessToken);
                var result = elanceClient.RestOperations.GetForObject<JsonValue>("http://api.elance.com/api2/profiles/my?access_token=" + accessGrant.AccessToken);

                ViewBag.AccessToken = accessGrant.AccessToken;
                ViewBag.ResultText = result.ToString();

                return View();
            }

            var parameters = new OAuth2Parameters
            {
                RedirectUrl = CallbackUrl,
                Scope = "basicInfo"
            };
            return Redirect(_elanceProvider.OAuthOperations.BuildAuthorizeUrl(GrantType.AuthorizationCode, parameters));
        }

        public ActionResult Callback(string code)
        {
            AccessGrant accessGrant = _elanceProvider.OAuthOperations.ExchangeForAccessAsync(code, CallbackUrl, null).Result;

            Session["AccessGrant"] = accessGrant;

            return RedirectToAction("Index");
        }
    }
}
