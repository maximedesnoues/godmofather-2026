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
    private void OnDisable()
    {
        Debug.Log("SoundController disabled, resetting volume to 0");
        AudioListener.volume = 0;
        _soundText.GetComponent<TextMeshProUGUI>().text = "0%";
    }
}
