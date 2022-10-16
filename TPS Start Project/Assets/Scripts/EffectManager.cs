//EffectManager는 싱글톤을 통해 외부 스크립트에서 접근하여 원하는 위치에 원하는 종류의 Effect를 생성 해주는 역할
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    private static EffectManager m_Instance; 
    public static EffectManager Instance
    {
        get
        {
            if (m_Instance == null) m_Instance = FindObjectOfType<EffectManager>();
            return m_Instance;
        }
    }

    public enum EffectType
    {
        Common, //일반적인 효과
        Flesh //피부나 살에 부딪혔을때 효과
    }
    
    public ParticleSystem commonHitEffectPrefab; //복제 생성
    public ParticleSystem fleshHitEffectPrefab; //피 효과
    
    public void PlayHitEffect(Vector3 pos, Vector3 normal, Transform parent = null, EffectType effectType = EffectType.Common) //parent가 있는 이유는 움직이는 부모를 따라 같이 움직이며 생성이 되어야할 때가 있기에
    {
        var targetPrefab = commonHitEffectPrefab;

        if(effectType == EffectType.Flesh)
        {
            targetPrefab = fleshHitEffectPrefab;
        }

        var effect = Instantiate(targetPrefab, pos, Quaternion.LookRotation(normal));

        if (parent != null) effect.transform.SetParent(parent);

        effect.Play();

    }
}