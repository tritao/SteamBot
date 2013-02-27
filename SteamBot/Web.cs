namespace SteamBot
{
    class Web
    {
        public class WebModule : Nancy.NancyModule
        {
            public WebModule()
            {
                Get["/"] = _ => "Hello World!";
            }
        }
    }
}
