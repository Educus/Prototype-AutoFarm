using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class MovingManager : MonoBehaviour
{
    // PC, NPC의 이동관련 매니저
    // 호출 시 해당 좌표로 이동, 이동이 끝나면 액션(있다면) 호출
    // 이동 중 다른 이동명령 시 이동 중지

    // 아직은 직선 이동만, 이후 AI길찾기 활용하여 수정할 예정
    // PathFinder? NavMeshAgent?

    private Dictionary<Transform, Coroutine> movingObj = new Dictionary<Transform, Coroutine>();

    public void Moving(Transform mover, Vector3 targetPos, float speed, Action onAction = null)
    {
        if (movingObj.ContainsKey(mover))
        {
            StopMoving(mover);
        }

        Coroutine move = StartCoroutine(IEMoving(mover, targetPos, speed, onAction));
        movingObj.Add(mover, move);
    }
    public void StopMoving(Transform mover)
    {
        StopCoroutine(movingObj[mover]);
        movingObj.Remove(mover);
    }

    private IEnumerator IEMoving(Transform mover, Vector3 targetPos, float speed, Action onAction)
    {
        Vector3 startPos = mover.position;

        while(Vector3.Distance(mover.position, targetPos) > 0.01f)
        {
            mover.position = Vector3.MoveTowards(mover.position, targetPos, speed * Time.deltaTime);

            yield return null;
        }

        mover.position = targetPos;

        if (movingObj.ContainsKey(mover))
        {
            movingObj.Remove(mover);
        }

        onAction?.Invoke();
    }
}
