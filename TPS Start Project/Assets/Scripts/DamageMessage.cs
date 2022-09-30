using UnityEngine;

public struct DamageMessage
{
    public GameObject damager; //공격을 가한측
    public float amount; //공격 데미지양

    public Vector3 hitPoint; //공격이 가해진 위치
    public Vector3 hitNormal; //공격을 맞은 표면이 바라보고 있던 방향(공격한 위치의 반대방향)
}