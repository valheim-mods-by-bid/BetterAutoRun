using UnityEngine;

namespace BetterAutoRun
{
	internal enum PathBlockReason
	{
		None,
		Collision,
		InvalidGround,
		Slope
	}

	internal sealed class PathSampleDebugInfo
	{
		internal Vector3 Target { get; set; }
		internal bool ColorError { get; set; }
		internal bool SlopeError { get; set; }
		internal Color TargetColor { get; set; }
		internal float TargetHeight { get; set; }
		internal bool Collision { get; set; }
		internal bool IsNearCollisionCheck { get; set; }
	}

	internal sealed class PathScanResult
	{
		internal int TraversablePoints { get; set; }
		internal PathBlockReason BlockingReason { get; set; }
		internal PathSampleDebugInfo LastSample { get; set; }
	}
}
