using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleTestSceneManager : MonoBehaviour
{
    [SerializeField] private GameObject sceneSpecificQuestUI;

    // Start is called before the first frame update
    void Start()
    {
        if (sceneSpecificQuestUI != null)
        {
            sceneSpecificQuestUI.SetActive(true); // 항상 활성화
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
