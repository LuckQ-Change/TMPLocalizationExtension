using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Config;
using TMPro.Localization;
using UnityEngine;

namespace DefaultNamespace
{
    public class DataLocalizationLoader : ILocalizationLoader
    {
        public Dictionary<long, LocalizedTextEntry> Load(CultureInfo culture)
        {
            ConfigKit.LoadLanguage((path) =>
            {
                // 使用 Resources.Load 加载资源，更符合 UPM 结构
                // 注意：当作为 Sample 导入后，资源将位于 Assets/Samples/ 下的 Resources 目录中
                var textAsset = Resources.Load<TextAsset>("Localization/" + path);
                if (textAsset != null)
                {
                    return textAsset.bytes;
                }
                Debug.LogError($"Localization data file not found in Resources: Localization/{path}");
                return null;
            });

            var dictionary = new Dictionary<long, LocalizedTextEntry>();

            foreach (var kvp in LanguageCategory.DataMap)
            {
                dictionary[kvp.Key] = new LocalizedTextEntry
                {
                    Id = kvp.Key,
                    Text = GetLanguage(kvp.Value, culture),
                };
            }

            return dictionary;
        }

        private string GetLanguage(LanguageTable table, CultureInfo culture)
        {
            switch (culture.Name)
            {
                case "zh-CN": return table.ZhCn;
                case "en-US": return table.EnUs;
                default: return table.EnUs;
            }
        }
    }
}