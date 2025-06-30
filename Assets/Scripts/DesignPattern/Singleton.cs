using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DesignPattern
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();
                    if (_instance == null)
                    {
                        GameObject singletonObject = new GameObject(typeof(T).Name);
                        _instance = singletonObject.AddComponent<T>();
                        DontDestroyOnLoad(singletonObject);
                    }
                    // DontDestroyOnLoad(_instance);
                }
                return _instance;
            }
        }

        protected void SingletonInit()
        {
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
    }
}

