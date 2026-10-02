using HarmonyLib;

namespace BetterAutoRun.Patches
{
	[HarmonyPatch(typeof(Character), nameof(Character.GetRunSpeedFactor))]
	internal static class CharacterGetRunSpeedFactorPatch
	{
		static void Postfix(Character __instance, ref float __result)
		{
			if (!BetterAutoRun.IsEnabled)
			{
				return;
			}

			if ((__instance == BetterAutoRun.State.Mount) && (__instance.HaveRider()))
			{
				__result = __result * BetterAutoRun.MountSpeedModifierFactorConfig.Value;
			}
		}
	}
}
