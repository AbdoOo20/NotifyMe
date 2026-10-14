namespace NotifyMe.Core.Services.Interface
{
    public interface ISendAlertToThirdParty
    {
        Task SendEmailAsync(string to, string subject, string siteUrl);

        Task SendTelegramAsync(string chatId, string message);

        Task SendWebhookAsync(string url, object payload);
    }
}
