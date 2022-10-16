using UnityEngine;
using UnityEngine.UI;
public class PlayerInput : MonoBehaviour
{
    public string moveHorizontalAxisName = "Horizontal";
    public string moveVerticalAxisName = "Vertical";

    public string fireButtonName = "Fire1";
    public string jumpButtonName = "Jump";
    public string reloadButtonName = "Reload";


    public Vector2 moveInput { get; private set; }
    public bool fire { get; private set; }
    public bool reload { get; private set; }
    public bool jump { get; private set; }


    private void Update()
    {
        if (GameManager.Instance != null
            && GameManager.Instance.isGameover) //싱글톤과 함께 죽었을 경우 유저 입력을 전부 무시하게 만듬
        {
            moveInput = Vector2.zero;
            fire = false;
            reload = false;
            jump = false;
            return;
        }

        moveInput = new Vector2(Input.GetAxis(moveHorizontalAxisName), Input.GetAxis(moveVerticalAxisName));
        if (moveInput.sqrMagnitude > 1) moveInput = moveInput.normalized;

        jump = Input.GetButtonDown(jumpButtonName);
        fire = Input.GetButton(fireButtonName);
        reload = Input.GetButtonDown(reloadButtonName);
    }
}