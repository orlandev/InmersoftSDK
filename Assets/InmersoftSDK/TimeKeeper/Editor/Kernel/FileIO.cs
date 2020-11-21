using System;
using System.IO;
using UnityEditor;

namespace TimeKeeper.Editor.Kernel
{
    public class FileIO
    {
        public static bool LoadSettingsBoolValue(string path)
        {
            if (!File.Exists(path)) return false;
            var file = new StreamReader(path);
            var fileContents = file.ReadToEnd();
            var dataIo = new DataBool();
            EditorJsonUtility.FromJsonOverwrite(fileContents, dataIo);
            file.Close();
            return dataIo.data;
        }

        public static void SaveSettingsBoolValue(string path, bool data)
        {
            var file = new StreamWriter(path);
            var dataBool = new DataBool();
            dataBool.data = data;
            var json = EditorJsonUtility.ToJson(dataBool, true);
            file.WriteLine(json);
            file.Close();
            AssetDatabase.Refresh();
        }

        [Serializable]
        public class DataBool
        {
            public bool data;
        }
    }
}