using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DesignPattern
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static bool _isInitialized = false;
        
        public static T Instance
        {
            get
            {
                // Don't call FindObjectOfType during construction/initialization
                if (_instance == null && _isInitialized)
                {
                    // Only search for existing instance if we're not in construction phase
                    _instance = FindObjectOfType<T>();
                    
                    if (_instance == null)
                    {
                        GameObject singletonObject = new GameObject(typeof(T).Name);
                        _instance = singletonObject.AddComponent<T>();
                        DontDestroyOnLoad(singletonObject);
                    }
                }
                return _instance;
            }
        }
        
        // Alternative safe instance getter that doesn't use FindObjectOfType
        public static T GetInstance()
        {
            if (_instance == null)
            {
                GameObject singletonObject = new GameObject(typeof(T).Name);
                _instance = singletonObject.AddComponent<T>();
                DontDestroyOnLoad(singletonObject);
            }
            return _instance;
        }

        protected virtual void Awake()
        {
            SingletonInit();
        }

        protected void SingletonInit()
        {
            _isInitialized = true;
            
            // 싱글톤 패턴 구현
            if (_instance == null)
            {
                _instance = this as T;
                
                // 부모가 있다면 루트로 이동
                if (transform.parent != null)
                {
                    transform.SetParent(null);
                }
            
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
                _isInitialized = false;
            }
        }
    }
}