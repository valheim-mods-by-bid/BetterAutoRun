using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using ValheimConfigSync;

namespace BetterAutoRun
{
	internal enum GroundType
	{
		Terrain,
		Path,
		Unknown
	}

	[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
	internal partial class BetterAutoRun : BaseUnityPlugin
	{
		public const string PluginGUID = PluginInfo.Guid;
		public const string PluginName = PluginInfo.Name;
		public const string PluginVersion = PluginInfo.Version;

		private static readonly AutoRunState RuntimeState = new AutoRunState();

		private readonly Harmony harmony = new Harmony(PluginGUID);
		private ConfigSyncManager configSync;
		private DebugRenderer debugRenderer;
		private PathScanDiagnostics diagnostics;
		private PathScanner pathScanner;
		private RidingController ridingController;
		private Shader legacyShader;
		private Material lineMaterial;
		private float updateTimer;

		internal static AutoRunState State
		{
			get { return RuntimeState; }
		}

		internal static bool IsEnabled
		{
			get { return EnabledConfig != null && EnabledConfig.Value; }
		}

		internal static bool IsRiding(IDoodadController controller = null)
		{
			if (controller == null)
			{
				Player player = Player.m_localPlayer;
				controller = player == null ? null : player.GetDoodadController();
			}
			return controller is Sadle;
		}

		private void Awake()
		{
			Logger.LogInfo($"{PluginName} {PluginVersion} loaded");
			configSync = new ConfigSyncManager(PluginGUID, PluginVersion, Logger);
			CreateConfigValues();
			AutoSprintShortcut = Config.Bind(
				"Client config",
				"AutoSprint",
				new KeyboardShortcut(KeyCode.LeftShift),
				new ConfigDescription("Key used to toggle automatic sprinting"));

			debugRenderer = new DebugRenderer();
			diagnostics = new PathScanDiagnostics(debugRenderer);
			pathScanner = new PathScanner(RuntimeState, diagnostics);
			ridingController = new RidingController(RuntimeState);
			harmony.PatchAll();
		}

		private void OnDestroy()
		{
			RuntimeState.Reset();
			if (configSync != null)
			{
				configSync.Dispose();
				configSync = null;
			}
			if (debugRenderer != null)
			{
				debugRenderer.Dispose();
			}
			if (lineMaterial != null)
			{
				Destroy(lineMaterial);
				lineMaterial = null;
			}
			harmony.UnpatchSelf();
		}

		private void Update()
		{
			if (!IsEnabled)
			{
				debugRenderer.BeginFrame();
				RuntimeState.Reset();
				return;
			}

			Player player = Player.m_localPlayer;
			if (player == null)
			{
				return;
			}

			if (VisualDebugConfig.Value && lineMaterial == null)
			{
				TryInitializeLineMaterial();
			}
			else if (!VisualDebugConfig.Value)
			{
				debugRenderer.BeginFrame();
			}

			bool movementControlActive = player.m_autoRun || IsRiding(player.GetDoodadController());
			if (player.IsDebugFlying() || !movementControlActive)
			{
				RuntimeState.MovementControlActive = false;
				return;
			}

			if (!RuntimeState.MovementControlActive)
			{
				RuntimeState.MovementControlActive = true;
				RuntimeState.LastPosition = Vector3.zero;
				RuntimeState.EvadeJumpAllowedAt =
					Time.time + EvadeJumpGracePeriodMillisConfig.Value / 1000f;
			}

			if (Input.GetMouseButton(1))
			{
				RuntimeState.InitialMoveDirection = player.GetLookDir();
				RuntimeState.PathDirectionOverrideUntil =
					Time.time + PathDirectionOverrideDurationMillisConfig.Value / 1000f;
			}

			updateTimer -= Time.deltaTime;
			if (updateTimer >= 0f)
			{
				return;
			}

			if (VisualDebugConfig.Value)
			{
				debugRenderer.BeginFrame();
			}

			pathScanner.RefreshEnvironment(player.transform.position);
			JumpIfStuck(player);
			Vector3 moveDirection = pathScanner.CalculateMoveDirection(player);
			ridingController.ApplyMoveDirection(player, moveDirection);
			if (VisualDebugConfig.Value)
			{
				diagnostics.Draw();
			}

			updateTimer = UpdateTimeMillisConfig.Value / 1000f;
			RuntimeState.LastPosition = player.transform.position;
		}

		private void JumpIfStuck(Player player)
		{
			if (RuntimeState.LastPosition == Vector3.zero || Time.time < RuntimeState.EvadeJumpAllowedAt)
			{
				return;
			}

			float squaredDistance = (RuntimeState.LastPosition - player.transform.position).sqrMagnitude;
			float movementThreshold = EvadeJumpMovementThresholdConfig.Value;
			if (EvadeJumpEnabledConfig.Value && squaredDistance < movementThreshold * movementThreshold &&
				player.GetStamina() > StaminaMinThresholdConfig.Value)
			{
				player.Jump(true);
			}
		}

		private void TryInitializeLineMaterial()
		{
			legacyShader = Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply");
			if (legacyShader == null)
			{
				foreach (Shader shader in Resources.FindObjectsOfTypeAll<Shader>())
				{
					if (shader.name == "Legacy Shaders/Particles/Alpha Blended Premultiply")
					{
						legacyShader = shader;
						break;
					}
				}
			}

			if (legacyShader != null)
			{
				lineMaterial = new Material(legacyShader);
				debugRenderer.SetMaterial(lineMaterial);
			}
		}
	}
}
