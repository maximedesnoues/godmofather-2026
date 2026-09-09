using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class ResolutionController : MonoBehaviour
{
    [SerializeField] private int _width = 1920;
    [SerializeField] private int _height = 1080;
    [SerializeField] private List<(int, int)> _resolutions = new List<(int, int)> { (1920, 1080), (1280, 720), (1024, 768) };
    [SerializeField] private TMP_Dropdown _resolutionDropdown;

    private void UpdateResolution()
    {
        (_width, _height) = _resolutions[_resolutionDropdown.value];
        Screen.SetResolution(_width, _height, Screen.fullScreen);
    }
}
