using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace BetterAutoRun
{
	internal sealed class RidingController
	{
		private static readonly FieldInfo SaddleControlDirectionField =
			AccessTools.Field(typeof(Sadle), "m_controlDir");

		private readonly AutoRunState state;
		private readonly Collider[] fellowRiderColliders = new Collider[128];
		private readonly int fellowRiderLayerMask;

		internal RidingController(AutoRunState state)
		{
			this.state = state;
			fellowRiderLayerMask = LayerMask.GetMask("character", "character_net");
		}

		internal void ApplyMoveDirection(Player player, Vector3 moveDirection)
		{
			state.Mount = null;
			state.FellowRiderCount = GetFellowRiderCount(player);
			Sadle saddle = player.GetDoodadController() as Sadle;
			if (saddle != null)
			{
				state.Mount = saddle.GetCharacter();
				SaddleControlDirectionField.SetValue(saddle, moveDirection);
				return;
			}

			player.SetMoveDir(moveDirection);
		}

		private int GetFellowRiderCount(Player player)
		{
			int colliderCount = Physics.OverlapSphereNonAlloc(
				player.transform.position,
				BetterAutoRun.FellowRiderDistanceConfig.Value,
				fellowRiderColliders,
				fellowRiderLayerMask);
			int riderCount = 0;
			for (int index = 0; index < colliderCount; index++)
			{
				Sadle saddle = fellowRiderColliders[index].GetComponentInChildren<Sadle>();
				if (saddle != null && saddle.GetUser() != 0)
				{
					riderCount++;
				}
			}
			return ModifierMath.CountFellowRiders(riderCount);
		}
	}
}
