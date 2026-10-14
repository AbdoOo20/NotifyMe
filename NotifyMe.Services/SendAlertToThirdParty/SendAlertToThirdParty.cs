using Microsoft.Extensions.Configuration;
using NotifyMe.Core.Services.Interface;
using System.Text;
using System.Text.Json;

namespace NotifyMe.Services.SendAlertToThirdParty
{
    public class SendAlertToThirdParty : ISendAlertToThirdParty
    {
        private readonly IConfiguration _configuration;

        private readonly IHttpClientFactory _httpClientFactory;

        public SendAlertToThirdParty(IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            _configuration = config;
            _httpClientFactory = httpClientFactory;
        }

        public async Task SendEmailAsync(string to, string subject, string siteUrl)
        {
            try 
            {
                var apiKey = _configuration["SendGrid:ApiKey"];

                var email = _configuration["SendGrid:Email"];

                var client = new SendGrid.SendGridClient(apiKey);

                var htmlBody = BuildEmailHtml(siteUrl);

                var msg = new SendGrid.Helpers.Mail.SendGridMessage()
                {
                    From = new SendGrid.Helpers.Mail.EmailAddress(email, "NotifyMe"),
                    Subject = subject,
                    HtmlContent = htmlBody,
                    PlainTextContent = $"Content changed on {siteUrl}"
                };

                msg.AddTo(to);

                await client.SendEmailAsync(msg);
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
        }
        
        public async Task SendTelegramAsync(string chatId, string message)
        {
            try
            {
                var token = _configuration["Telegram:BotToken"];

                var url = $"https://api.telegram.org/bot{token}/sendMessage?chat_id={chatId}&text={message}";

                var client = _httpClientFactory.CreateClient();

                await client.GetAsync(url);
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
        }

        public async Task SendWebhookAsync(string url, object payload)
        {
            try
            {
                var json = JsonSerializer.Serialize(payload);

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var client = _httpClientFactory.CreateClient();

                await client.PostAsync(url, content);
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
        }

        private string BuildEmailHtml(string siteUrl)
        {
            var domain = _configuration["BaseURL"];

            return $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <meta charset='UTF-8'>
                        <style>
                            body {{
                                font-family: Arial, sans-serif;
                                background-color: #f5f7fa;
                                padding: 20px;
                            }}
                            .container {{
                                background-color: #ffffff;
                                padding: 20px;
                                border-radius: 6px;
                                max-width: 600px;
                                margin: auto;
                            }}
                            .logo {{
                                text-align: center;
                                margin-bottom: 20px;
                            }}
                            .logo img {{
                                max-width: 150px;
                            }}
                            .content {{
                                font-size: 15px;
                                color: #333;
                            }}
                            .alert {{
                                background-color: #fff3cd;
                                border-left: 4px solid #ffc107;
                                padding: 10px;
                                margin: 15px 0;
                            }}
                            .footer {{
                                font-size: 12px;
                                color: #888;
                                margin-top: 20px;
                                text-align: center;
                            }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='logo'>
                                <img src='{domain}/images/logo.png' alt='NotifyMe'>
                            </div>

                            <div class='content'>
                                <h2>🚨 Monitor Alert</h2>

                                <div class='alert'>
                                    We detected a change on the following page:
                                    <br />
                                    <strong>{siteUrl}</strong>
                                </div>

                                <p>
                                    This notification was sent because the content of the page has changed.
                                </p>

                                <p>
                                    You can review the details inside your NotifyMe dashboard.
                                </p>
                            </div>

                            <div class='footer'>
                                © {DateTime.UtcNow.Year} NotifyMe — All rights reserved
                            </div>
                        </div>
                    </body>
                    </html>";
        }
    }
}
