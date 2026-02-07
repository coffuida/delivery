using UnityEngine;

/// <summary>
/// 배경음악 관리자
/// inGame Scene 시작 시 배경음악 자동 재생
/// </summary>
public class BackgroundMusicManager : MonoBehaviour
{
    [Header("배경음악")]
    public AudioClip deliveryBackgroundSound;

    [Header("설정")]
    [Range(0f, 1f)]
    public float volume = 0.5f;
    public bool loop = true;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = loop;
        audioSource.volume = volume;
    }

    void Start()
    {
        PlayBackgroundMusic();
    }

    void PlayBackgroundMusic()
    {
        if (deliveryBackgroundSound != null)
        {
            audioSource.clip = deliveryBackgroundSound;
            audioSource.Play();
            Debug.Log("[배경음악] DeliverybackgroundSound 재생");
        }
        else
        {
            Debug.LogWarning("[배경음악] deliveryBackgroundSound가 없습니다!");
        }
    }

    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        audioSource.volume = volume;
    }
}
