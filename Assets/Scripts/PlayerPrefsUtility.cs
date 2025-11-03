using System.Collections.Generic;
using UnityEngine;

public static class PlayerPrefsUtility
{
    public static List<string> GetAllKeys()
    {
        var keys = new List<string>();

#if UNITY_EDITOR
        var playerPrefs = UnityEditor.EditorPrefs.GetString("UnityEditor.PlayerSettings.EditorPrefs");
#endif
        return keys;
    }
}
