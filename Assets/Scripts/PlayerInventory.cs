using UnityEngine;
using TMPro;

public class PlayerInventory : MonoBehaviour
{
    public int coinCount = 0;
    public TextMeshProUGUI coinText;

    void Start()
    {
        UpdateUI();
    }

    public void AddCoin()
    {
        coinCount++;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (coinText != null)
            coinText.text = coinCount.ToString() + " / 12";
    }
}
