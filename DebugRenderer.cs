using System.Collections.Generic;
using UnityEngine;

namespace BetterAutoRun
{
	internal sealed class DebugRenderer
	{
		private readonly List<LineRenderer> lineRenderers = new List<LineRenderer>();
		private Material material;
		private int activeLineCount;

		internal void SetMaterial(Material lineMaterial)
		{
			material = lineMaterial;
		}

		internal void BeginFrame()
		{
			activeLineCount = 0;
			foreach (LineRenderer lineRenderer in lineRenderers)
			{
				lineRenderer.enabled = false;
			}
		}

		internal void DrawLine(Vector3 start, Vector3 end, Color color, float width = 0.02f)
		{
			if (material == null)
			{
				return;
			}

			LineRenderer lineRenderer = GetLineRenderer();
			lineRenderer.material = material;
			lineRenderer.startColor = color;
			lineRenderer.endColor = color;
			lineRenderer.startWidth = width;
			lineRenderer.endWidth = width;
			lineRenderer.positionCount = 2;
			lineRenderer.useWorldSpace = true;
			lineRenderer.SetPosition(0, start);
			lineRenderer.SetPosition(1, end);
		}

		internal void DrawPathScans(
			SortedList<float, SortedList<int, PathSampleDebugInfo>> scans,
			Vector3 startPoint,
			float raycastHeight)
		{
			foreach (KeyValuePair<float, SortedList<int, PathSampleDebugInfo>> scan in scans)
			{
				Vector3 segmentStart = startPoint;
				segmentStart.y += raycastHeight;

				foreach (KeyValuePair<int, PathSampleDebugInfo> sampleEntry in scan.Value)
				{
					PathSampleDebugInfo sample = sampleEntry.Value;
					Vector3 target = sample.Target;
					if (!sample.Collision)
					{
						target.y += raycastHeight;
					}

					Color color = Color.green;
					float width = 0.02f;
					if (sample.ColorError)
					{
						color = Color.red;
					}
					if (sample.SlopeError)
					{
						color = Color.magenta;
					}
					if (sample.Collision)
					{
						color = Color.blue;
					}
					if (sample.IsNearCollisionCheck)
					{
						color = sample.Collision ? new Color(1f, 0.271f, 0f) : Color.yellow;
						if (sample.Collision)
						{
							width = 0.04f;
						}
					}

					DrawLine(segmentStart, target, color, width);
					segmentStart = target;
				}
			}
		}

		internal void Dispose()
		{
			foreach (LineRenderer lineRenderer in lineRenderers)
			{
				if (lineRenderer != null)
				{
					Object.Destroy(lineRenderer.gameObject);
				}
			}
			lineRenderers.Clear();
		}

		private LineRenderer GetLineRenderer()
		{
			LineRenderer lineRenderer;
			if (activeLineCount < lineRenderers.Count)
			{
				lineRenderer = lineRenderers[activeLineCount];
			}
			else
			{
				lineRenderer = new GameObject("BetterAutoRun Debug Line").AddComponent<LineRenderer>();
				lineRenderers.Add(lineRenderer);
			}

			activeLineCount++;
			lineRenderer.enabled = true;
			return lineRenderer;
		}
	}
}
