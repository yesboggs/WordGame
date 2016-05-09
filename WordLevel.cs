﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]


public class WordLevel {

    public int levelNum;
    public int longWordIndex;
    public string word;
    public Dictionary<char, int> charDict;
    public List<string> subWords;

    static public Dictionary<char, int> MakeCharDict (string w)
    {
        Dictionary<char, int> dict = new Dictionary<char, int>();
        char c;
        for (int i = 0; i < w.Length; i++){
            c = w[i];
            if (dict.ContainsKey(c))
            {
                dict[c]++;
            }
            else
            {
                dict.Add(c, 1);
            }
        }
        return (dict);
    }

    public static bool CheckWordInLevel(string str, WordLevel level)
    {
        Dictionary<char, int> counts = new Dictionary<char, int>();
        for (int i = 0; i < str.Length; i++)
        {
            char c = str[i];

            if (level.charDict.ContainsKey(c))
            {
                if (!counts.ContainsKey(c))
                {
                    counts.Add(c, 1);
                }
                else
                {
                    counts[c]++;
                }
                if(counts[c] > level.charDict[c])
                {
                    return (false);
                }
                
            }
            else
            {
                return (false);
            }
        }
        return (true);
    }
	// Use this for initialization

}
