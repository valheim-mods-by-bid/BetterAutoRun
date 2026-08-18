using UnityEngine;

namespace BetterAutoRun
{
	internal static class AutoSprintController
	{
		internal static void ApplyControls(Player player, ref bool run, bool autoRun)
		{
			AutoRunState state = BetterAutoRun.State;
			if (state.SprintEnabled && (BetterAutoRun.GlobalSprintToggleConfig.Value || player.m_autoRun))
			{
				if (state.SprintExhausted && player.GetStamina() >= player.GetMaxStamina())
				{
					state.SprintExhausted = false;
				}
				if (!state.SprintExhausted && player.GetStamina() > BetterAutoRun.StaminaMinThresholdConfig.Value)
				{
					run = true;
				}
				else
				{
					state.SprintExhausted = true;
				}
			}

			if (autoRun && state.InitialMoveDirection == Vector3.zero)
			{
				state.InitialMoveDirection = player.GetLookDir();
			}
			else if (!autoRun && !player.m_autoRun && !BetterAutoRun.IsRiding())
			{
				state.InitialMoveDirection = Vector3.zero;
				state.LastPosition = Vector3.zero;
			}
		}

		internal static void HandleToggle(Player player)
		{
			if (!BetterAutoRun.AutoSprintShortcut.Value.IsDown() ||
				(!BetterAutoRun.GlobalSprintToggleConfig.Value && !player.m_autoRun))
			{
				return;
			}

			AutoRunState state = BetterAutoRun.State;
			if (state.SprintEnabled)
			{
				state.SprintExhausted = false;
			}
			state.SprintEnabled = !state.SprintEnabled;
		}
	}
}
