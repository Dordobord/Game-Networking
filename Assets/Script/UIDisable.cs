using UnityEngine;

public class UIDisable : MonoBehaviour
{
    public Canvas mainCanvas;

    public void DisableOnStart()
    {
        mainCanvas.enabled = false;
    }
}
