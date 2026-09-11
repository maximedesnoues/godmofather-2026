using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SoundController : MonoBehaviour
{
    [SerializeField] private Slider _soundSlider;
    [SerializeField] private GameObject _soundText;
    private void Start()
    {
        AudioListener.volume = 1;
    }
    public void UpdateVolume()
    {
        AudioListener.volume = _soundSlider.value;
        _soundText.GetComponent<TextMeshProUGUI>().text = Mathf.RoundToInt(_soundSlider.value * 100) + "%";
    }
    private void OnDestroy()
    {
        Debug.Log("SoundController destroyed, resetting volume to 0");
        AudioListener.volume = 0;
        _soundText.GetComponent<TextMeshProUGUI>().text = "0%";
    }
}
