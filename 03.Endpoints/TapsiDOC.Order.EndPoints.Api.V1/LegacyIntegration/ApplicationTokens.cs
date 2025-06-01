using Microsoft.IdentityModel.Tokens;
using System.Collections.Concurrent;

namespace TapsiDOC.Order.EndPoints.Api.V1.LegacyIntegration
{
    public static class ApplicationTokens
    {
        public static ConcurrentDictionary<string, SecurityKey> Tokens { get; } = new(StringComparer.Ordinal)
        {
            ["TapsiDocApp"] = JwtSecurityKey.Create("g30zQaUBLjfI0UeDJ1uWVLMLmQSQugUisP1EiPgl8H5i3wscbWyGS84YNbVCP00SHx6ESSBWre6azGvBMQkpdIxuQfv2li7XkGzq1kS5DUOpyzjoKjEZB253K443cY3TI6yzyzpzWMVyTEHyy1BuwccNYOyYfMzBZ7gAAQhIJHwqi1jY3eN1mxLoAXfjAxtSHevCGXHhTaSvuPkXXikVrcJZFeWVUQJO0E2Vlv2qqt5naGqHVvJyVc5Ew56LsidUDFlT8aQUy0wZkIQ1sOpjKRbketkLqwl9B73ZZcvXc1NbH2afmmkFoHR47XuwFhjf")
        };
    }
}
