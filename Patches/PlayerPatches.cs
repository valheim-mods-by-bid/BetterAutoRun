using HarmonyLib;

namespace BetterAutoRun.Patches
{
	[HarmonyPatch(typeof(Player), "SetControls")]
	internal static class PlayerSetControlsPatch
	{
		private static void Prefix(Player __instance, ref bool run, bool autoRun, bool jump, out bool __state)
		{
			__state = __instance == Player.m_localPlayer
				&& BetterAutoRun.IsEnabled
				&& BetterAutoRun.KeepAutoRunOnJumpConfig.Value
				&& __instance.m_autoRun
				&& jump;

			if (!BetterAutoRun.IsEnabled)
			{
				return;
			}

			AutoSprintController.ApplyControls(__instance, ref run, autoRun);
		}

		private static void Postfix(Player __instance, bool __state)
		{
			if (__state)
			{
				__instance.m_autoRun = true;
			}
		}
	}

	[HarmonyPatch(typeof(Player), "Update")]
	internal static class PlayerUpdatePatch
	{
		private static void Prefix(Player __instance)
		{
			if (!BetterAutoRun.IsEnabled || __instance != Player.m_localPlayer)
			{
				return;
			}

			AutoSprintController.HandleToggle(__instance);
		}
	}

}
