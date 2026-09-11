using UnityEngine;
using UnityEngine.UI;

public class MosquitoPopup : MonoBehaviour
{
    [Header("Popup")]
    [SerializeField] private GameObject popup;

    [Header("Background")]
    [SerializeField] private Image screenBackground;
    [SerializeField] private Sprite normalBackground;
    [SerializeField] private Sprite mosquitoBackground;

    public void ChooseYes()
    {
        screenBackground.sprite = mosquitoBackground;
        popup.SetActive(false);
    }

    public void ChooseNo()
    {
        screenBackground.sprite = normalBackground;
        popup.SetActive(false);
    }

    public void OpenPopup()
    {
        popup.SetActive(true);
    }
}