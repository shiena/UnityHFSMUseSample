using UnityEngine;

public class BallMissSensor : MonoBehaviour
{
    [SerializeField]
    private MainGameScene scene = null;


    private void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.CompareTag("Ball"))
        {
            scene.MissSignal();
        }
    }
}