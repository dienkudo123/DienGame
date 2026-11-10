using System.Runtime.InteropServices;
using UnityEngine;

namespace DienGame.Gameplay.Web
{
    public static class DienGameWebMobileBridge
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void DienGame_ConfigureMobileWeb();
#endif

        public static void Configure()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            try
            {
                DienGame_ConfigureMobileWeb();
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"DienGame Web mobile setup failed: {exception.Message}");
            }
#endif
        }
    }
}
