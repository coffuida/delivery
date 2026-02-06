using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    void Start()
    {
        // 메뉴 화면에서는 마우스 커서가 자유롭게 움직여야 합니다.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // 버튼 클릭 시 실행될 함수
    public void ClickStartGame()
    {
        // "PlayMap"은 실제 게임 맵 씬의 이름이어야 합니다.
        SceneManager.LoadScene("inGame");
    }

    public void ClickQuitGame()
    {
        Application.Quit();
        Debug.Log("게임 종료");
    }
}