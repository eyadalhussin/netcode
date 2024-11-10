using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OfflineManager : MonoBehaviour
{
    private WaitForSeconds waitTime = new WaitForSeconds(1);
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(CheckInternetConnection());
    }

    IEnumerator CheckInternetConnection()
    {
        while(Application.internetReachability == NetworkReachability.NotReachable)
        {
            DebugManager.Log("No internet connection is available");
            yield return waitTime;
        }
        DebugManager.Log("Internet connection available. Transitioning to  Room Scene.");
        SceneManager.LoadScene("Room");
    }
}
