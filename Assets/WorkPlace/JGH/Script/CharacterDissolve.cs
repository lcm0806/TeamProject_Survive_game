using UnityEngine;

public class CharacterDissolve : MonoBehaviour
{
    public Material dissolveMaterial;
    private float dissolveAmount = 1f;
    private float dissolveSpeed = 2f;

    private Renderer[] renderers;
    private bool isAppearing = false;
    private bool isDisappearing = false;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();

        foreach (var rend in renderers)
        {
            var newMat = new Material(dissolveMaterial);
            rend.material = newMat;
            rend.material.SetFloat("_DissolveAmount", 1f);
        }

        gameObject.SetActive(false);
    }

    void Update()
    {
        if (isAppearing)
        {
            dissolveAmount -= Time.deltaTime * dissolveSpeed;
            SetDissolve(dissolveAmount);
            if (dissolveAmount <= 0f)
            {
                dissolveAmount = 0f;
                isAppearing = false;
                SetDissolve(dissolveAmount);
            }
        }

        if (isDisappearing)
        {
            dissolveAmount += Time.deltaTime * dissolveSpeed;
            SetDissolve(dissolveAmount);
            if (dissolveAmount >= 1f)
            {
                dissolveAmount = 1f;
                isDisappearing = false;
                SetDissolve(dissolveAmount);
                gameObject.SetActive(false);
            }
        }
    }

    public void Appear()
    {
        gameObject.SetActive(true);
        dissolveAmount = 1f;
        SetDissolve(dissolveAmount);
        isAppearing = true;
    }

    public void Disappear()
    {
        isDisappearing = true;
    }

    private void SetDissolve(float value)
    {
        foreach (var rend in renderers)
        {
            rend.material.SetFloat("_DissolveAmount", value);
        }
    }
}