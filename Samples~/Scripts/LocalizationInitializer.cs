using UnityEngine;
using TMPro.Localization;

namespace TMPro.Localization.Samples
{
    /// <summary>
    /// 示例初始化脚本。
    /// 在实际项目中，你应该在游戏的初始化逻辑中调用 Localization.Initialize。
    /// </summary>
    public static class LocalizationInitializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInitialize()
        {
            if (!Localization.Initialized)
            {
                Debug.Log("Initializing Localization System with DataLocalizationLoader (Sample)...");
                Localization.Initialize(new DataLocalizationLoader());
            }
        }
    }
}
