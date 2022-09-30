using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController characterController; //CharacterController 컴포넌트는 자동으로 중력을 받아 아래로 떨어지지 않음(RigidBody를 사용하지 않고 캐릭터를 움직이게하기)
    private PlayerInput playerInput;
    private Animator animator;
    
    private Camera followCam;
    
    public float speed = 6f;
    public float jumpVelocity = 8f;
    [Range(0.01f, 1f)] public float airControlPercent; //공중에 체류하는동안 플레이어가 원래속도의 몇퍼센트를 통제하는지를 설정

    public float speedSmoothTime = 0.1f; //플레이어의 움직임의 속도값 변화를 부드럽게 해주는 지연값
    public float turnSmoothTime = 0.1f; //움직이려는 방향의 회전 속도값의 변화를 부드럽게 해주는 지연값

    private float speedSmoothVelocity; //이동값의 변화 속도
    private float turnSmoothVelocity; //회전값의 변화 속도
    
    private float currentVelocityY;
    
    public float currentSpeed =>
        new Vector2(characterController.velocity.x, characterController.velocity.z).magnitude; //지면상의 현재 속도
    
    private void Start()
    {
        playerInput = GetComponent<PlayerInput>(); //입력값 가져오기
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>(); //실제로 움직임을 적용할 컴포넌트
        followCam = Camera.main;
    }

    private void FixedUpdate() //물리 갱신 주기
    {
        if (currentSpeed > 0.2f || playerInput.fire) Rotate(); //플레이어 회전
        //currentSpeed > 0.2f는 아예 움직이지 않을때는 카메라를 돌려 캐릭터 구경할 수 있지만 조금이라도 움직이는 입력값이 들어오면 바로 캐릭터가 카메라가보는 정면으로 회전
        //playerInput.fire는 무기 발사 시 플레이어가 플레이어 카메라가 바라보는 방향으로 회전
        Move(playerInput.moveInput); //플레이어 움직임
        
        if (playerInput.jump) Jump();
    }

    private void Update() // 물리적으로 정확한 값을 수치로 연산할때는 오차가 발생하는 문제가 생김
    {
        UpdateAnimation(playerInput.moveInput);
    }

    public void Move(Vector2 moveInput) //실제 움직이는 값
    {
        var targetSpeed = speed * moveInput.magnitude; //가고싶은 속도 magnitude를 통해 움직임을 살짝 흘린 경우 1보다 작은값이 들어올수가 있다. 그런경우 최대 속도보다 작은값을 사용된다.(즐 살짝 눌렀을 때 살짝 움직인다는것)
        var moveDirection = Vector3.Normalize(transform.forward * moveInput.y + transform.right * moveInput.x); //이동방향(앞/옆만 계산)
        //moveInput미리 노멀라이즈되어 있음 
        //moveDirection은 방향값으로 사용하는것이고 Vector3.Normalize를하는 이유는 값이 1이 아닌 경우가 발생할 수도 있으므로 적용

        var smothTime = characterController.isGrounded ? speedSmoothTime : speedSmoothTime / airControlPercent; //현재 속도에서 타겟속도로 부드럽게 변환하는값
        //바닥에 존재하는지 확인하고 speedSmoothTime를 사용하고 airControlPercent는 1보다 같거나 작은값(0.01~1)인데
        //1보다 작은값으로 어떤값을 나누게되면 값이 커지게됨
        //바닥에 닿아있지 않을 경우 speedSmoothTime을 늘려 변환하는 지연시간이 늘어나고(targetSpeed에 도달하는 시간이 길어졌다는말) 그만큼 키가 안먹히게된다.
        //그래서 공중에 있을 때는 움직임 키를 둔하게 만드는것

        targetSpeed = Mathf.SmoothDamp(currentSpeed,targetSpeed, ref speedSmoothVelocity, smothTime); //원래값(currentSpeed)에서 목표값(targetSpeed)으로 변화하는 직전까지의 값에 지연시간을 적용해 부드럽게 이어지도록
        currentVelocityY += Time.deltaTime * Physics.gravity.y; //중력에 의한 바닥에 떨어지는 속도
        //가속도는 시간당 속도가 늘어나는 정도
        //Physics.gravity.y(기본값 -9.8)는 중력가속도에 Time.deltaTime시간을 곱해서 원래 속도에 더해줌
        var velocity = moveDirection * targetSpeed + Vector3.up * currentVelocityY; //속도를 두가지로 나누어 앞/옆 그리고 위로 따로 계산하여 마지막에 합친것

        characterController.Move(velocity * Time.deltaTime);//월드스페이스 기준으로 현재위치에서 얼만큼 더 이동할지를 입력하는것
        //여기서 Time.deltaTime은 Move(Vector2 moveInput)함수가 FixedUpdate에서 호출되기에 자동으로 Time.Fixeddeltatime으로 변환되어진다

        if(characterController.isGrounded) //플레이어가 바닥에 닿아 있다면 떨어지는 속도값을 0으로 초기화 시켜줘야함 해주지 않으면 속도가 -로 계속 커질경우 바닥이 존재해도 뚫고 아래로 떨어짐
        {                                   //characterController.isGrounded는 characterController에서 제공하는 바닥에 닿아있는지 자동으로 알려주는 Bool(불리언)값
            currentVelocityY = 0f;
        }
    }

    public void Rotate()
    {
        var targetRotation = followCam.transform.eulerAngles.y;

        targetRotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothVelocity, turnSmoothTime);
        //Mathf.SmoothDampAngle(현재각도, 타겟각도, 변화속도, 지연시간)
        //SmoothDampAngle은 SmoothDamp처럼 동작을 하지만 각도의 범위를 고려해 Damping이 이루어짐
        //오일러각은 결과적으론 같은각을 표현하지만 +와 -로 다르게 표현이될때가 있다.
        //예로 30->-270도로 회전할 경우 결과적으론 90도로 회전하는것이지만
        //SmoothDamp은 300도의 변화가 생기게 된다.SmoothDampAngle은 이것을 방지하 제대로 동작되게한다.

        transform.eulerAngles = Vector3.up * targetRotation;
    }

    public void Jump()
    {
        if (!characterController.isGrounded) return; //닿아있지 않다면
        currentVelocityY = jumpVelocity; //닿아있다면 Y방향의 속도를 점프속도로 변화해주어 설정
        //매 FixedUpdate에서 Move()함수를 불러 currentVelocityY += Time.deltaTime * Physics.gravity.y;가 커지게되어 위로 캐릭터가 점프
    }

    private void UpdateAnimation(Vector2 moveInput) //입력값에 따라 현재 플레이어 애니메이션 갱신
    {
        var animationSpeedPercent = currentSpeed / speed; //현재속도가 최고속도 대비 몇퍼센트인지 계산
        //애니메이터에 무브 파라미터값 전달
        animator.SetFloat("Vertical Move", moveInput.y * animationSpeedPercent, 0.05f, Time.deltaTime);
        animator.SetFloat("Horizontal Move", moveInput.x * animationSpeedPercent, 0.05f, Time.deltaTime); //Set에서 deltaTime 파라미터 : 마지막으로 Set메서드를 실행한 시간과 지금 실행한 시간 사이의 시간간격, DampTime과 Time.delta를 통해 애니메이터의 파라미터 값을 이전값에서 현재값으로 부드럽게 변환
        //moveInput.x와 y의 값을 플레이어 캐릭터의 실제속도가 최고속도 대비 몇퍼센트인가를 통해서 애니메이터의 SetFloat에 얼마나 전달할 것인가를 결정
        //예를 들어 앞쪽 버튼을 눌러 moveInput.y값이 1이라고 가정했을 때 이상태로 플레이어가 벽에 닿아 있다면 더이상 움직일 수 없기 때문에 실제 속도는 0이기에
        //이때는 최고속도 대비 실제 속도가 0%기에 moveInput.y 값이 그대로 1로 들어가는게 아니라 0% 들어가게되고 애니메이션도 앞쪽 입력값이 들어와도 실제로는 0%가 들어오기에 동작안하게됨
        //최고속도 대비 이제 막 움직여 실제 속도가50%일 경우 0.5가 곱해져 앞쪽으로 뛰는 애니메이션이 상대적으로 적게 블랜딩됨
    }
}