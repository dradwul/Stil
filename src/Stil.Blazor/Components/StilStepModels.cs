namespace Stil.Blazor.Components;

/// <summary>The state of a step in a <see cref="StilStepper"/>.</summary>
public enum StilStepState
{
    /// <summary>Done before the current step, upcoming after it.</summary>
    Auto,
    Upcoming,
    Done,
    Error
}

/// <summary>A step in a <see cref="StilStepper"/>.</summary>
/// <param name="Title">The step's name.</param>
/// <param name="Description">An optional line under the name.</param>
/// <param name="State">Auto follows the current step; set Done or Error to override it.</param>
public sealed record StilStepItem(string Title, string? Description = null, StilStepState State = StilStepState.Auto);
