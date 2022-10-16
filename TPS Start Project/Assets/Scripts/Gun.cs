using System;
using System.Collections;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public enum State
    {
        Ready, //총발사 준비 상태
        Empty, //총알 빈 상태
        Reloading //재장전 상태
    }
    public State state { get; private set; }
    
    private PlayerShooter gunHolder; //총의 주인이 누구인지 알려주는 클래스
    private LineRenderer bulletLineRenderer; //총알궤적
    //사운드
    private AudioSource gunAudioPlayer;
    public AudioClip shotClip;
    public AudioClip reloadClip;
    //파티클 효과
    public ParticleSystem muzzleFlashEffect;
    public ParticleSystem shellEjectEffect;
    
    public Transform fireTransform; //총알발사 위치
    public Transform leftHandMount; //왼손위치

    public float damage = 25; //데미지
    public float fireDistance = 100f; //발사거리

    public int ammoRemain = 100; //현재 가지고 있는 탄약수
    public int magAmmo; //현재 탄창에 있는 총알 수
    public int magCapacity = 30; //탄창의 넣을 수 있는 탄약 수

    public float timeBetFire = 0.12f; //총발사 간격시간
    public float reloadTime = 1.8f; //재장전시간

    [Range(0f, 10f)] public float maxSpread = 3f;//탄 퍼짐 간격
    [Range(1f, 10f)] public float stability = 1f; //반동 속도(낮추면 탄퍼지는정도가 빠르게 증가)
    [Range(0.01f, 3f)] public float restoreFromRecoilSpeed = 2f; //탄퍼짐 돌아오는속도
    private float currentSpread; //현재 탄퍼짐의 정도값
    private float currentSpreadVelocity; //탄퍼짐 실시간 변화량

    private float lastFireTime; //가장 최근 발사가 이루어진 시점

    private LayerMask excludeTarget; //총알을 쏴서는안되는 대상을 거르기위한 레이어마스크

    private void Awake()
    {
        // 사용할 컴포넌트들의 참조를 가져오기
        gunAudioPlayer = GetComponent<AudioSource>();
        bulletLineRenderer = GetComponent<LineRenderer>();

        bulletLineRenderer.positionCount = 2; //점의 개수를 2개로 할당하여 처음점은 총구의 위치 두번째 점은 총알이 맞은 위치
        bulletLineRenderer.enabled = false;

    }

    public void Setup(PlayerShooter gunHolder) //총에게 총의 주인이 누구인지 알려주는 역할
    {
        this.gunHolder = gunHolder; //입력받은 gunHolder를 자기자신의 gunHolder에 할당하여
        excludeTarget = gunHolder.excludeTarget; //총의 주인이 쏘지 않기로한 레이어를 할당
    }

    private void OnEnable()
    {
        currentSpread = 0f; // 탄 퍼짐
        magAmmo = magCapacity; //현재 탄창에 남아있는 탄의 수 최대로 초기화
        state = State.Ready;
        lastFireTime = 0f;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public bool Fire(Vector3 aimTarget)
    {

        if(state == State.Ready && Time.time >= lastFireTime + timeBetFire) //발사가능한 상태이고 현재 시간이 마지막발사 시점에서 발사간격을 더한것보다 더 많은 시간이 흘렀을 경우
        {                                                                   //발사가능 
            //현재 여기가 문제되는상황
            var xError = Utility.GedRandomNormalDistribution(0f, currentSpread); //정규분포에 의한 오차 x방향으로 오차(기준, 정규분포평평하게만드는정도)
            var yError = Utility.GedRandomNormalDistribution(0f, currentSpread);

            var fireDirection = aimTarget - fireTransform.position;  //타겟 - 총구 = 발사 방향

            fireDirection = Quaternion.AngleAxis(yError, Vector3.up) * fireDirection; //총이 향하던 방향 움직이기(회전할 정도, 회전 축)
                                                                                      //y축 기준으로 fireDirection만큼에서 yError만큼 조금 회전

            fireDirection = Quaternion.AngleAxis(xError, Vector3.right) * fireDirection;

            currentSpread += 1f / stability; //반동 만들어냄 (stability높을수록 더해지는 값이 낮아져 반동이 줄어듬)
            lastFireTime = Time.time; //마지막 총발사 시점 현재시간으로 초기화
            Shot(fireTransform.position, fireDirection);
            //가오시안 분포(정규분포) 랜덤을 사용하여 탄퍼짐 효과 제작
            //정규분포는 어떤 값이 출현할 확률의 분포도. 값의 범위중 평균에 가까워질수록 확률이 높고 평균에 멀어질수록 확률이 낮음
            //중간에 값들이 몰려서 볼록하게 올라와있는 그래프 모양
            return true;
        }
        return false;
    }
    
    private void Shot(Vector3 startPoint, Vector3 direction) //시작지점과 발사 방향
    {
        RaycastHit hit;
        Vector3 hitPostion;

        if(Physics.Raycast(startPoint, direction, out hit, fireDistance, ~excludeTarget)) //~는 반대 즉 excludeTarget를 제외한
        {
            var target = hit.collider.GetComponent<IDamageable>(); //IDamageable메세지를 받아왔다면 데미지를 줄 수 있는 대상이라는것

            if(target != null) //초기화
            {
                DamageMessage damageMessage;

                damageMessage.damager = gunHolder.gameObject;
                damageMessage.amount = damage;
                damageMessage.hitPoint = hit.point;
                damageMessage.hitNormal = hit.normal; //충돌한 포지션의 노말벡터

                // 상대방의 OnDamage 함수를 실행시켜서 상대방에게 데미지 주기
                target.ApplyDamage(damageMessage);
            }
            else
            {
                //EffectManager.Instance.PlayHitEffect(hit.point, hit.normal, hit.transform);
            }
            hitPostion = hit.point;
        }
        else
        {
            hitPostion = startPoint + direction * fireDistance; //RayCast충돌이 일어나지 않았을 경우 총알이 최대 사정거리까지 이동한 위치를 hitPostion으로 사용
        }

        StartCoroutine(ShotEffect(hitPostion));

        magAmmo--; //탄창의 남은 탄약 1빼기
        if(magAmmo <= 0) //탄창에 탄이 없다면
        {
            state = State.Empty;
        }
    }

    private IEnumerator ShotEffect(Vector3 hitPosition) //번쩍이는 효과
    {
        muzzleFlashEffect.Play();
        shellEjectEffect.Play();

        gunAudioPlayer.PlayOneShot(shotClip); //소리를 연달아 재생하는경우 Play()보단 PlayOneShot()을 사용
        //Play()는 플레이를 하기전 오디오소스 클립에 사용할 클립을 할당해야하며
        //gunAudioPlayer.clip = shotClip; 이렇게
        //gunAudioPlayer.Play(); 그다음 재생, Play()가 실행될땐 직전까지 진행중이던 소리를 정지하고 재생하기에 총처럼 연달아 내야하는 소리에는 부자연스러움(소리가 중첩이 안된다.)

        bulletLineRenderer.SetPosition(0,fireTransform.position); //라인랜더러 사용(인덱스, 위치값)
        bulletLineRenderer.SetPosition(1, hitPosition); //맞은 위치
        bulletLineRenderer.enabled = true;
        yield return new WaitForSeconds(0.03f); 
        //0.03초 텀 이유는 bulletLineRenderer.enabled가 활성화 된다음 바로 비활성화되서 궤적이 아예 그려지지않음

        bulletLineRenderer.enabled = false; 
    }
    
    public bool Reload()
    {
        if(state == State.Reloading || ammoRemain <= 0 || magAmmo >= magCapacity) //재장전할 수 없는 상태일 경우
        { 
            return false;
        }
        // 재장전 처리 시작
        StartCoroutine(ReloadRoutine());
        return true; //그외에는 true
    }

    private IEnumerator ReloadRoutine()
    {
        state = State.Reloading; //현재상태를 재장전 상태로 초기화 재장전도중 발사하거나 재장전중 다시 재장전하는 상황을 막기위해
        gunAudioPlayer.PlayOneShot(reloadClip);
        yield return new WaitForSeconds(reloadTime);
                                                                            //ammoToFill = magCapacity - magAmmo가 남아있는 탄의 개수보다 높으면 안되기에 Mathf.Clamp를 사용하여 그만큼 수를 잘라줌
        var ammoToFill = Mathf.Clamp(magCapacity - magAmmo, 0, ammoRemain); //채울 탄약 개수, (탄창의 최대 개수 - 현재 탄창 탄 개수, 0에서, 현재 남은 탄약 수)
        magAmmo += ammoToFill;                                                      
        ammoRemain -= ammoToFill;

        state = State.Ready;
    }

    private void Update()
    {
        currentSpread = Mathf.Clamp(currentSpread, 0f, maxSpread); //currentSpread값이(반동이 누적이 되어도) maxSpread를 넘기지 못하도록 계속 잡아줌
        currentSpread 
            = Mathf.SmoothDamp(currentSpread, 0f, ref currentSpreadVelocity, 1f/restoreFromRecoilSpeed); //Fire함수와 별개로 매프레임마다 0에 가깝게 부드럽게 계속 만들어줌
    }
}