using UnityEngine;
using UnityEditor;
using System.Collections;

public class SolipsoUtilityEditor : Editor
{
#if UNITY_EDITOR
    [MenuItem("Solipsoid/PauseGame %p")]
    static void PauseGame()
    {
        if (Application.isPlaying)
        {
            Debug.Log("pause game quick key...");
            Debug.Break();
        }
    }
#endif
}
