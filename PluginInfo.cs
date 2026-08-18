using System.Reflection;

[assembly: AssemblyVersion(BetterAutoRun.PluginInfo.AssemblyVersion)]
[assembly: AssemblyFileVersion(BetterAutoRun.PluginInfo.AssemblyVersion)]

namespace BetterAutoRun
{
    internal static class PluginInfo
    {
        public const string Guid = "org.bepinex.plugins.bid.betterautorun";
        public const string Name = "BetterAutoRun";
        public const string Version = "2.0.0";
        public const string AssemblyVersion = Version + ".0";
    }
}
