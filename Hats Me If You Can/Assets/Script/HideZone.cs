using UnityEngine;

public class HideZoneTrigger : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        RigidbodyPlayerWithSprintAndStamina player = other.GetComponent<RigidbodyPlayerWithSprintAndStamina>();
        if (player != null && player.IsCrouching())
        {
            player.SetHidden(true);
            Debug.Log("Player is crouching inside HideZone → HIDDEN ✅");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        RigidbodyPlayerWithSprintAndStamina player = other.GetComponent<RigidbodyPlayerWithSprintAndStamina>();
        if (player != null)
        {
            player.SetHidden(false);
            Debug.Log("Player exited HideZone → NOT HIDDEN ❌");
        }
    }
}
