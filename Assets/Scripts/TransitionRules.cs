using UnityEngine;

namespace ConwayGame
{
    public static class TransitionRules
    {
        public static float GetRecoveryChance(int recoveryCountN, float recoveryRateX)
        {
            float chancePercent = Mathf.Max(0, recoveryCountN) * Mathf.Max(0f, recoveryRateX);
            return Mathf.Clamp01(chancePercent / 100f);
        }

        public static float GetInfectionChance(int infectionCountM, float infectionDecayY)
        {
            float decayPercent = Mathf.Max(0, infectionCountM) * Mathf.Max(0f, infectionDecayY);
            return Mathf.Clamp01(1f - decayPercent / 100f);
        }

        public static int GetInfectionTargetCount(int waveStep)
        {
            if (waveStep <= 0)
            {
                return 0;
            }

            if (waveStep <= 2)
            {
                return 8;
            }

            if (waveStep == 3)
            {
                return 5;
            }

            if (waveStep <= 5)
            {
                return 3;
            }

            return 1;
        }
    }
}
