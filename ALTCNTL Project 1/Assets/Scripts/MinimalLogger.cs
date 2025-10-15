using System.Collections;
using UnityEngine;

public class MinimalLogger : MonoBehaviour
{
    void Awake() { Debug.Log("[MinimalLogger] Awake"); }
    void OnEnable() { Debug.Log("[MinimalLogger] OnEnable"); }
    void Start() { Debug.Log("[MinimalLogger] Start"); StartCoroutine(Ping()); }

    IEnumerator Ping()
    {
        while (true)
        {
            Debug.Log($"[MinimalLogger] timeScale={Time.timeScale}, realtime={Time.realtimeSinceStartup:F2}");
            yield return new WaitForSecondsRealtime(1f);
        }
    }
}
