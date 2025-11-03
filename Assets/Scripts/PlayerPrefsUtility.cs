using System.Collections.Generic;
using UnityEngine;

public static class PlayerPrefsUtility
{
    private const string USER_KEY_LIST = "UserKeyList";

    public static void AddUserKey(string key)
    {
        var keys = GetAllKeys();
        if (!keys.Contains(key))
        {
            keys.Add(key);
            SaveKeys(keys);
        }
    }

    public static void RemoveUserKey(string key)
    {
        var keys = GetAllKeys();
        if (keys.Contains(key))
        {
            keys.Remove(key);
            SaveKeys(keys);
        }
    }

    public static List<string> GetAllKeys()
    {
        string raw = PlayerPrefs.GetString(USER_KEY_LIST, "");
        return new List<string>(raw.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries));
    }

    private static void SaveKeys(List<string> keys)
    {
        string joined = string.Join(",", keys);
        PlayerPrefs.SetString(USER_KEY_LIST, joined);
        PlayerPrefs.Save();
    }
}
