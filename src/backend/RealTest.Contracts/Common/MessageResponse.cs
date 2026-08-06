namespace RealTest.Contracts.Common;

/// <summary>Generic acknowledgement payload for endpoints that don't return a resource.</summary>
public sealed record MessageResponse(string Message);
