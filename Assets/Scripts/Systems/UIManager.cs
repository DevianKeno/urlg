using System;
using System.Collections.Generic;
using RL.UI;
using UnityEngine;
using UnityEngine.UI;

namespace RL
{
    public class UIManager : MonoBehaviour
    {
        // public ArrowPointer ArrowPointer;
        [SerializeField] Canvas canvas;
        public Canvas Canvas => canvas;
        public TransitionOptions TransitionOptions = new();

        Dictionary<string, GameObject> _prefabsDict = new();

        [SerializeField] Image vignette;
        [SerializeField] Canvas transitionCanvas;

        // void Awake()
        // {
        //     ArrowPointer = GetComponentInChildren<ArrowPointer>();
        // }

        // public void HideArrowPointer()
        // {
        //     ArrowPointer.gameObject.SetActive(false);
        // }

        // public void ShowArrowPointer()
        // {
        //     ArrowPointer.gameObject.SetActive(true);
        // }

        internal void Initialize()
        {
            foreach (GameObject element in Resources.LoadAll<GameObject>("Prefabs/UI"))
            {
               _prefabsDict[element.name] = element;
            }
        }
        
        TransitionEffect transitionEffect;
        bool _hasPendingTransition;

        public void PlayTransitionHalf(Action callback = null)
        {
            var go = Instantiate(Resources.Load<GameObject>("Prefabs/UI/Transition"));
            transitionEffect = go.GetComponent<TransitionEffect>();

            transitionEffect.transform.SetParent(transitionCanvas.transform);
            transitionEffect.SetOptions(TransitionOptions);
            transitionEffect.PlayToHalf(callback);
            _hasPendingTransition = true;
        }

        public void PlayTransitionEnd(Action callback = null)
        {
            if (!_hasPendingTransition) return;

            transitionEffect?.PlayToEnd(callback);
            _hasPendingTransition = false;
        }
        
        /// Taken from UZSG
        /// <summary>
        /// Create an instance of a UI prefab.
        /// </summary>
        /// <typeparam name="T">Window script attach to the root.</typeparam>
        public T Create<T>(string prefabName, bool show = true) where T : Window
        {            
            if (_prefabsDict.ContainsKey(prefabName))
            {
                var go = Instantiate(_prefabsDict[prefabName], Canvas.transform);
                go.name = prefabName;

                if (go.TryGetComponent(out T element))
                {
                    if (!show) element.Hide();
                    return element;
                }
                return default;
            }

            return default;
        }
    }
}