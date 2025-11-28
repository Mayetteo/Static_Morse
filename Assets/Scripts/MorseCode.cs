using System.Collections.Generic;
using UnityEngine;

public static class MorseCodeHelper
{
    private static readonly Dictionary<char, string> morseMap = new Dictionary<char, string>()
    {
        {'A', ".-"}, {'B', "-..."}, {'C', "-.-."}, {'D', "-.."}, {'E', "."},
        {'F', "..-."}, {'G', "--."}, {'H', "...."}, {'I', ".."}, {'J', ".---"},
        {'K', "-.-"}, {'L', ".-.."}, {'M', "--"}, {'N', "-."}, {'O', "---"},
        {'P', ".--."}, {'Q', "--.-"}, {'R', ".-."}, {'S', "..."}, {'T', "-"},
        {'U', "..-"}, {'V', "...-"}, {'W', ".--"}, {'X', "-..-"}, {'Y', "-.--"},
        {'Z', "--.."}, {'0', "-----"}, {'1', ".----"}, {'2', "..---"}, 
        {'3', "...--"}, {'4', "....-"}, {'5', "....."}, {'6', "-...."}, 
        {'7', "--..."}, {'8', "---.."}, {'9', "----."}
    };

    public static string GetMorse(char letter)
    {
        letter = char.ToUpper(letter);
        if (morseMap.ContainsKey(letter))
            return morseMap[letter];
        return "";
    }
}   