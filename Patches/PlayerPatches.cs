using HarmonyLib;

namespace BetterAutoRun.Patches
{
	[HarmonyPatch(typeof(Player), "SetControls")]
	internal static class PlayerSetControlsPatch
	{
		static void Prefix(Player __instance, ref bool run, bool autoRun)
		{
			if (!BetterAutoRun.IsEnabled)
			{
				return;
			}

			AutoSprintController.ApplyControls(__instance, ref run, autoRun);
		}
	}

	[HarmonyPatch(typeof(Player), "Update")]
	internal static class PlayerUpdatePatch
	{
		private static void Prefix(Player __instance)
		{
			if (!BetterAutoRun.IsEnabled)
			{
				return;
			}

			AutoSprintController.HandleToggle(__instance);
		}
	}

}
