using UnityEngine;

namespace HyperPuzzleEngine
{
    public class DisableDebuggingInBuilds : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void DisableLoggerOutsideOfEditor()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.unityLogger.logEnabled = true; // Enable logging in Editor or Development build
#else
        Debug.unityLogger.logEnabled = false; // Disable logging in release builds
#endif
        }
    }
}