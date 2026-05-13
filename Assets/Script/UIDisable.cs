using UnityEngine;

public class UiDisable : MonoBehaviour
{
    public GameObject buttonParent;

    public void DisableButtonOnClick()
    {
        buttonParent.SetActive(false);
    }
}
