using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ShelterEntrance : Structure
{
    [Tooltip("SceneSystem에 등록된 ‘쉘터’ 씬으로 돌아갑니다.")]
    public override void Interact()
    {
        SceneSystem.Instance.LoadShelterScene();
    }
}
