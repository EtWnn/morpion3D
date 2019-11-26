using System;
using System.Threading;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public static class Utils
{
    /// <summary>
    /// Interpolation iterator between two 3-vectors.
    /// <para>
    /// Smoothed interpolation use F:t -> 2 * t^2 * (1.5 - x) instead of F:t->t when
    /// computing res(t) = init + (traget - init) * F(t) for t in [0, 1]
    /// </para>
    /// </summary>
    ///
    /// <param name="initPos">Initial position.</param>
    /// <param name="targetPos">Target position.</param>
    /// <param name="duration">Duration</param>
    /// <param name="smoothed">If true use a smoothed interpolation instead of a linear interpolation.</param>
    /// <returns>Vector3 interpolation iterator between initPos and targetPos</returns>
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

    /// <summary>
    /// Interpolation iterator between two Quaternion representing rotations.
    /// 
    /// <para>
    /// Smoothed interpolation use F:t -> 2 * t^2 * (1.5 - x) instead of F:t->t when
    /// computing res(t) = init + (traget - init) * F(t) for t in [0, 1]
    /// </para>
    /// </summary>
    /// <param name="initRot">Initial rotation quaternion.</param>
    /// <param name="targetRot">Target rotation quaternion.</param>
    /// <param name="duration">Duration</param>
    /// <param name="smoothed">If true use a smoothed interpolation instead of a linear interpolation.</param>
    /// <returns>Quaternion interpolation iterator between initRot and targetRot</returns>
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

    /// <summary>
    /// Interpolation iterator between two position / rotation pairs.
    /// 
    /// <para>
    /// Smoothed interpolation use F:t -> 2 * t^2 * (1.5 - x) instead of F:t->t when
    /// computing res(t) = init + (traget - init) * F(t) for t in [0, 1]
    /// </para>
    /// </summary>
    /// <param name="initPos">Initial position.</param>
    /// <param name="initRot">Initial rotation quaternion.</param>
    /// <param name="targetPos">Target position.</param>
    /// <param name="targetRot">Target rotation quaternion.</param>
    /// <param name="duration">Duration</param>
    /// <param name="smoothed">If true use a smoothed interpolation instead of a linear interpolation.</param>
    /// <returns>Pair<Vector3, Quaternion> interpolation iterator between initRot and targetRot</returns>
    public static IEnumerator LerpMoveAndRotate(
        Vector3 initPos, Quaternion initRot,
        Vector3 targetPos, Quaternion targetRot,
        float duration, bool smoothed = false)
    {
        Func<float, float> F = (float x) => x;

        if (smoothed)
            F = (float x) => 2 * Mathf.Pow(x, 2) * (1.5f - x);

        float t = 0;
        float dt = Time.timeScale / duration;
        while ((t += dt * Time.deltaTime) < 1)
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

    /// <summary>
    /// Interpolation iterator between two floats.
    /// <para>
    /// Smoothed interpolation use F:t -> 2 * t^2 * (1.5 - x) instead of F:t->t when
    /// computing res(t) = init + (traget - init) * F(t) for t in [0, 1]
    /// </para>
    /// </summary>
    ///
    /// <param name="initVal">Initial value.</param>
    /// <param name="targetVal">Target value.</param>
    /// <param name="duration">Duration</param>
    /// <param name="smoothed">If true use a smoothed interpolation instead of a linear interpolation.</param>
    /// <returns>Float interpolation iterator between initVal and targetVal</returns>
    public static IEnumerator LerpFloat(float initVal, float targetVal, float duration, bool smoothed = false)
    {
        Func<float, float> F = (float x) => x;

        if (smoothed)
            F = (float x) => 2 * Mathf.Pow(x, 2) * (1.5f - x);

        float t = 0;
        float dt = Time.timeScale / duration;
        while ((t += dt * Time.deltaTime) < 1)
        {
            yield return Mathf.Lerp(initVal, targetVal, F(t));
        }
        yield return targetVal;
        yield break;
    }
}

/// <summary>
/// Generic class deriving from EventArgs defining an public property and an constructor to initialize it.
/// </summary>
/// <typeparam name="T">Type of the property.</typeparam>
public class TEventArgs<T> : EventArgs
{
    /// <summary>
    /// EventArgs data to transmit.
    /// </summary>
    public T Data { get; set; }
    
    public TEventArgs(T data)
    {
        Data = data;
    }
}

/// <summary>
/// Thread-safe ressource of type T, with an assosiciated Action, for a producer / single consumer pattern.
/// <para>
/// Writing in the ressource flags it so the consumer can check and known the ressource has been changed.
/// In this case the associated Action is executed while the ressource is still locked.
/// </para>
/// </summary>
/// <typeparam name="T">The type of the ressource.</typeparam>
public class SharedUpdatable<T>
{
    /// <summary>
    /// Action to execute when the ressource has been changed. The Action can safely reads the ressource.
    /// </summary>
    public Action<T> UpdateAction { get; set; }

    private object lockObject;
    private T data;
    private bool upToDate;

    public SharedUpdatable(T value = default)
    {
        lockObject = new object();
        data = value;
        upToDate = true;
        UpdateAction = (_) => {};
    }

    /// <summary>
    /// Wait until the ressource is available and write value in it.
    /// </summary>
    virtual public void Write(T value)
    {
        lock (lockObject)
        {
            data = value;
            upToDate = false;
        }
    }

    /// <summary>
    /// Wait until the ressource is available, check its "update" flag, if the ressource has been updated, execute
    /// <see cref="UpdateAction"/> and release the ressource.
    /// </summary>
    /// <returns>True if the ressource has been processed, fasle otherwise</returns>
    virtual public bool TryProcessIfNew()
    {
        bool processed = false;
        lock (lockObject)
        {
            if (!upToDate)
            {
                UpdateAction(data);
                processed = true;
                upToDate = true;
            }
        }

        return processed;
    }
}

// Based on work from various author, see https://forum.unity.com/threads/extended-coroutines.202064/ for original post and contributions

/// <summary>
/// State for <see cref="CoroutineController"/> class.
/// </summary>
public enum CoroutineState
{
    Ready,
    Running,
    Paused,
    Finished
}

/// <summary>
/// Corountine wrapper providing, pause / resume / stop / reset features, execption handling
/// and an event fired when the coroutine is finsihed.
/// <param>One can use <see cref="CoroutineExtension.CoroutineExtensions.StartCoroutineEx(MonoBehaviour, IEnumerator)"/>
/// extension method to a wrapped coroutine in Unity fashion, as follow <code>this.StartCoroutineEx(routine())</code>.</param>
/// </summary>
public class CoroutineController
{
    /// <summary>
    /// Event fired when the coroutine is finished, the event is not fired if the coroutine is stopped or reset.
    /// </summary>
    public event EventHandler Finished;

    public IEnumerator Routine { get; private set; }
    public Coroutine Coroutine { get; private set; }
    public CoroutineState State { get; private set; }

    /// <summary>
    /// Initialize the object but does not start the coroutine.
    /// </summary>
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

    public void Reset(MonoBehaviour monoBehaviour)
    {
        if (State != CoroutineState.Finished)
            Stop();
        StartCoroutine(monoBehaviour);
    }
}

/// <summary>
/// Extension namespace for <see cref="CoroutineController"/>.
/// You must use this namespace for the extension method to be available.
/// </summary>
namespace CoroutineExtension
{
    /// <summary>
    /// Extension static class for <see cref="CoroutineController"/>.
    /// </summary>
    public static class CoroutineExtensions
    {
        /// <summary>
        /// Extends <c>Unity.MonoBehaviour</c> to start a coroutine wrapper in a
        /// <see cref="CoroutineController"/>.
        /// </summary>
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
}

/// <summary>
/// Wraps an event to keep track of subscribers and provide a method to unsubscribe all of them.
/// </summary>
/// <typeparam name="T">EventHandler delegate type</typeparam>
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
