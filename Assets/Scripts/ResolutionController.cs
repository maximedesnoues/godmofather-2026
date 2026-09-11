using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class ResolutionController : MonoBehaviour
{
    [SerializeField] private int _width = 1920;
    [SerializeField] private int _height = 1080;
    [SerializeField] private List<(int, int)> _resolutions = new List<(int, int)> { (1920, 1080), (1280, 720), (1024, 576) };
    [SerializeField] private TMP_Dropdown _resolutionDropdown;

    public void UpdateResolution()
    {
        (_width, _height) = _resolutions[_resolutionDropdown.value];
        Screen.SetResolution(_width, _height, Screen.fullScreen);
        Debug.Log($"Resolution updated to: {_width}x{_height}");
    }

    private void OnDestroy()
    {
        (_width, _height) = _resolutions[Random.Range(0, _resolutions.Count)];
        Screen.SetResolution(_width, _height, Screen.fullScreen);
        Debug.Log($"Resolution updated to: {_width}x{_height}");
    }
}
