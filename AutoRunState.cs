using UnityEngine;

namespace BetterAutoRun
{
	internal sealed class AutoRunState
	{
		internal bool SprintEnabled { get; set; }
		internal bool SprintExhausted { get; set; }
		internal Vector3 InitialMoveDirection { get; set; } = Vector3.zero;
		internal float PathDirectionOverrideUntil { get; set; }
		internal float WaterHeight { get; set; }
		internal LiquidVolume LiquidVolume { get; set; }
		internal Vector3 LastPosition { get; set; } = Vector3.zero;
		internal bool MovementControlActive { get; set; }
		internal float EvadeJumpAllowedAt { get; set; }
		internal GroundType StartGroundType { get; set; } = GroundType.Unknown;
		internal Character Mount { get; set; }
		internal int FellowRiderCount { get; set; }

		internal void Reset()
		{
			SprintEnabled = false;
			SprintExhausted = false;
			InitialMoveDirection = Vector3.zero;
			PathDirectionOverrideUntil = 0f;
			WaterHeight = 0f;
			LiquidVolume = null;
			LastPosition = Vector3.zero;
			MovementControlActive = false;
			EvadeJumpAllowedAt = 0f;
			StartGroundType = GroundType.Unknown;
			Mount = null;
			FellowRiderCount = 0;
		}
	}
}
