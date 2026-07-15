using UnityEngine;
using TMPro;

public class UIPlayer : MonoBehaviour
{
    [SerializeField]private TextMeshProUGUI prompText;

    void Start()
    {
        
    }

    public void UpdateText(string promptMessage)
    {
        prompText.text = promptMessage;
    }
}
