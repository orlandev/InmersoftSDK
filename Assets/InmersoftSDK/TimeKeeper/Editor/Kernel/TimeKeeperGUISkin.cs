using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TimeKeeper.Editor.Kernel
{
    public static class TimeKeeperGuiSkin
    {
        private static GUISkin _skin;


        private static readonly Dictionary<string, GUIStyle> CustomStyles = new Dictionary<string, GUIStyle>();

        private static GUISkin Skin
        {
            get
            {
                if (_skin != null) return _skin;

                const string skinName = "TimeKeeperGuiSkin.guiskin";

                _skin =
                    AssetDatabase.LoadAssetAtPath(TimeKeeperConstants.SkinFolder + "/" + skinName, typeof(GUISkin)) as
                        GUISkin;

                // string skinPath = EM_Constants.SkinFolder + "/" + skinName;

                if (_skin == null) Debug.LogError("Couldn't load the GUISkin at " + skinName);

                return _skin;
            }
        }

        public static GUIStyle GetCustomStyle(string styleName)
        {
            if (CustomStyles.ContainsKey(styleName)) return CustomStyles[styleName];

            if (Skin == null) return null;
            var style = Skin.FindStyle(styleName);

            if (style == null)
                Debug.LogError("Couldn't find style " + styleName);
            else
                CustomStyles.Add(styleName, style);

            return style;
        }
    }
}