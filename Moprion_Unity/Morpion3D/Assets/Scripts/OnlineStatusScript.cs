using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OnlineStatusScript : MonoBehaviour
{
    public Color OfflineColor;
    public Color OnlineColor;

    public string OfflineText = "Offline";
    public string OnlineText = "Online";

    private Image image;
    private TextMeshProUGUI text;

    // Start is called before the first frame update
    void Start()
    {
        OfflineColor = Color.red;
        OnlineColor = Color.green;

        OfflineText = "Offline";
        OnlineText = "Online";

        image = transform.Find("Background").GetComponent<Image>();
        text = transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>();

        image.color = OfflineColor;
        text.text = OfflineText;
    }

    public void SetOnlineStatus(bool isOnline)
    {
        image.color = isOnline ? OnlineColor : OfflineColor;
        text.text = isOnline ? OnlineText : OfflineText;
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.B))
        //    StartCoroutine("Blink");
    }

    IEnumerator Blink()
    {
        Debug.Log("Blink started");

        float period = 0.2f;
        bool alt = false;
        while (true)
        {
            SetOnlineStatus(alt);
            alt = !alt;
            yield return new WaitForSeconds(period);
        }
    }

    public void SetActive(bool value)
    {
        gameObject.SetActive(value);
    }
}
