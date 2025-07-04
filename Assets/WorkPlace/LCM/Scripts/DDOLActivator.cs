using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DDOLActivator : MonoBehaviour
{
    [SerializeField]
    private string[] _targetSceneNames;

    private GameObject _objectToControl;

    private void Awake()
    {
        if(transform.parent == null)
        {
            DontDestroyOnLoad(this.gameObject);
        }

        _objectToControl = this.gameObject;
    }

    private void OnEnable()
    {
        // 씬 로드 이벤트를 구독합니다.
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // 씬 로드 이벤트 구독을 해제합니다. (오브젝트가 파괴될 때 중요)
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 현재 로드된 씬의 이름이 _targetSceneNames 배열에 포함되어 있는지 확인합니다.
        bool shouldBeActive = false;
        foreach (string sceneName in _targetSceneNames)
        {
            if (scene.name == sceneName)
            {
                shouldBeActive = true;
                break;
            }
        }

        // 제어할 오브젝트의 활성화 상태를 설정합니다.
        if (_objectToControl != null)
        {
            _objectToControl.SetActive(shouldBeActive);
            Debug.Log($"DDOL 오브젝트 '{_objectToControl.name}' 활성화 상태: {shouldBeActive} (씬: {scene.name})");
        }
    }
}
