using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAirChamber : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 3)
            PlayerManager.Instance.IsInAirChamber = true;
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 3)
            PlayerManager.Instance.IsInAirChamber = false;
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 5f);
    }
}
