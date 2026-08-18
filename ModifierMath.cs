using System;

namespace BetterAutoRun
{
    internal static class ModifierMath
    {
        internal static float CalculateGroupBonus(float modifierPerRider, int fellowRiderCount, float maximumBonus)
        {
            float nonNegativeModifier = Math.Max(0f, modifierPerRider);
            int nonNegativeRiderCount = Math.Max(0, fellowRiderCount);
            float nonNegativeMaximum = Math.Max(0f, maximumBonus);

            return Math.Min(nonNegativeModifier * nonNegativeRiderCount, nonNegativeMaximum);
        }

        internal static int CountFellowRiders(int occupiedSaddles)
        {
            return Math.Max(0, occupiedSaddles - 1);
        }
    }
}
