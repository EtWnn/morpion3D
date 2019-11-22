using System;
using System.Threading;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

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

public class SharedUpdatable<T, TResUpdateFunction>
{
    public Func<T, TResUpdateFunction> UpdateAction { get; set; }
    public Action<TResUpdateFunction> FollowUpAction { get; set; }
    
    private object lockObject;
    private T data;
    private bool upToDate;

    public SharedUpdatable(T value = default)
    {
        lockObject = new object();
        data = value;
        upToDate = true;
        UpdateAction = (_) => { return default; };
        FollowUpAction = (_) => {};
    }

    virtual public void Write(T value)
    {
        lock(lockObject)
        {
            data = value;
            upToDate = false;
        }
    }

    virtual public bool TryProcessIfNew()
    {
        bool processed = false;
        TResUpdateFunction updateFunctionResult = default;
        lock(lockObject)
        {
            if (!upToDate)
            {
                updateFunctionResult = UpdateAction(data);
                processed = true;
                upToDate = false;
            }
        }

        if (processed)
            FollowUpAction(updateFunctionResult);

        return processed;
    }
}

public class SharedUpdatable<T>
{
    public Action<T> UpdateAction { get; set; }
    public Action FollowUpAction { get; set; }

    private object lockObject;
    private T data;
    private bool upToDate;

    public SharedUpdatable(T value = default)
    {
        lockObject = new object();
        data = value;
        upToDate = true;
        UpdateAction = (_) => {};
        FollowUpAction = () => { };
    }

    virtual public void Write(T value)
    {
        lock (lockObject)
        {
            data = value;
            upToDate = false;
        }
    }

    virtual public bool TryProcessIfNew()
    {
        bool processed = false;
        lock (lockObject)
        {
            if (!upToDate)
            {
                UpdateAction(data);
                processed = true;
                upToDate = false;
            }
        }

        if (processed)
            FollowUpAction();

        return processed;
    }
}

/// Based on work from various author, see https://forum.unity.com/threads/extended-coroutines.202064/ for original post and contributions

public enum CoroutineState
{
    Ready,
    Running,
    Paused,
    Finished
}

public class CoroutineController
{
    public event EventHandler Finished;

    public IEnumerator Routine { get; private set; }
    public Coroutine Coroutine { get; private set; }
    public CoroutineState State { get; private set; }

    public CoroutineController(IEnumerator routine)
    {
        Routine = routine;
        State = CoroutineState.Ready;
    }

    public void StartCoroutine(MonoBehaviour monoBehaviour) => Coroutine = monoBehaviour.StartCoroutine(Start());

    private IEnumerator Start()
    {
        if (State != CoroutineState.Ready)
            throw new System.InvalidOperationException("Unable to start coroutine in state: " + State);

        State = CoroutineState.Running;
        do
        {
            try
            {
                if (!Routine.MoveNext())
                    State = CoroutineState.Finished;
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Exception in coroutine: " + ex.Message);
                State = CoroutineState.Finished;
                break;
            }

            yield return Routine.Current;
            while (State == CoroutineState.Paused)
                yield return null;
        } while (State == CoroutineState.Running);

        State = CoroutineState.Finished;

        Finished?.Invoke(this, EventArgs.Empty);
    }

    public void Stop()
    {
        if (State != CoroutineState.Running && State != CoroutineState.Paused)
            throw new System.InvalidOperationException("Unable to stop coroutine in state: " + State);

        State = CoroutineState.Finished;
    }

    public void Pause()
    {
        if (State != CoroutineState.Running)
            throw new System.InvalidOperationException("Unable to pause coroutine in state: " + State);

        State = CoroutineState.Paused;
    }

    public void Resume()
    {
        if (State != CoroutineState.Paused)
            throw new System.InvalidOperationException("Unable to resume coroutine in state: " + State);

        State = CoroutineState.Running;
    }

    public void Reset()
    {
        if (State != CoroutineState.Finished)
            throw new System.InvalidOperationException("Unable to reset coroutine in state: " + State);
    }
}

public static class CoroutineExtensions
{
    public static CoroutineController StartCoroutineEx(this MonoBehaviour monoBehaviour, IEnumerator routine)
    {
        if (routine == null)
        {
            throw new System.ArgumentNullException("routine");
        }

        CoroutineController coroutineController = new CoroutineController(routine);
        coroutineController.StartCoroutine(monoBehaviour);
        return coroutineController;
    }
}

public class EventWrapper<T>
{
    private event EventHandler<T> _event;
    private List<EventHandler<T>> _subscribedEventHandlers = new List<EventHandler<T>>();

    public void Subscribe(EventHandler<T> eventHandler)
    {
        _event += eventHandler;
        _subscribedEventHandlers.Add(eventHandler);
    }

    public void Unsubscribe(EventHandler<T> eventHandler)
    {
        if (_subscribedEventHandlers.Contains(eventHandler))
        {
            _event -= eventHandler;
            _subscribedEventHandlers.Remove(eventHandler);
        }
    }

    public void UnsubscribeAll()
    {
        foreach (var eventHandler in _subscribedEventHandlers)
            _event -= eventHandler;
        _subscribedEventHandlers.Clear();
    }

    public void Invoke(object sender, T e) => _event?.Invoke(sender, e);
}
