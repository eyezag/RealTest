using System.Text.Json.Serialization;

namespace RealTest.Contracts.Common;

[JsonConverter(typeof(JsonStringEnumConverter<AccountStatus>))]
public enum AccountStatus
{
    PendingVerification = 1,
    Active = 2,
    Suspended = 3,
    Disabled = 4
}
