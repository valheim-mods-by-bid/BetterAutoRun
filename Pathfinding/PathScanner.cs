using System;
using System.Collections.Generic;
using UnityEngine;

namespace BetterAutoRun
{
	internal sealed class PathScanner
	{
		private static readonly string[] DefaultLayers =
		{
			"piece", "terrain", "piece_nonsolid", "static_solid", "Default", "vehicle",
			"pathblocker", "character", "Default_small"
		};

		private static readonly string[] RidingLayers =
		{
			"piece", "terrain", "piece_nonsolid", "static_solid", "Default", "vehicle", "pathblocker"
		};

		private readonly AutoRunState state;
		private readonly PathScanDiagnostics diagnostics;
		private readonly int defaultLayerMask;
		private readonly int ridingLayerMask;
		private readonly int waterLayerMask;
		private readonly List<Heightmap> heightmaps = new List<Heightmap>();
		private readonly Collider[] waterColliders = new Collider[32];
		internal PathScanner(AutoRunState state, PathScanDiagnostics diagnostics)
		{
			this.state = state;
			this.diagnostics = diagnostics;
			defaultLayerMask = LayerMask.GetMask(DefaultLayers);
			ridingLayerMask = LayerMask.GetMask(RidingLayers);
			waterLayerMask = LayerMask.GetMask("WaterVolume");
		}

		internal void RefreshEnvironment(Vector3 position)
		{
			state.WaterHeight = GetWaterHeight(position);
			state.LiquidVolume = FindLiquidVolume(position);
		}

		internal Vector3 CalculateMoveDirection(Player player)
		{
			Vector3 playerForward;
			Vector3 startPoint = GetStartPoint(player, out playerForward);
			Vector3 bestDirection = playerForward;
			int maxTraversablePoints = -1;
			float bestAngle = 0f;
			PathScanResult bestResult = null;
			int requiredTraversablePoints = BetterAutoRun.PathPointsConfig.Value;
			SortedList<float, PathScanResult> regularResults =
				new SortedList<float, PathScanResult>();

			diagnostics.BeginScan(startPoint);
			if (BetterAutoRun.IsRiding(player.GetDoodadController()))
			{
				requiredTraversablePoints = (int)(requiredTraversablePoints * BetterAutoRun.RidingDistanceFactorConfig.Value);
			}

			float angleIncrement = (float)BetterAutoRun.MaxAngleConfig.Value / BetterAutoRun.NumAnglesConfig.Value;
			for (int index = 0; index < BetterAutoRun.NumAnglesConfig.Value; index++)
			{
				for (int side = -1; side <= 1; side += 2)
				{
					float angle = index * angleIncrement * side;
					Vector3 direction = RotateDirection(playerForward, angle);
					diagnostics.PrepareAngle(angle);
					PathScanResult result = ScanDirection(player, startPoint, direction, angle);
					regularResults.Add(angle, result);

					if (result.TraversablePoints > maxTraversablePoints)
					{
						maxTraversablePoints = result.TraversablePoints;
						bestDirection = direction;
						bestAngle = angle;
						bestResult = result;
						if (maxTraversablePoints >= requiredTraversablePoints)
						{
							break;
						}
					}

					if (index == 0)
					{
						break;
					}
				}

				if (maxTraversablePoints >= requiredTraversablePoints)
				{
					break;
				}
			}

			if (bestDirection == playerForward && bestResult != null &&
				bestResult.BlockingReason == PathBlockReason.InvalidGround)
			{
				float ignoredSelectedAngle;
				ApplyNearCollisionCorrection(
					player,
					startPoint,
					bestDirection,
					bestAngle,
					angleIncrement,
					regularResults,
					out ignoredSelectedAngle);
				diagnostics.MarkSelectedAngle(bestAngle);
				return playerForward;
			}

			float selectedAngle;
			Vector3 moveDirection = ApplyNearCollisionCorrection(
				player,
				startPoint,
				bestDirection,
				bestAngle,
				angleIncrement,
				regularResults,
				out selectedAngle);
			diagnostics.MarkSelectedAngle(selectedAngle);
			return moveDirection;
		}

		private PathScanResult ScanDirection(
			Player player,
			Vector3 startPoint,
			Vector3 direction,
			float angle,
			int maxPointsOverride = 0,
			bool isNearCollisionCheck = false)
		{
			int maxPoints = maxPointsOverride;
			if (maxPoints == 0)
			{
				maxPoints = BetterAutoRun.PathPointsConfig.Value;
				if (BetterAutoRun.IsRiding(player.GetDoodadController()) && !isNearCollisionCheck)
				{
					maxPoints = (int)(maxPoints * BetterAutoRun.RidingDistanceFactorConfig.Value);
				}
			}

			float sampleDistance = isNearCollisionCheck
				? BetterAutoRun.NearCollisionDetectionLengthScaleConfig.Value
				: 1f;
			Vector3 lastPosition = startPoint;
			float targetHeight = startPoint.y;
			float lastHeight = targetHeight;
			float accumulatedHeightDifference = 0f;
			PathScanResult result = new PathScanResult();

			for (int distance = 1; distance <= maxPoints; distance++)
			{
				Vector3 targetPosition = startPoint + direction * (distance * sampleDistance);
				targetHeight = targetPosition.y += accumulatedHeightDifference;
				PathSampleDebugInfo sample = new PathSampleDebugInfo
				{
					IsNearCollisionCheck = isNearCollisionCheck,
					Target = targetPosition,
					TargetHeight = targetHeight
				};

				float newHeightDifference;
				if (CollisionDetected(
					player,
					targetPosition,
					lastHeight,
					targetHeight,
					lastPosition,
					out newHeightDifference,
					sample,
					isNearCollisionCheck))
				{
					if (!sample.SlopeError && !sample.ColorError)
					{
						sample.Collision = true;
					}
					result.LastSample = sample;
					result.BlockingReason = GetBlockingReason(sample);
					diagnostics.RecordSample(angle, distance, sample);
					break;
				}

				sample.Target += Vector3.up * newHeightDifference;
				targetHeight += newHeightDifference;
				accumulatedHeightDifference += newHeightDifference;
				result.TraversablePoints++;
				result.LastSample = sample;
				diagnostics.RecordSample(angle, distance, sample);
				lastHeight = targetHeight;
				lastPosition = targetPosition;
			}

			return result;
		}

		private bool CollisionDetected(
			Player player,
			Vector3 targetPosition,
			float lastHeight,
			float targetHeight,
			Vector3 lastPosition,
			out float heightDifference,
			PathSampleDebugInfo sample,
			bool isNearCollisionCheck)
		{
			int layerMask = BetterAutoRun.IsRiding(player.GetDoodadController()) ? ridingLayerMask : defaultLayerMask;
			heightDifference = 0f;
			float heightOffset = BetterAutoRun.RaycastHeightConfig.Value;
			lastPosition.y = lastHeight + heightOffset;
			targetPosition.y = targetHeight + heightOffset;
			Vector3 direction = (targetPosition - lastPosition).normalized;
			float pathLength = Vector3.Distance(lastPosition, targetPosition);

			RaycastHit obstacleHit;
			bool obstacleDetected = Physics.Raycast(lastPosition, direction, out obstacleHit, pathLength, layerMask);
			Vector3 downRayStart = obstacleDetected ? obstacleHit.point : targetPosition;
			downRayStart.y += BetterAutoRun.CollisionHeightIncreaseConfig.Value;
			downRayStart += direction * 0.005f;

			RaycastHit groundHit;
			bool groundDetected = Physics.Raycast(
				downRayStart,
				Vector3.down,
				out groundHit,
				BetterAutoRun.CollisionRayLengthDownConfig.Value,
				layerMask);

			diagnostics.RecordLine(
				downRayStart,
				downRayStart + Vector3.down * BetterAutoRun.CollisionRayLengthDownConfig.Value);

			if (!groundDetected)
			{
				sample.SlopeError = true;
				sample.Target += Vector3.down * BetterAutoRun.CollisionRayLengthDownConfig.Value;
				return true;
			}

			// Only compare against obstacleHit when the horizontal ray actually hit something.
			if (obstacleDetected && groundHit.point.y < obstacleHit.point.y)
			{
				groundHit.point = downRayStart;
			}

			heightDifference = groundHit.point.y - (lastPosition.y - heightOffset);
			float rayLength = obstacleDetected ? obstacleHit.distance : pathLength;
			if (groundHit.collider.gameObject.layer != 11 || !player.IsRunning() ||
				heightDifference < -0.5f || isNearCollisionCheck)
			{
				if (heightDifference != 0f && rayLength / Math.Abs(heightDifference) < BetterAutoRun.MaxInclineConfig.Value)
				{
					sample.Target = groundHit.point;
					sample.TargetHeight = sample.Target.y;
					if (heightDifference < 0f)
					{
						sample.SlopeError = true;
					}
					return true;
				}
			}

			if (groundHit.collider.gameObject.layer == 11)
			{
				Heightmap heightmap;
				Color targetColor = GetHitColor(targetPosition, out heightmap);
				sample.TargetColor = targetColor;
				float worldHeight;
				Heightmap.GetHeight(targetPosition, out worldHeight);
				if (heightmap != null && heightmap.GetLava(targetPosition) > 0.2f)
				{
					sample.ColorError = true;
					return true;
				}

				if (worldHeight <= GetLiquidHeight(targetPosition) ||
					worldHeight + player.m_swimDepth / 2f <= state.WaterHeight)
				{
					sample.ColorError = true;
					return true;
				}

				if (state.StartGroundType == GroundType.Path && !IsPath(targetColor))
				{
					sample.ColorError = true;
					return true;
				}

				return false;
			}

			if (groundHit.collider.gameObject.GetComponentInParent<Piece>() == null &&
				state.StartGroundType == GroundType.Path)
			{
				sample.Target = groundHit.point;
				sample.TargetHeight = sample.Target.y;
				sample.SlopeError = true;
				return true;
			}

			if (state.StartGroundType == GroundType.Terrain && !player.IsRunning())
			{
				sample.Target = groundHit.point;
				sample.TargetHeight = sample.Target.y;
				sample.SlopeError = true;
				return true;
			}

			return false;
		}

		private Vector3 GetStartPoint(Player player, out Vector3 lookDirection)
		{
			state.StartGroundType = GroundType.Unknown;
			Vector3 startPoint = player.transform.position;
			lookDirection = player.transform.forward;

			IDoodadController controller = player.GetDoodadController();
			if (BetterAutoRun.IsRiding(controller))
			{
				Component controlled = controller.GetControlledComponent();
				if (controlled != null)
				{
					startPoint = controlled.transform.position;
					lookDirection = controlled.transform.forward;
				}
			}

			RaycastHit groundHit;
			Vector3 castStart = startPoint + Vector3.up * 0.1f;
			if (Physics.Raycast(
				castStart,
				Vector3.down,
				out groundHit,
				BetterAutoRun.CollisionRayLengthDownConfig.Value,
				defaultLayerMask))
			{
				startPoint.y = groundHit.point.y;
				if (groundHit.collider.gameObject.layer == 11)
				{
					state.StartGroundType = GroundType.Terrain;
					Heightmap heightmap;
					if (IsPath(GetHitColor(groundHit.point, out heightmap)))
					{
						state.StartGroundType = GroundType.Path;
					}
				}
				else if (groundHit.collider.GetComponentInParent<Piece>() != null)
				{
					state.StartGroundType = GroundType.Path;
				}
			}

			bool pathDirectionOverrideActive = Time.time <= state.PathDirectionOverrideUntil;
			if (state.InitialMoveDirection != Vector3.zero &&
				(state.StartGroundType == GroundType.Terrain || pathDirectionOverrideActive))
			{
				lookDirection = state.InitialMoveDirection;
			}

			lookDirection.y = 0f;
			lookDirection.Normalize();
			return startPoint;
		}

		private Vector3 ApplyNearCollisionCorrection(
			Player player,
			Vector3 startPoint,
			Vector3 direction,
			float angle,
			float angleIncrement,
			SortedList<float, PathScanResult> regularResults,
			out float selectedAngle)
		{
			selectedAngle = angle;
			if (!BetterAutoRun.NearCollisionDetectionEnabledConfig.Value)
			{
				return direction;
			}

			float leftAngle = -BetterAutoRun.NearCollisionDetectionAngleConfig.Value;
			float rightAngle = BetterAutoRun.NearCollisionDetectionAngleConfig.Value;
			Vector3 leftDirection = RotateDirection(direction, leftAngle);
			Vector3 rightDirection = RotateDirection(direction, rightAngle);
			float leftScanAngle = angle + leftAngle;
			float rightScanAngle = angle + rightAngle;
			diagnostics.PrepareAngle(leftScanAngle);
			diagnostics.PrepareAngle(rightScanAngle);
			PathScanResult leftResult = ScanDirection(player, startPoint, leftDirection, leftScanAngle, 1, true);
			PathScanResult rightResult = ScanDirection(player, startPoint, rightDirection, rightScanAngle, 1, true);

			if (leftResult.TraversablePoints == 0 && rightResult.TraversablePoints > 0)
			{
				return SelectAdjacentRegularPath(
					player,
					startPoint,
					direction,
					angle,
					angleIncrement,
					1f,
					regularResults,
					out selectedAngle);
			}
			if (rightResult.TraversablePoints == 0 && leftResult.TraversablePoints > 0)
			{
				return SelectAdjacentRegularPath(
					player,
					startPoint,
					direction,
					angle,
					angleIncrement,
					-1f,
					regularResults,
					out selectedAngle);
			}
			if (rightResult.TraversablePoints == 0 && leftResult.TraversablePoints == 0)
			{
				return RotateDirection(direction, angle - 180f);
			}
			return direction;
		}

		private Vector3 SelectAdjacentRegularPath(
			Player player,
			Vector3 startPoint,
			Vector3 direction,
			float angle,
			float angleIncrement,
			float side,
			SortedList<float, PathScanResult> regularResults,
			out float selectedAngle)
		{
			selectedAngle = angle;
			float candidateAngle = angle + angleIncrement * side;
			if (candidateAngle < -BetterAutoRun.MaxAngleConfig.Value ||
				candidateAngle > BetterAutoRun.MaxAngleConfig.Value)
			{
				diagnostics.MarkNearCollisionCandidate(candidateAngle, false);
				return direction;
			}

			PathScanResult candidateResult = FindRegularResult(regularResults, candidateAngle);
			if (candidateResult == null)
			{
				Vector3 candidateDirection = RotateDirection(direction, angleIncrement * side);
				diagnostics.PrepareAngle(candidateAngle);
				candidateResult = ScanDirection(
					player,
					startPoint,
					candidateDirection,
					candidateAngle,
					1,
					false);
			}

			bool candidateAccepted = candidateResult.TraversablePoints > 0;
			diagnostics.MarkNearCollisionCandidate(candidateAngle, candidateAccepted);
			if (!candidateAccepted)
			{
				return direction;
			}

			selectedAngle = candidateAngle;
			return RotateDirection(direction, angleIncrement * side);
		}

		private static PathScanResult FindRegularResult(
			SortedList<float, PathScanResult> regularResults,
			float angle)
		{
			foreach (KeyValuePair<float, PathScanResult> result in regularResults)
			{
				if (Mathf.Abs(result.Key - angle) < 0.001f)
				{
					return result.Value;
				}
			}
			return null;
		}

		private Color GetHitColor(Vector3 point, out Heightmap heightmap)
		{
			heightmaps.Clear();
			Heightmap.FindHeightmap(point, BetterAutoRun.HeightmapSearchRadiusConfig.Value, heightmaps);
			Color paint = Heightmap.m_paintMaskNothing;
			heightmap = null;
			foreach (Heightmap candidate in heightmaps)
			{
				int x;
				int y;
				candidate.WorldToVertex(point, out x, out y);
				paint = candidate.GetPaintMask(x, y);
				heightmap = candidate;
				if (IsPath(paint))
				{
					return paint;
				}
			}
			return paint;
		}

		private float GetWaterHeight(Vector3 position)
		{
			int count = Physics.OverlapSphereNonAlloc(position, 2f, waterColliders, waterLayerMask);
			for (int index = 0; index < count; index++)
			{
				WaterVolume waterVolume = waterColliders[index].GetComponent<WaterVolume>();
				if (waterVolume != null)
				{
					return waterVolume.GetWaterSurface(position);
				}
			}
			return 0f;
		}

		private LiquidVolume FindLiquidVolume(Vector3 position)
		{
			int count = Physics.OverlapSphereNonAlloc(position, 2f, waterColliders, waterLayerMask);
			for (int index = 0; index < count; index++)
			{
				Transform parent = waterColliders[index].transform.parent;
				LiquidVolume liquidVolume = parent == null ? null : parent.GetComponentInChildren<LiquidVolume>();
				if (liquidVolume != null)
				{
					return liquidVolume;
				}
			}
			return null;
		}

		private float GetLiquidHeight(Vector3 position)
		{
			return state.LiquidVolume == null ? 0f : state.LiquidVolume.GetSurface(position);
		}

		private static bool IsPath(Color color)
		{
			if (ColorsAreEqual(color, Heightmap.m_paintMaskPaved))
			{
				return true;
			}
			return !BetterAutoRun.PavedOnlyConfig.Value && ColorsAreEqual(color, Heightmap.m_paintMaskDirt);
		}

		private static bool ColorsAreEqual(Color first, Color second)
		{
			float tolerance = BetterAutoRun.PathToleranceConfig.Value;
			return Mathf.Abs(first.r - second.r) <= tolerance &&
				Mathf.Abs(first.g - second.g) <= tolerance &&
				Mathf.Abs(first.b - second.b) <= tolerance &&
				Mathf.Abs(first.a - second.a) <= tolerance;
		}

		private static Vector3 RotateDirection(Vector3 direction, float angle)
		{
			Vector3 result = Quaternion.Euler(0f, angle, 0f) * direction;
			result.y = 0f;
			result.Normalize();
			return result;
		}

		private static PathBlockReason GetBlockingReason(PathSampleDebugInfo sample)
		{
			if (sample.ColorError)
			{
				return PathBlockReason.InvalidGround;
			}
			if (sample.SlopeError)
			{
				return PathBlockReason.Slope;
			}
			return PathBlockReason.Collision;
		}

	}
}
