using System;
using UnityEngine;

namespace Core.Scripts
{
    public class StaticReadyChecker<T> : MonoBehaviour
    {
        private static bool _isReady;
        private static event Action OnReady;
        
        public static T Instance { get; private set; }
        
        public static void DoOnReady(Action action)
        {
            if (_isReady)
            {
                action.Invoke();
            }
            else
            {
                OnReady += action;
            }
        }
        
        protected void SetReady(T instance)
        {
            if (_isReady)
            {
                Debug.LogError($"Second singleton {GetType()} {gameObject.name}");
                return;
            }
            
            Instance = instance;
            OnReady?.Invoke();
        }

        protected void Reset()
        {
            _isReady = false;
            Instance = default;
        }
    }
}