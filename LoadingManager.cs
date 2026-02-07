using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// 로딩 화면 관리자
/// 
/// 기능:
/// - 씬 전환 시 로딩 화면 표시
/// - 비동기 씬 로드
/// - 로딩 진행률 표시
/// </summary>
public class LoadingManager : MonoBehaviour
{
    private static string nextSceneName;

    /// <summary>
    /// 씬 로드 (외부에서 호출)
    /// </summary>
    public static void LoadScene(string sceneName)
    {
        nextSceneName = sceneName;
        SceneManager.LoadScene("LoadingScene"); // 로딩 씬으로 먼저 이동
    }

    void Start()
    {
        // 로딩 씬에서 실제 씬 로드 시작
        StartCoroutine(LoadSceneAsync());
    }

    IEnumerator LoadSceneAsync()
    {
        // 최소 로딩 시간 (너무 빠르면 로딩 화면이 깜빡임)
        yield return new WaitForSeconds(0.5f);

        // 비동기 씬 로드 시작
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextSceneName);
        asyncLoad.allowSceneActivation = false; // 로딩 완료 후 자동 전환 방지

        // 로딩 진행률 업데이트
        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);

            // UI 업데이트 (LoadingUI가 있다면)
            LoadingUI loadingUI = FindObjectOfType<LoadingUI>();
            if (loadingUI != null)
            {
                loadingUI.UpdateProgress(progress);
            }

            // 로딩 완료 (90% = 완료 대기 상태)
            if (asyncLoad.progress >= 0.9f)
            {
                // 추가 대기 시간 (선택적)
                yield return new WaitForSeconds(0.3f);

                // 씬 전환
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}