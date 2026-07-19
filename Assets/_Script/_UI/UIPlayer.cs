using TMPro;
using UnityEngine;

public class UIPlayer : MonoBehaviour
{
    [SerializeField] private TMP_Text promptText;

    public void UpdateText(string promptMessage)
    {
        if (promptText != null)
            promptText.text = promptMessage;
    }
}