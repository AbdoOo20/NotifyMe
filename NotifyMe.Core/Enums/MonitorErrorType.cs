namespace NotifyMe.Core.Enums
{
    public enum MonitorErrorType
    {
        Timeout,
        Network,
        Dns,
        Ssl,
        HttpError,
        SelectorNotFound,
        EmptyContent,
        Playwright,
        Cancelled,
        RateLimited,
        BusinessRule,
        Unknown
    }

}