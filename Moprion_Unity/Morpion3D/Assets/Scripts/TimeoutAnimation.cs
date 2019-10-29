using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeoutAnimation : MonoBehaviour
{
    public float Duration;
    private Image Stator;

    // Start is called before the first frame update
    void Start()
    {
        Duration = 5;
        Stator = transform.Find("Stator").GetComponent<Image>();
        StartTimeout();
    }

    public void StartTimeout()
    {
        StartCoroutine(TimeoutAnim());
    }

    IEnumerator TimeoutAnim()
    {
        float t = 0;
        float dt = Time.timeScale / Duration;
        while (t < 1)
        {
            Stator.fillAmount = 1 - t;
            t += dt;
            yield return null;
        }
        yield return null;
    }
}
