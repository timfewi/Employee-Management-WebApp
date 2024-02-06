
namespace ClientLibrary.Helpers
{
    public class CustomHttpHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            bool loginUrl = request.RequestUri!.AbsolutePath.Contains("login");
            bool registerUrl = request.RequestUri!.AbsolutePath.Contains("register");
            bool refreshTokenUrl = request.RequestUri!.AbsolutePath.Contains("refresh-token");
            return base.SendAsync(request, cancellationToken);
        }
    }
}
