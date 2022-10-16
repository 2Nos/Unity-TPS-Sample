using UnityEngine;


public class PlayerShooter : MonoBehaviour
{
    public enum AimState //TPS는 3가지상태가 존재 가만히, 정조준하며, 조준없이
    {
        Idle,
        HipFire //조준없이 발사(간결을 위해 2개만)
    }

    public AimState aimState { get; private set; }

    public Gun gun;
    public LayerMask excludeTarget; //조준에서 제외 레이어
    
    private PlayerInput playerInput;
    private Animator playerAnimator;
    private Camera playerCamera;

    private float waitingTimeForReleasingAim = 2.5f; //일정시간동안 발사가 없으면 Idle로 되돌아오게(shoot함수에서 if(lineUp)구간으로 만든상태를 다시 되돌려해서 만듬(계속 총쏘는 상태))
    private float lastFireInputTime; //마지막 발사된 시간

    private Vector3 aimPoint; //실제 조준하고 있는 대상 FPS는 필요가 없음 FPS는 그냥 정중앙으로 조준
                              //TPS의 문제점으로 상황에 따라 예를 들어 캐릭터의 앞에 벽이 있어 중앙을 바라볼때 가로막히게되어 실제 총을 쏘는 위치인 캐릭터쪽의  Raycast는 막혀있지만
                              //TPS 카메라의 에임에는 막혀있지 않기에 벽넘어에 있는 대상을 맞춰야되기에 실제로 맞게될 곳(벽)을 aimPoint에 저장해 좀 더 현실감을 더해줌(조준점을 두개사용)
    private bool linedUp => !(Mathf.Abs( playerCamera.transform.eulerAngles.y - transform.eulerAngles.y) > 1f); //플레이어가 바라보는 방향과 카메라가 바라보는 방향이 너무 벌어지지 않았는지를 bool로 리턴해주는 프로퍼티
                                                                                                                //플레이어 가만있는 상태에서 카메라 회전했을 때 발사 버튼누르면 바로 발사가 안되고 캐릭터를 카메라 바라보는 방향으로 정렬함
                                                                                                                //플레이어카메라 y축회전과 플레이어 y축회전값의 차이가 절대이 1(1도)보다 클경우 false
    private bool hasEnoughDistance => !Physics.Linecast(transform.position + Vector3.up * gun.fireTransform.position.y,gun.fireTransform.position, ~excludeTarget); //플레이어의 정면에 총을 발사할 수 있을정도의 공간이 있는지 반환하는 프로퍼티
                                                                                                                                                                    //총구가 벽을 뚫었을 경우는 총 발사x
                                                                                                                                                                    //계산으로는 캐릭터 pivot위치(발)에서부터 총까지의 y값과 총발사위치 사이에 콜라이더가 존재한다면 false로 반환(즉 공간이 확보안되었단얘기) 
    
    void Awake()
    {
        if (excludeTarget != (excludeTarget | (1 << gameObject.layer))) //비트연산을 통해 플레이어 캐릭터의 레이어가 excludeTarget에 포함되어있지 않다면 포함되게 만듬
        {                                                               //플레이어가 자신을 쏘는 일이 없도록 미리 예외 처리
            excludeTarget |= 1 << gameObject.layer;
        }
    }

    private void Start()
    {
        playerCamera = Camera.main;
        playerInput = GetComponent<PlayerInput>();
        playerAnimator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        aimState = AimState.Idle;
        gun.gameObject.SetActive(true); //gun스크립트를 활성화 시킴
        gun.Setup(this); //자기자신을 넣어 초기화
    }

    private void OnDisable()
    {
        aimState = AimState.Idle;
        gun.gameObject.SetActive(false); //총쏘는 기능이 비활성화일때마다 gun스크립트도 비활성화
    }

    private void FixedUpdate()
    {
        if (playerInput.fire)
        {
            lastFireInputTime = Time.time;
            Shoot();
        }
        else if (playerInput.reload)
        {
            Reload();
        }
    }

    private void Update()
    {
        UpdateAimTarget();

        var angle = playerCamera.transform.eulerAngles.x; //바닥은 0 중앙 0.5 위는 1로 만듬(+90 0 -90)
        if(angle > 270f) //-270나 270의 경우 90도이기에 그런 값들이 들어올 수도 있기에 변환
        {
            angle -= 360f;
        }
        angle = angle / 180f * -1f + 0.5f; //-90 0 90을 -1 0 1로 만듬 여기서 -180은 위 아래값이 반대로 되어 있기에 변경
        playerAnimator.SetFloat("Angle", angle);

        if (!playerInput.fire && Time.time >= lastFireInputTime + waitingTimeForReleasingAim) //발사하고 있을때는 lastFireInputTime + waitingTimeForReleasingAim이 Time.time보단 높고 멈췄을땐 waitingTimeForReleasingAim만큼 Time.time이 흐르면 lastFireInputTime + waitingTimeForReleasingAim값이 Time.time보다 작음
        {
            aimState = AimState.Idle;
        }

        UpdateUI();
    }

    public void Shoot()
    {
         if(aimState == AimState.Idle) //가만히 있는 상태면
         {
             if(linedUp) //발사할 방향으로 정렬이 되어 있으면
             {
                 aimState = AimState.HipFire;
             }
         }
         else if (aimState == AimState.HipFire) //발사 준비가 이미 되었거나 발사중인 상태면
         {
             if (hasEnoughDistance) //공간확보 되었는지
             {
                
                 if (gun.Fire(aimPoint)) //gun.Fire()는 발사에 성공했는지 여부를 리턴하는 함수. aimPoint는 발사할 위치.
                 {
                     playerAnimator.SetTrigger("Shoot");
                 }
             }
             else
             {
                 aimState = AimState.Idle;
             }
         }
    }

    public void Reload()
    {
        if(gun.Reload())
        {
            playerAnimator.SetTrigger("Reload");
        }
    }

    private void UpdateAimTarget()
    {
        RaycastHit hit;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 1f)); //카메라 정중앙으로

        if(Physics.Raycast(ray, out hit, gun.fireDistance, ~excludeTarget))
        {
            aimPoint = hit.point; 

            if(Physics.Linecast(gun.fireTransform.position, hit.point, out hit, ~excludeTarget)) //캐릭터가 바라보는 방향에서 카메라의 정중앙 사이 장애물이 있을경우 장애물쪽 히트포인트 사용
            {
                aimPoint = hit.point;
            }
        }
        else
        {
            aimPoint = playerCamera.transform.position + playerCamera.transform.forward * gun.fireDistance;
        }
    }

    private void UpdateUI() //남은 탄약 UI와 조준점 UI갱신(강사가 UIManager 미리 만들어놓음)
    {
        if (gun == null || UIManager.Instance == null) return; //싱글턴
        
        UIManager.Instance.UpdateAmmoText(gun.magAmmo, gun.ammoRemain); //탄약 UI 갱신
        
        UIManager.Instance.SetActiveCrosshair(hasEnoughDistance); //조준점 켜고 끄는 UI
        UIManager.Instance.UpdateCrossHairPosition(aimPoint); //조준점 위치 갱신
    }

    private void OnAnimatorIK(int layerIndex) //왼손을 총 왼손위치에
    {
        if (gun == null || gun.state == Gun.State.Reloading) return;

        playerAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1.0f);
        playerAnimator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1.0f);

        playerAnimator.SetIKPosition(AvatarIKGoal.LeftHand, gun.leftHandMount.position);
        playerAnimator.SetIKRotation(AvatarIKGoal.LeftHand, gun.leftHandMount.rotation);
    }
}