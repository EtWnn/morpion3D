using System;
using System.Collections;
using UnityEngine;

public static class Utils
{
    public static IEnumerator LerpMove(Vector3 initPos, Vector3 targetPos, float duration, bool smoothed = false)
    {
        Func<float, float> F = (float x) => x;

        if (smoothed)
            F = (float x) => 2 * Mathf.Pow(x, 2) * (1.5f - x);

        float t = 0;
        float dt = Time.timeScale / duration;
        while ((t += dt*Time.deltaTime) < 1)
        {
            yield return Vector3.Lerp(initPos, targetPos, F(t));
        }
        yield return targetPos;
        yield break;
    }

    public static IEnumerator LerpRotate(Quaternion initRot, Quaternion targetRot, float duration, bool smoothed = false)
    {

        Func<float, float> F = (float x) => x;

        if (smoothed)
            F = (float x) => 2 * Mathf.Pow(x, 2) * (1.5f - x);

        float t = 0;
        float dt = Time.timeScale / duration;
        while ((t += dt * Time.deltaTime) < 1)
        {
            yield return Quaternion.Lerp(initRot, targetRot, F(t));
        }
        yield return targetRot;
        yield break;
    }

    public static IEnumerator LerpMoveAndRotate(
        Vector3 initPos, Quaternion initRot,
        Vector3 targetPos, Quaternion targetRot,
        float duration, bool smoothed = false)
    {
        Func<float, float> F = (float x) => x;

        if (smoothed)
            F = (float x) => 6 * Mathf.Pow(x, 2) * (duration / 2 - x / 3) / Mathf.Pow(duration, 3);

        float t = 0;
        float dt = Time.timeScale / duration;
        while ((t += dt) < 1)
        {
            var Ft = F(t);
            yield return new Tuple<Vector3, Quaternion>(
                Vector3.Lerp(initPos, targetPos, Ft),
                Quaternion.Lerp(initRot, targetRot, Ft)
                );
        }
        yield return new Tuple<Vector3, Quaternion>(targetPos, targetRot);
        yield break;
    }

    public static IEnumerator LerpFloat(float initPos, float targetPos, float duration, bool smoothed = false)
    {
        Func<float, float> F = (float x) => x;

        if (smoothed)
            F = (float x) => 2 * Mathf.Pow(x, 2) * (1.5f - x);

        float t = 0;
        float dt = Time.timeScale / duration;
        while ((t += dt * Time.deltaTime) < 1)
        {
            yield return Mathf.Lerp(initPos, targetPos, F(t));
        }
        yield return targetPos;
        yield break;
    }
}

public class TEventArgs<T> : EventArgs
{
    public T Data { get; set; }
    
    public TEventArgs(T data)
    {
        Data = data;
    }
}

public class StringEventArgs: EventArgs
{
    public string Message { get; set; }
}
