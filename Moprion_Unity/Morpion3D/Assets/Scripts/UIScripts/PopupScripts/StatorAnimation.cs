using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;


public class StatorAnimation : MonoBehaviour
{
    public enum EAnimation
    {
        Timeout,
        Rotate,
        Pulse,
    }

    public event EventHandler<TEventArgs<EAnimation>> Finished;
    private Image stator;
    private Image rotator;
    private Color baseStatorColor;

    public void StartTimeout(float duration)
    {
        rotator.fillAmount = 1;
        StartCoroutine(TimeoutAnim(duration));
    }

    public void StartPulse(Color color, float period, int numPulses = 0)
    {
        rotator.fillAmount = 0;
        StartCoroutine(PulseAnim(baseStatorColor, color, period, numPulses));
    }

    public void StartRotate(float period, float numTurns = 0, float fillAmount = 0.25f)
    {
        rotator.fillAmount = fillAmount;
        StartCoroutine(RotateRotatorAnim(period, numTurns));
    }

    public void Interrupt()
    {
        StopAllCoroutines();
    }

    // Start is called before the first frame update
    void Awake()
    {
        stator = GetComponent<Image>();
        baseStatorColor = stator.color;
        rotator = transform.Find("Rotator").GetComponent<Image>();
    }

    /// Used for test purpose

    //private void Start()
    //{
    //    Finished += OnFinished;
    //    StartTimeout(3);
    //}

    //private void OnFinished(object sender, TEventArgs<EAnimation> e)
    //{
    //    switch (e.Data)
    //    {
    //        case EAnimation.Timeout:
    //            StartRotate(1, 3);
    //            break;
    //        case EAnimation.Rotate:
    //            StartPulse(rotator.color, 1, 3);
    //            break;
    //        case EAnimation.Pulse:
    //            StartTimeout(3);
    //            break;
    //        default:
    //            break;
    //    }
    //}

    IEnumerator TimeoutAnim(float duration)
    {
        float t = 0;
        float dt = Time.timeScale / duration;
        while ((t += dt*Time.deltaTime) < 1)
        {
            rotator.fillAmount = 1 - t;
            yield return null;
        }
        rotator.fillAmount = 0;
        Finished?.Invoke(this, new TEventArgs<EAnimation>(EAnimation.Timeout));
    }

    IEnumerator PulseAnim(Color c1, Color c2, float period, int numPulses = 0)
    {
        float t = 0;
        float dt = Time.timeScale / period;
        float duration = numPulses == 0 ? Mathf.Infinity : numPulses;

        Func<float, float> F = (x) => (Mathf.Cos(Mathf.PI * (2*x + 1)) + 1)/2;
        while ((t += dt * Time.deltaTime) < duration)
        {
            stator.color = Color.Lerp(c1, c2, F(t));
            yield return null;
        }
        stator.color = c1;
        Finished?.Invoke(this, new TEventArgs<EAnimation>(EAnimation.Pulse));
    }

    IEnumerator RotateRotatorAnim(float period, float numTurns = 0)
    {
        float t = 0;
        float dt = Time.timeScale / period;
        float duration = numTurns == 0 ? Mathf.Infinity : numTurns;

        while ((t += dt * Time.deltaTime) < duration)
        {
            rotator.gameObject.transform.eulerAngles = new Vector3(0, 0, -360f * t);
            yield return null;
        }
        rotator.gameObject.transform.eulerAngles = Vector3.zero;
        Finished?.Invoke(this, new TEventArgs<EAnimation>(EAnimation.Rotate));
    }
}
