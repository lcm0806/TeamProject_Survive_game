using UnityEngine;

public class StatusWatcher : MonoBehaviour
{
    private double _prevOxygen;
    private double _prevEnergy;
    private double _prevDurability;

    [SerializeField] private ShelterUI shelterUI;

    private void Start()
    {
        _prevOxygen = StatusSystem.Instance.GetOxygen();
        _prevEnergy = StatusSystem.Instance.GetEnergy();
        _prevDurability = StatusSystem.Instance.GetDurability();

        if (shelterUI == null)
        {
            shelterUI = FindObjectOfType<ShelterUI>();
        }
    }

    private void Update()
    {
        double curOxygen = StatusSystem.Instance.GetOxygen();
        double curEnergy = StatusSystem.Instance.GetEnergy();
        double curDurability = StatusSystem.Instance.GetDurability();

        if (!Mathf.Approximately((float)curOxygen, (float)_prevOxygen) ||
            !Mathf.Approximately((float)curEnergy, (float)_prevEnergy) ||
            !Mathf.Approximately((float)curDurability, (float)_prevDurability))
        {
            shelterUI.DisplayIndicators(0);
        }

        _prevOxygen = curOxygen;
        _prevEnergy = curEnergy;
        _prevDurability = curDurability;
    }
}