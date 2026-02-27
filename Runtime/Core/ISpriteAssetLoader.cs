using UnityEngine;

namespace TMPro.Localization
{
    public interface ISpriteAssetLoader
    {
        TMP_SpriteAsset LoadSpriteAsset(string name);
    }
}