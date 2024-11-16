using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Auth.OAuth2.Web;
using System;
using System.Collections.Specialized;
using System.Net;
using System.Threading;
using System.Threading.Tasks;


public class LocalServerCodeReceiver : ICodeReceiver
{
    private readonly string redirectUri;

    public LocalServerCodeReceiver(string redirectUri)
    {
        if (!redirectUri.EndsWith("/"))
        {
            redirectUri += "/";
        }
        this.redirectUri = redirectUri;
    }

    public string RedirectUri => redirectUri;

    public async Task<AuthorizationCodeResponseUrl> ReceiveCodeAsync(AuthorizationCodeRequestUrl url, CancellationToken taskCancellationToken)
    {
        string authorizationUrl = url.Build().AbsoluteUri;

        using (var listener = new HttpListener())
        {
            listener.Prefixes.Add(RedirectUri);
            listener.Start();

            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = authorizationUrl,
                    UseShellExecute = true
                });

                // Wait for the authorization response with cancellation token
                var context = await listener.GetContextAsync().ConfigureAwait(false);

                // Extract query parameters before sending response
                var queryString = context.Request.QueryString;
                LogQueryString(queryString);

                // Create response parameters dictionary
                var responseParameters = new System.Collections.Generic.Dictionary<string, string>();
                foreach (string key in queryString.AllKeys)
                {
                    if (key != null)
                    {
                        responseParameters[key] = queryString[key] ?? string.Empty;
                    }
                }

                // Validate response parameters
                if (queryString == null || !queryString.HasKeys())
                {
                    throw new InvalidOperationException("No query string received.");
                }

                string? code = queryString["code"];
                string? error = queryString["error"];

                if (!string.IsNullOrEmpty(error))
                {
                    throw new InvalidOperationException($"OAuth authorization error: {error}");
                }

                if (string.IsNullOrEmpty(code))
                {
                    throw new InvalidOperationException("No authorization code found in the response.");
                }

                // Send response to browser with a hyperlink to close the window
                using (var response = context.Response)
                {
                    string responseString = @"
                        <html>
                            <head>
                                <title>Authorization Complete</title>
                                <script type='text/javascript'>
                                    function closeWindow() {
                                        window.open('', '_self').close();
                                    }
                                </script>
                            </head>
                            <body>
                                Authorization complete. You can <a href='javascript:closeWindow()'>close</a> this window.
                            </body>
                        </html>";
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                    response.ContentLength64 = buffer.Length;
                    await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                }

                return new AuthorizationCodeResponseUrl(responseParameters);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to receive authorization response: {ex.Message}", ex);
            }
            finally
            {
                listener.Stop();
            }
        }
    }

    private void LogQueryString(NameValueCollection queryString)
    {
        if (queryString == null)
        {
            Console.WriteLine("Query string is null.");
            return;
        }

        Console.WriteLine("Full query string: " + queryString.ToString());

        Console.WriteLine("Received query string parameters:");
        foreach (string? key in queryString.AllKeys)
        {
            if (key != null)
            {
                Console.WriteLine($"{key}: {queryString[key]}");
            }
        }
    }
}