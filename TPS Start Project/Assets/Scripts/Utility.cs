using UnityEngine;
using UnityEngine.AI;

public static class Utility
{
    public static Vector3 GetRandomPointOnNavMesh(Vector3 center, float distance, int areaMask)
    {
        var randomPos = Random.insideUnitSphere * distance + center;
        
        NavMeshHit hit;
        
        NavMesh.SamplePosition(randomPos, out hit, distance, areaMask);
        
        return hit.position;
    }
    
    public static float GedRandomNormalDistribution(float mean, float standard)
    {
        var x1 = Random.Range(0f, 1f);
        var x2 = Random.Range(0f, 1f);
        return mean + standard * (Mathf.Sqrt(-2.0f * Mathf.Log(x1)) * Mathf.Sin(2.0f * Mathf.PI * x2)); //정규분포를 따르는 난수 생성 공식
                                                                                                        //f(x)=A*exp((-z^2)/2)로 표시할 수 있으며, 여기서 A는 Peak 값의 높이,  z는 z = (x - m) / σ 입니다. m은 평균값, σ는 표준편차입니다.
           
    }
}