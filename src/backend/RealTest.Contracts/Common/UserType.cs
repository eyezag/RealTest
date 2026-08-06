using System.Text.Json.Serialization;

namespace RealTest.Contracts.Common;

/// <summary>
/// Public contract mirror of the domain UserType enum. Kept separate from the domain
/// so the wire format can stay stable even if internal domain modeling changes.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<UserType>))]
public enum UserType
{
    Tester = 1,
    CompanyUser = 2,
    Administrator = 3
}
