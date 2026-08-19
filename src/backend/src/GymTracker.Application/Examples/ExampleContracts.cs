namespace GymTracker.Application.Examples;

public sealed record UpsertExampleRequest(
    string Code,
    string Name,
    string Description);

public sealed record ExampleResponse(
    Guid Id,
    string Code,
    string Name,
    string Description);
