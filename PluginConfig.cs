using BepInEx.Configuration;
using ValheimConfigSync;

namespace BetterAutoRun
{
    internal partial class BetterAutoRun
    {
        public static ConfigEntry<bool> GlobalSprintToggleConfig;
        public static ConfigEntry<bool> KeepAutoRunOnJumpConfig;
        public static ConfigEntry<float> StaminaMinThresholdConfig;
        public static ConfigEntry<int> UpdateTimeMillisConfig;
        public static ConfigEntry<int> MaxAngleConfig;
        public static ConfigEntry<int> NumAnglesConfig;
        public static ConfigEntry<int> PathPointsConfig;
        public static ConfigEntry<float> PathToleranceConfig;
        public static ConfigEntry<float> MaxInclineConfig;
        public static ConfigEntry<bool> PavedOnlyConfig;
        public static ConfigEntry<bool> EnabledConfig;
        public static ConfigEntry<bool> VisualDebugConfig;
        public static ConfigEntry<float> HeightmapSearchRadiusConfig;
        public static ConfigEntry<float> RaycastHeightConfig;
        public static ConfigEntry<float> CollisionHeightIncreaseConfig;
        public static ConfigEntry<float> CollisionRayLengthDownConfig;
        public static ConfigEntry<float> NearCollisionDetectionAngleConfig;
        public static ConfigEntry<float> NearCollisionCorrectionAngleConfig;        
        public static ConfigEntry<float> NearCollisionDetectionLengthScaleConfig;
        public static ConfigEntry<float> RidingDistanceFactorConfig;
        public static ConfigEntry<bool> NearCollisionDetectionEnabledConfig;
        public static ConfigEntry<bool> EvadeJumpEnabledConfig;
        public static ConfigEntry<int> EvadeJumpGracePeriodMillisConfig;
        public static ConfigEntry<float> EvadeJumpMovementThresholdConfig;
        public static ConfigEntry<int> PathDirectionOverrideDurationMillisConfig;
        public static ConfigEntry<float> MountSpeedModifierFactorConfig;
        public static ConfigEntry<float> MountStaminaModifierFactorConfig;
        public static ConfigEntry<KeyboardShortcut> AutoSprintShortcut;

        private void CreateConfigValues()
        {
            Config.SaveOnConfigSet = true;

            GlobalSprintToggleConfig = Config.Bind("Client config", "GlobalSprintToggle", false,
                new ConfigDescription("Only sprint in autorun mode, or also in normal run mode (global)"));
            KeepAutoRunOnJumpConfig = Config.Bind("Client config", "KeepAutoRunOnJump", true,
                new ConfigDescription("Keep autorun enabled when the player presses jump"));
            StaminaMinThresholdConfig = Config.Bind("Client config", "StaminaMinThreshold", 30f,
                new ConfigDescription("Stop sprinting if stamina is below this value", new AcceptableValueRange<float>(0f, 250f)));
            UpdateTimeMillisConfig = Config.Bind("Client config", "UpdateMillis", 100,
                new ConfigDescription("Update direction every x milliseconds", new AcceptableValueRange<int>(0, 2000)));
            MaxAngleConfig = Config.Bind("Client config", "MaxAngle", 90,
                new ConfigDescription("Max angle to look for a path", new AcceptableValueRange<int>(1, 180)));
            NumAnglesConfig = Config.Bind("Client config", "NumAngles", 10,
                new ConfigDescription("Number of angles to check a path between current forward direction and MaxAngle", new AcceptableValueRange<int>(1, 36)));
            PathPointsConfig = Config.Bind("Client config", "PathPoints", 10,
                new ConfigDescription("Number of one-metre path samples to check per angle", new AcceptableValueRange<int>(1, 50)));
            PathToleranceConfig = Config.Bind("Client config", "PathTolerance", 0.3f,
                new ConfigDescription("Tolerance a point is still considered a path (0-1)", new AcceptableValueRange<float>(0f, 1f)));
            MaxInclineConfig = Config.Bind("Client config", "MaxInclineRatio", 1.0f,
                new ConfigDescription("Maximum ratio forward/height", new AcceptableValueRange<float>(0.1f, 10f)));
            PavedOnlyConfig = Config.Bind("Client config", "PavedOnly", false,
                new ConfigDescription("Only consider paved roads as paths"));
            EnabledConfig = Config.Bind("Client config", "enabled", true,
                new ConfigDescription("Enable or disable the mod"));
            VisualDebugConfig = Config.Bind("Client config", "VisualDebug", false,
                new ConfigDescription("Enable or disable visual debugging"));
            HeightmapSearchRadiusConfig = Config.Bind("Client config", "HeightmapSearchRadius", 0.5f,
                new ConfigDescription("Radius to look for heightmaps (internal)", new AcceptableValueRange<float>(0.1f, 5f)));
            RaycastHeightConfig = Config.Bind("Client config", "RaycastHeight", 0.5f,
                new ConfigDescription("Height of the raycast for horizontal collision detection", new AcceptableValueRange<float>(0f, 5f)));
            CollisionHeightIncreaseConfig = Config.Bind("Client config", "CollissionHeightIncrease", 1.25f,
                new ConfigDescription("Height added to the vertical collision detection line", new AcceptableValueRange<float>(0f, 5f)));
            CollisionRayLengthDownConfig = Config.Bind("Client config", "CollissonRayLengthDown", 4f,
                new ConfigDescription("Length of the downward collision detection raycast", new AcceptableValueRange<float>(0.5f, 20f)));
            NearCollisionDetectionAngleConfig = Config.Bind("Client config", "NearCollissionDetectionAngle", 30f,
                new ConfigDescription("Angle for near collision detection", new AcceptableValueRange<float>(0f, 180f)));
            NearCollisionCorrectionAngleConfig = Config.Bind("Client config", "NearCollissionCorrectionAngle", 30f,
                new ConfigDescription("Angle for near collision detection", new AcceptableValueRange<float>(0f, 180f)));
            NearCollisionDetectionLengthScaleConfig = Config.Bind("Client config", "NearCollissionDetectionLengthScale", 1f,
                new ConfigDescription("Scale factor for the near collision detection ray", new AcceptableValueRange<float>(0.25f, 5f)));
            RidingDistanceFactorConfig = Config.Bind("Client config", "RidingDistanceFactor", 2f,
                new ConfigDescription("Multiply the number of path samples checked while riding", new AcceptableValueRange<float>(0.001f, 5f)));
            NearCollisionDetectionEnabledConfig = Config.Bind("Client config", "NearCollissionDetectionEnabled", true,
                new ConfigDescription("Correct movement in case of near collission detection"));
            EvadeJumpEnabledConfig = Config.Bind("Client config", "EvadeJumpEnabled", true,
                new ConfigDescription("Jump if stuck somewhere"));
            EvadeJumpGracePeriodMillisConfig = Config.Bind("Client config", "EvadeJumpGracePeriodMillis", 500,
                new ConfigDescription("Delay before evade jump detection starts after automatic movement begins", new AcceptableValueRange<int>(0, 3000)));
            EvadeJumpMovementThresholdConfig = Config.Bind("Client config", "EvadeJumpMovementThreshold", 0.05f,
                new ConfigDescription("Minimum movement between checks to avoid an evade jump, in metres", new AcceptableValueRange<float>(0f, 0.1f)));
            PathDirectionOverrideDurationMillisConfig = Config.Bind("Client config", "PathDirectionOverrideDurationMillis", 3000,
                new ConfigDescription("How long the selected direction is preferred on paths after a mouse input", new AcceptableValueRange<int>(0, 30000)));
            MountSpeedModifierFactorConfig = Config.Bind("Server config", "MountSpeedBaseModifier", 1f,
                new ConfigDescription("Server-controlled mount run speed base modifier", new AcceptableValueRange<float>(0f, 10f)));
            MountStaminaModifierFactorConfig = Config.Bind("Server config", "MountStaminaUsageBaseModifier", 1f,
                new ConfigDescription("Server-controlled stamina usage modifier for mounts", new AcceptableValueRange<float>(0f, 10f)));

			RegisterSynchronizedConfigEntries();
        }

		private void RegisterSynchronizedConfigEntries()
		{
            /*
			configSync.Register(StaminaMinThresholdConfig, ConfigPolicy.ServerRecommended);
			configSync.Register(UpdateTimeMillisConfig, ConfigPolicy.ServerRecommended);
			configSync.Register(MaxAngleConfig, ConfigPolicy.ServerRecommended);
			configSync.Register(NumAnglesConfig, ConfigPolicy.ServerRecommended);
			configSync.Register(PathPointsConfig, ConfigPolicy.ServerRecommended);
			configSync.Register(PathToleranceConfig, ConfigPolicy.ServerRecommended);
			configSync.Register(MaxInclineConfig, ConfigPolicy.ServerRecommended);
			configSync.Register(PavedOnlyConfig, ConfigPolicy.ServerRecommended);
			configSync.Register(HeightmapSearchRadiusConfig, ConfigPolicy.ServerRecommended);
			configSync.Register(RaycastHeightConfig, ConfigPolicy.ServerRecommended);
			configSync.Register(CollisionHeightIncreaseConfig, ConfigPolicy.ServerRecommended);
			configSync.Register(CollisionRayLengthDownConfig, ConfigPolicy.ServerRecommended);
			configSync.Register(NearCollisionDetectionAngleConfig, ConfigPolicy.ServerRecommended);
			configSync.Register(NearCollisionDetectionLengthScaleConfig, ConfigPolicy.ServerRecommended);
			configSync.Register(RidingDistanceFactorConfig, ConfigPolicy.ServerRecommended);
			configSync.Register(NearCollisionDetectionEnabledConfig, ConfigPolicy.ServerRecommended);
			configSync.Register(EvadeJumpEnabledConfig, ConfigPolicy.ServerRecommended);
			configSync.Register(EvadeJumpGracePeriodMillisConfig, ConfigPolicy.ServerRecommended);
			configSync.Register(EvadeJumpMovementThresholdConfig, ConfigPolicy.ServerRecommended);
			configSync.Register(PathDirectionOverrideDurationMillisConfig, ConfigPolicy.ServerRecommended);
            */

			configSync.Register(MountSpeedModifierFactorConfig, ConfigPolicy.ServerAuthoritative);
			configSync.Register(MountStaminaModifierFactorConfig, ConfigPolicy.ServerAuthoritative);
		}
    }
}
