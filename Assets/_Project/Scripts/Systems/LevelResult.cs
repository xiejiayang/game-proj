using System.Collections.Generic;
using System.Linq;
using Dujiangyan.Data;

namespace Dujiangyan.Systems
{
    /// <summary>
    /// 结算判定器
    /// </summary>
    public static class LevelResult
    {
        public static PuzzleResult Evaluate(LevelConfigSO config, PuzzleRuntime runtime)
        {
            var result = new PuzzleResult();
            result.consumedResource = runtime.consumedResource;
            result.simulationTime = runtime.simulationTime;

            bool flooded = runtime.villageHitCount >= config.village.floodThreshold;
            bool timeout = runtime.simulationTime >= config.targetDuration * 2f;
            bool criticalDestroyed = runtime.placedBlocks.Any(b => !b.isIndestructible && b.health <= 0f);

            if (flooded)
            {
                result.failReason = FailReason.Flood;
                result.isSuccess = false;
            }
            else if (criticalDestroyed)
            {
                result.failReason = FailReason.Destroyed;
                result.isSuccess = false;
            }
            else if (timeout)
            {
                result.failReason = FailReason.Timeout;
                result.isSuccess = false;
            }
            else if (runtime.simulationTime >= config.targetDuration)
            {
                result.failReason = FailReason.None;
                result.isSuccess = true;
                result.isFrugal = runtime.consumedResource.IsLessOrEqual(config.frugalThreshold);
            }
            else
            {
                // 模拟尚未结束
                result.failReason = FailReason.None;
                result.isSuccess = false;
            }

            result.unlockedGallery = new List<string>();
            if (result.isSuccess)
            {
                result.unlockedGallery.AddRange(config.galleryUnlocks);
                if (result.isFrugal)
                    result.unlockedGallery.AddRange(config.hiddenGalleryUnlocks);
            }

            return result;
        }
    }
}
