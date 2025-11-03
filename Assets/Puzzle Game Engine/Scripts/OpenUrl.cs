using UnityEngine;

namespace HyperPuzzleEngine
{
    public class UrlOpener : MonoBehaviour // Renamed class to avoid name conflict
{
    public string url;

    public void OpenWebsite() // Renamed method to avoid name conflict
    {
        Application.OpenURL(url);
    }
}
}