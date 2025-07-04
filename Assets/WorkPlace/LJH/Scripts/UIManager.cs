using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private TextMeshProUGUI popupText;
    [SerializeField] private float displayDuration = 2f;

    private Coroutine currentRoutine;

    private void Start()
    {
        AudioSystem.Instance.StopBGM();
        AudioSystem.Instance.PlayBGMByName("Lonely Martian");
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ShowPopup(string message)
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(ShowPopupRoutine(message));
    }

    private IEnumerator ShowPopupRoutine(string message)
    {
        popupText.text = message;
        popupText.alpha = 1f;

        yield return new WaitForSeconds(displayDuration);

        // ������ �������
        float fadeTime = 0.5f;
        float t = 0f;
        while (t < fadeTime)
        {
            popupText.alpha = Mathf.Lerp(1f, 0f, t / fadeTime);
            t += Time.deltaTime;
            yield return null;
        }

        popupText.alpha = 0f;
    }
}
