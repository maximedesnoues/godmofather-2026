using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SoundController : MonoBehaviour
{
    [SerializeField] private Slider _soundSlider;
    [SerializeField] private GameObject _soundText;
    public void UpdateVolume()
    {
        AudioListener.volume = _soundSlider.value;
        _soundText.GetComponent<TextMeshProUGUI>().text = Mathf.RoundToInt(_soundSlider.value * 100) + "%";
    }
}
