 using UnityEngine;

/// <summary>
/// 컨베이어 벨트 사운드 관리
/// ⭐ AudioSource 제어 가능 버전
/// </summary>
public class ConveyorBeltSound : MonoBehaviour
{
    [Header("⭐ Audio Source")]
    [Tooltip("사운드를 재생할 AudioSource (비워두면 자동 생성)")]
    public AudioSource audioSource;

    [Header("사운드")]
    public AudioClip conveyorBeltDropSound;

    [Header("설정")]
    [Range(0f, 1f)]
    [Tooltip("사운드 볼륨")]
    public float volume = 1f;

    [Tooltip("3D 사운드 사용 여부")]
    public bool use3DSound = false;

    [Range(0f, 100f)]
    [Tooltip("3D 사운드 최소 거리")]
    public float minDistance = 1f;

    [Range(0f, 500f)]
    [Tooltip("3D 사운드 최대 거리")]
    public float maxDistance = 50f;

    void Awake()
    {
        // AudioSource가 없으면 자동 생성
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();

            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                Debug.Log("[컨베이어벨트] AudioSource 자동 생성");
            }
        }

        // AudioSource 설정
        ConfigureAudioSource();
    }

    void Start()
    {
        // AudioSource 최종 확인
        if (audioSource == null)
        {
            Debug.LogError("[컨베이어벨트] AudioSource가 없습니다!");
        }
    }

    /// <summary>
    /// ⭐ AudioSource 설정
    /// </summary>
    void ConfigureAudioSource()
    {
        if (audioSource == null) return;

        audioSource.playOnAwake = false;
        audioSource.volume = volume;

        // 2D 또는 3D 사운드 설정
        if (use3DSound)
        {
            audioSource.spatialBlend = 1f; // 3D
            audioSource.minDistance = minDistance;
            audioSource.maxDistance = maxDistance;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
        }
        else
        {
            audioSource.spatialBlend = 0f; // 2D
        }

        Debug.Log($"[컨베이어벨트] AudioSource 설정 완료 (3D: {use3DSound})");
    }

    void OnCollisionEnter(Collision collision)
    {
        // 물품이 벨트에 닿았는지 확인
        Item item = collision.gameObject.GetComponent<Item>();
        if (item != null)
        {
            PlayDropSound();
            Debug.Log($"[컨베이어벨트] {item.itemName} 착지 사운드 재생");
        }
    }

    /// <summary>
    /// Drop 사운드 재생
    /// </summary>
    void PlayDropSound()
    {
        if (audioSource != null && conveyorBeltDropSound != null)
        {
            audioSource.PlayOneShot(conveyorBeltDropSound, volume);
        }
        else if (conveyorBeltDropSound == null)
        {
            Debug.LogWarning("[컨베이어벨트] conveyorBeltDropSound가 없습니다!");
        }
        else if (audioSource == null)
        {
            Debug.LogError("[컨베이어벨트] audioSource가 없습니다!");
        }
    }

    /// <summary>
    /// 볼륨 조정 (외부 호출용)
    /// </summary>
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }

    /// <summary>
    /// 3D 사운드로 변경
    /// </summary>
    public void Enable3DSound(bool enable)
    {
        use3DSound = enable;
        ConfigureAudioSource();
    }
}