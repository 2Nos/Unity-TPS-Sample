//UIManager는 게임에 직접적인 영향을 주는것이 아닌 다른 스크립트에서 쉽게 접근할 수 있도록 만드는 역할
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private static UIManager instance; //싱글톤을 통해서 다른 스크립트에서 UI에 접근이 용이하도록 구현
    
    public static UIManager Instance
    {
        get
        {
            if (instance == null) instance = FindObjectOfType<UIManager>();

            return instance;
        }
    }

    [SerializeField] private GameObject gameoverUI;
    [SerializeField] private Crosshair crosshair; //조준점

    [SerializeField] private Text healthText;
    [SerializeField] private Text lifeText;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text ammoText; //탄약표시
    [SerializeField] private Text waveText; //남은 적의 수

    public void UpdateAmmoText(int magAmmo, int remainAmmo) //현재 탄약과 총 탄약
    {
        ammoText.text = magAmmo + "/" + remainAmmo;
    }

    public void UpdateScoreText(int newScore) //점수
    {
        scoreText.text = "Score : " + newScore;
    }
    
    public void UpdateWaveText(int waves, int count) //현재 웨이브와 남은 적수
    {
        waveText.text = "Wave : " + waves + "\nEnemy Left : " + count;
    }

    public void UpdateLifeText(int count) //남은 목숨
    {
        lifeText.text = "Life : " + count;
    }

    public void UpdateCrossHairPosition(Vector3 worldPosition) //크로스헤어 조준점을 해당 위치를 표시하는 위치로 이동
    {
        crosshair.UpdatePosition(worldPosition);
    }
    
    public void UpdateHealthText(float health) //체력
    {
        healthText.text = Mathf.Floor(health).ToString();
    }
    
    public void SetActiveCrosshair(bool active) //조준점 활성화 여부
    {
        crosshair.SetActiveCrosshair(active);
    }
    
    public void SetActiveGameoverUI(bool active) //게임오버창 활성화 비활성화
    {
        gameoverUI.SetActive(active);
    }
    
    public void GameRestart() //현재씬을 재시작
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}