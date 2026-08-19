using System.Collections.Generic;
using UnityEngine;

namespace BetterAutoRun
{
	internal sealed class PathScanDiagnostics
	{
		private readonly DebugRenderer renderer;
		private readonly SortedList<float, SortedList<int, PathSampleDebugInfo>> scans =
			new SortedList<float, SortedList<int, PathSampleDebugInfo>>();
		private readonly SortedList<float, bool> extendedChecks = new SortedList<float, bool>();
		private readonly List<DebugLine> lines = new List<DebugLine>();
		private Vector3 startPoint;
		private bool hasSelectedAngle;
		private float selectedAngle;

		internal PathScanDiagnostics(DebugRenderer renderer)
		{
			this.renderer = renderer;
		}

		internal void BeginScan(Vector3 scanStartPoint)
		{
			if (!Enabled)
			{
				return;
			}

			startPoint = scanStartPoint;
			scans.Clear();
			extendedChecks.Clear();
			lines.Clear();
			hasSelectedAngle = false;
			selectedAngle = 0f;
		}

		internal void PrepareAngle(float angle)
		{
			if (Enabled && !scans.ContainsKey(angle))
			{
				scans.Add(angle, new SortedList<int, PathSampleDebugInfo>());
			}
		}

		internal void RecordSample(float angle, int distance, PathSampleDebugInfo sample)
		{
			if (!Enabled)
			{
				return;
			}

			PrepareAngle(angle);
			SortedList<int, PathSampleDebugInfo> scan = scans[angle];
			if (scan.ContainsKey(distance))
			{
				scan.Remove(distance);
			}
			scan.Add(distance, sample);
		}

		internal void RecordLine(Vector3 start, Vector3 end)
		{
			if (Enabled)
			{
				lines.Add(new DebugLine(start, end));
			}
		}

		internal void MarkExtendedCheck(float angle, bool accepted)
		{
			if (!Enabled)
			{
				return;
			}

			if (extendedChecks.ContainsKey(angle))
			{
				extendedChecks[angle] = accepted;
			}
			else
			{
				extendedChecks.Add(angle, accepted);
			}
		}

		internal void MarkSelectedAngle(float angle)
		{
			if (!Enabled)
			{
				return;
			}

			hasSelectedAngle = true;
			selectedAngle = angle;
		}

		internal void Draw()
		{
			if (!Enabled)
			{
				return;
			}

			foreach (DebugLine line in lines)
			{
				renderer.DrawLine(line.Start, line.End, Color.black);
			}
			renderer.DrawPathScans(
				scans,
				startPoint,
				BetterAutoRun.RaycastHeightConfig.Value,
				hasSelectedAngle,
				selectedAngle,
				extendedChecks);
		}

		private static bool Enabled
		{
			get { return BetterAutoRun.VisualDebugConfig != null && BetterAutoRun.VisualDebugConfig.Value; }
		}

		private struct DebugLine
		{
			internal readonly Vector3 Start;
			internal readonly Vector3 End;

			internal DebugLine(Vector3 start, Vector3 end)
			{
				Start = start;
				End = end;
			}
		}
	}
}
