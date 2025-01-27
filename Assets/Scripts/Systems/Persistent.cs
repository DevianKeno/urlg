/*
Component Title: Persistent
Data written: October 11, 2024
Date revised: January 26, 2024

Programmer/s:
    Gian Paolo Buenconsejo

Purpose:
    Attached to GameObjects to make them into a singleton.

Data Structures:
    N/A
*/

using UnityEngine;

namespace RL
{
    public class Persistent : MonoBehaviour
    {
        static Persistent _instance;

        void Awake()
        {
            if (_instance != null)
            {
                Destroy(this);
            } else
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }   
        }
    }
}