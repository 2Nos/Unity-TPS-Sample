using UnityEngine;
using UnityEngine.UI;

public class Crosshair : MonoBehaviour
{
    public Image aimPointReticle; //에임 조준점
    public Image hitPointReticle; //실제 맞는 조준점

    public float smoothTime = 0.2f;
    
    private Camera screenCamera;
    private RectTransform crossHairRectTransform; //hitPointReticle를 받아와

    private Vector2 currentHitPointVelocity;
    private Vector2 targetPoint;

    private void Awake()
    {
        screenCamera = Camera.main;
        crossHairRectTransform = hitPointReticle.GetComponent<RectTransform>();
    }

    public void SetActiveCrosshair(bool active) //조준점 활성화 비활성화
    {
        hitPointReticle.enabled = active; //둘 모두 활성화 갱신
        aimPointReticle.enabled = active;
    }

    public void UpdatePosition(Vector3 worldPoint) //worldPoint를 받아 targetPoint로 변경하는 역할. 이 targetPoint값으로 crossHairRectTransform의 위치가 이동된다.
    {
        targetPoint = screenCamera.WorldToScreenPoint(worldPoint);
    }

    private void Update()
    {
        if (!hitPointReticle.enabled) return;
        crossHairRectTransform.position = Vector2.SmoothDamp(crossHairRectTransform.position, targetPoint, ref currentHitPointVelocity, smoothTime);
        //hitPointReticle활성화 된순간에만 hitPointReticle을 targetPoint로 옮기기
        //crossHairRectTransform은 스크린스페이스오버레이로 지정된 캔버스의 자식으로 들어있는 UI의 RectTransform이기 때문에 crossHairRectTransform.position 값은 Vector3지만 실제로는 스크린스페이스 즉,
        //화면상의 위치를 기준으로 잡혀있다. 따라서 worldPoint가 아니라 worldPoint를 화면상의 위치로 바꾼 targetPoint를 할당
    }
}