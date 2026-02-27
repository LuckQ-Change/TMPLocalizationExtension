using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;

namespace TMPro.Localization
{
    public static class SpriteTagParser
    {
        private static Dictionary<string, TMP_SpriteAsset> _spriteAssetCache = new Dictionary<string, TMP_SpriteAsset>();
        private static ISpriteAssetLoader _loader;
        
        // 匹配 <sprite="asset_name"> 或 <sprite name="asset_name">
        private static readonly Regex SpriteTagRegex = new Regex(@"<sprite(?:(?:\s+name)?\s*=\s*""([^""]+)"")?[^>]*>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static void Initialize(ISpriteAssetLoader loader)
        {
            _loader = loader;
        }

        /// <summary>
        /// 解析并确保文本中引用的 SpriteAsset 已加载。
        /// </summary>
        public static void PreloadSprites(string text)
        {
            if (string.IsNullOrEmpty(text) || _loader == null) return;

            var matches = SpriteTagRegex.Matches(text);
            foreach (Match match in matches)
            {
                if (match.Groups.Count > 1 && match.Groups[1].Success)
                {
                    string assetName = match.Groups[1].Value;
                    if (!string.IsNullOrEmpty(assetName))
                    {
                        GetSpriteAsset(assetName);
                    }
                }
            }
        }

        public static TMP_SpriteAsset GetSpriteAsset(string assetName)
        {
            if (_spriteAssetCache.TryGetValue(assetName, out var asset))
            {
                return asset;
            }

            if (_loader != null)
            {
                asset = _loader.LoadSpriteAsset(assetName);
                if (asset != null)
                {
                    _spriteAssetCache[assetName] = asset;
                }
                return asset;
            }

            return null;
        }

        public static void ClearCache()
        {
            _spriteAssetCache.Clear();
        }
    }
}