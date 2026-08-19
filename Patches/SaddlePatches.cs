using HarmonyLib;
using UnityEngine;

namespace BetterAutoRun.Patches
{
	[HarmonyPatch(typeof(Sadle), nameof(Sadle.UseStamina))]
	internal static class SadleUseStaminaPatch
	{
		static void Prefix(ref float v)
		{
			if (!BetterAutoRun.IsEnabled)
			{
				return;
			}

			v = v * BetterAutoRun.MountStaminaModifierFactorConfig.Value;
			float groupBonus = ModifierMath.CalculateGroupBonus(
				BetterAutoRun.MountStaminaGroupModifierFactorConfig.Value,
				BetterAutoRun.State.FellowRiderCount,
				BetterAutoRun.MaxMountStaminaGroupModifierFactorConfig.Value);
			v = v * (1 - groupBonus);
		}
	}

	[HarmonyPatch(typeof(Sadle), nameof(Sadle.ApplyControlls))]
	internal static class SadleApplyControlsPatch
	{
		static void Prefix(Vector3 moveDir, Vector3 lookDir)
		{
			if (BetterAutoRun.IsEnabled && moveDir.z > 0)
			{
				BetterAutoRun.State.InitialMoveDirection = lookDir;
				BetterAutoRun.State.PathDirectionOverrideUntil =
					Time.time + BetterAutoRun.PathDirectionOverrideDurationMillisConfig.Value / 1000f;
			}

		}
	}
}
