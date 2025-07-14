namespace Content.Server.Chemistry.Components;

[RegisterComponent]
public sealed partial class SolutionSorptionComponent : Component
{
    [DataField]
    public float SorptionRate = 0f;
}
