using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OnlineStatusScript : MonoBehaviour
{
    public Color OfflineColor;
    public Color OnlineColor;

    private Color currentColor;

    // Start is called before the first frame update
    void Start()
    {
        OfflineColor = Color.red;
        OnlineColor = Color.green;

        currentColor = 
            transform.Find("Canvas/Panel/Indicator/Background")
            .GetComponent<Image>().color;
        currentColor = OnlineColor;
    }

    public void SetOnlineStatus(bool isOnline)
    {
        currentColor = isOnline ? OnlineColor : OfflineColor;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
            StartCoroutine("Blink");
    }

    IEnumerator Blink()
    {
        float period = 0.2f;
        Color previousColor;
        Color nextColor = OnlineColor;
        while(true)
        {
            previousColor = currentColor;
            currentColor = nextColor;
            nextColor = previousColor;
            yield return new WaitForSeconds(period);
        }
    }
}
