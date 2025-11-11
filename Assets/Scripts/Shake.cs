using System.Collections;
using UnityEngine;

public class Shake : MonoBehaviour
{
    public bool start = false;
    public AnimationCurve curve;
    public float duration = 1f;
    public Vector3 offset = Vector3.zero;
    public Transform player;

    private void Start()
    {
        offset = player.position - transform.position;
    }

    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Alpha3))
        //{
        //    start = true;
        //}

        if (start)
        {
            start = false;
            StartCoroutine(Shaking());
        }
    }

    IEnumerator Shaking()
    {
        //// sha'ke
        //float elapsedTime = 0f;
        //while (elapsedTime < duration)
        //{
        //    elapsedTime += Time.deltaTime;
        //    float strength = curve.Evaluate(elapsedTime / duration);
        //    transform.position += Random.insideUnitSphere * strength;
        //    yield return null;
        //}
        //Vector3 finalPos = transform.position;
        //// rewind
        //elapsedTime = 0f;
        //while (elapsedTime < 0.5f)
        //{
        //    elapsedTime += Time.deltaTime;
        //    float strength = elapsedTime / 0.5f;
        //    transform.position += ((player.position - offset)- finalPos)  * strength;
        //    yield return null;
        //}
        //transform.position = player.position - offset;
        // shake
        float elapsedTime = 0f;
        Vector3 localShakeOffset = Vector3.zero; // 本地坐标系下的抖动偏移

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float strength = curve.Evaluate(elapsedTime / duration);

            // 在本地坐标系中累积抖动偏移
            Vector3 newLocalShake = Random.insideUnitSphere * strength;
            transform.position += newLocalShake;
            localShakeOffset += newLocalShake;

            yield return null;
        }

        // rewind - 平滑消除本地坐标系中的抖动偏移
        elapsedTime = 0f;
        Vector3 startRewindOffset = localShakeOffset; // 开始回归时的总抖动偏移量
        Vector3 startRewindPosition = transform.position; // 开始回归时的位置

        while (elapsedTime < 0.5f)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / 0.5f;

            // 计算当前帧应该剩余的偏移量（从startRewindOffset归零）
            Vector3 currentRemainingOffset = Vector3.Lerp(startRewindOffset, Vector3.zero, t);

            // 直接计算目标位置：开始位置减去剩余偏移量
            transform.position = startRewindPosition - currentRemainingOffset;

            yield return null;
        }

        // 最终确保完全消除抖动偏移
        transform.position = startRewindPosition;
    }
}