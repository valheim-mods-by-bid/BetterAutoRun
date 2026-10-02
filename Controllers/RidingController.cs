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

		internal RidingController(AutoRunState state)
		{
			this.state = state;
		}

		internal void ApplyMoveDirection(Player player, Vector3 moveDirection)
		{
			state.Mount = null;
			Sadle saddle = player.GetDoodadController() as Sadle;
			if (saddle != null)
			{
				state.Mount = saddle.GetCharacter();
				SaddleControlDirectionField.SetValue(saddle, moveDirection);
				return;
			}

			player.SetMoveDir(moveDirection);
		}
	}
}
