using Content.Server.Nutrition.EntitySystems;
using Content.Server.Research.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server.Research.Components;

[RegisterComponent, Access(typeof(ThinkingDomeSystem)), AutoGenerateComponentPause]
public sealed partial class ThinkingDomeComponent : Component
{
    /// <summary>
    /// The sound played when someone's inside
    /// </summary>
    [DataField("thinkingSound")]
    public SoundSpecifier? ThinkingSound;

    public EntityUid? Stream;

    [DataField("working")]
    public bool Working = false;

    [DataField("pointsPerSecond"), ViewVariables(VVAccess.ReadWrite)]
    public int PointsPerSecond = 25;

    [DataField("nextUpdate", customTypeSerializer: typeof(TimeOffsetSerializer)), ViewVariables(VVAccess.ReadWrite)]
    [AutoPausedField]
    public TimeSpan NextUpdate;

    [DataField("updateTime"), ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan UpdateTime = TimeSpan.FromSeconds(1);
}
