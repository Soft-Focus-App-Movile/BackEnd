using SoftFocusBackend.Users.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Subscription.Application.Commands;

public class CreateBasicSubscriptionCommand
{
    public string UserId { get; set; } = string.Empty;
    public UserType UserType { get; set; }
}
