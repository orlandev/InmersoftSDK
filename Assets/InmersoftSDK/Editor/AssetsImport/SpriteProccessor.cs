using System;
using UnityEditor;
using UnityEngine;

namespace InmersoftSDK.AssetsImport
{
    public class SpriteProccessor : AssetPostprocessor
    {
        /// <summary>
        /// Programado por Orlando Novas Rodriguez
        /// Inmersoft Team 2020
        /// Property of Inmersoft and Orlando Novas Rodriguez 
        /// </summary>
        private void OnPostprocessSprites(Texture2D texture, Sprite[] sprites)
        {
            string lowercPath = assetPath.ToLower();
            bool isInSpriteDir = lowercPath.IndexOf("/sprites/", StringComparison.Ordinal) != -1;

            if (!isInSpriteDir) return;
            TextureImporter textureImporter = (TextureImporter) assetImporter;
            textureImporter.textureType = TextureImporterType.Sprite;
            AssetDatabase.Refresh();
            Debug.Log("Inmersoft SDK: Asset type change to Sprite.");
        }
    }
}