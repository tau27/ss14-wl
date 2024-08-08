using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Content.Shared._WL.Stand_Fall_Crouch
{
    [RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
    public sealed partial class StandFallCrouchComponent : Component
    {
        [DataField("StandFallToggleAction", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string StandFallToggleAction = "ActionToggleFall";

        [DataField, AutoNetworkedField]
        public EntityUid? StandFallToggleActionEntity;

        [/*ViewVariables(VVAccess.ReadWrite), */DataField("isWantStay"), AutoNetworkedField]
        public bool IsWantStay = true;

        [DataField("walkModifier")/*, AutoNetworkedField/*, ViewVariables(VVAccess.ReadWrite)*/]
        public float WalkModifier = 0.1f;

        [DataField("sprintModifier")/*, AutoNetworkedField/*, ViewVariables(VVAccess.ReadWrite)*/]
        public float SprintModifier = 0.2f;

        [DataField("timeToPutDownSelf")]
        public float TimeToPutDownSelf = 0.15f;
        [DataField("timeToPutDownOther")]
        public float TimeToPutDownOther = 2.0f;
        [DataField("timeToStandUpSelf")]
        public float TimeToStandUpSelf = 1.25f;
        [DataField("timeToStandUpOther")]
        public float TimeToStandUpOther = 3.5f;

        [DataField("isCrawling")] public bool IsCrawling = false;
    }
}
