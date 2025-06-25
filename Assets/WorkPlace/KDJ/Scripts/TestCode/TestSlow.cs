using UnityEngine;

public class TestSlow : MonoBehaviour
{
    // private void OnTriggerEnter(Collider other)
    // {
    //     if (other.gameObject.layer == 3)
    //         other.GetComponent<PlayerController>().PlayerSlow(30f);
    // }
    // private void OnTriggerExit(Collider other)
    // {
    //     if (other.gameObject.layer == 3)
    //         other.GetComponent<PlayerController>().OutOfSlow(30f);
    // }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 5f);
    }
}
