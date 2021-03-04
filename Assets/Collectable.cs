using UnityEngine;

public class Collectable : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Destroy(gameObject);
        GameplayManager.Score();
        MyoIMUController.Vibrate();
    }
}