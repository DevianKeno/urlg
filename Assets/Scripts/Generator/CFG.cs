using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace URLG.Generator
{
    public class CFG : MonoBehaviour
    {
        
        public struct Token
        {
            public string Key;
            public RoomType RoomType;
        }

        public static string start = "start";
        public static string end = "end";
        public static string content = "content";
        List<Token> tokens = new();

        internal void Initialize()
        {
            var file = Resources.Load<TextAsset>("ruleset");
            string contents = file.text;            
            var tokensraw = contents.Split(';');
            
            foreach (var t in tokensraw)
            {
                var token = t.Trim();
                try
                {
                    var split = token.Split(':');
                    var lhs = split[0];
                    var rhs = split[1];
                    var rules = new List<string>();
                    try
                    {
                        rules = rhs.Split('|').ToList();
                    } catch
                    {
                        rules.Add(rhs);
                    }
                } catch
                {
                    var lhs = token;
                }

                tokens.Add(new Token{
                    Key = token,
                });
            }
            Debug.Log("buffer");
        }

        void Start()
        {
            // Initialize();
        }

        List<string> c = new();
        
        public static string GenerateMissionString()
        {
            string mission = "";
            // c.Add("DUNGEON");
            return mission;
        }

        public static List<Token> Tokenize(string mission)
        {
            var tokens = new List<Token>();
            return tokens;
        }
    }
}