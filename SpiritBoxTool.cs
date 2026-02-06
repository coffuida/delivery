using UnityEngine;

/// <summary>
/// Spirit Box 검수기 (슬롯 4번)
/// 
/// 기능:
/// - 검수대 물품 자동 감지
/// - 일반물품: 일반 전파 소리
/// - 귀품: 지직거리는 소리 / 이상한 소리 (triggersSpiritBox = true)
/// </summary>
public class SpiritBoxTool : MonoBehaviour, IInspectionTool
{
    [Header("오디오 설정")]
    public AudioSource audioSource;
    public AudioClip normalRadioSound;     // 일반 전파 소리
    public AudioClip hauntedSound;         // 귀품 소리 (기본)

    [Header("물품 감지")]
    public Transform itemPlacementSlot;
    public float checkInterval = 1f;

    [Header("디버그")]
    public bool showDebugLog = true;

    // 내부 변수
    private bool isActive = false;
    private float checkTimer = 0f;

    void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource != null)
        {
            audioSource.loop = true;
            audioSource.playOnAwake = false;
        }
    }

    void Update()
    {
        if (!isActive) return;

        checkTimer += Time.deltaTime;
        if (checkTimer >= checkInterval)
        {
            checkTimer = 0f;
            CheckCurrentItem();
        }
    }

    public void Activate()
    {
        isActive = true;

        CheckCurrentItem();

        if (showDebugLog)
            Debug.Log("[Spirit Box] 활성화");
    }

    public void Deactivate()
    {
        isActive = false;

        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();

        if (showDebugLog)
            Debug.Log("[Spirit Box] 비활성화");
    }

    public void CheckItem(GameObject item)
    {
        if (item == null)
        {
            PlayNormalSound();
            return;
        }

        Item itemScript = item.GetComponent<Item>();
        if (itemScript == null)
        {
            PlayNormalSound();
            return;
        }

        // triggersSpiritBox = true면 귀품 소리
        if (itemScript.hauntedData.triggersSpiritBox)
        {
            // 커스텀 귀품 소리가 있으면 사용, 없으면 기본 귀품 소리
            AudioClip soundToPlay = itemScript.hauntedData.spiritBoxSound != null
                ? itemScript.hauntedData.spiritBoxSound
                : hauntedSound;

            PlayHauntedSound(soundToPlay);
        }
        else
        {
            PlayNormalSound();
        }
    }

    /// <summary>
    /// 현재 물품 체크
    /// </summary>
    void CheckCurrentItem()
    {
        if (itemPlacementSlot == null) return;

        if (itemPlacementSlot.childCount > 0)
        {
            GameObject item = itemPlacementSlot.GetChild(0).gameObject;
            CheckItem(item);
        }
        else
        {
            PlayNormalSound();
        }
    }

    /// <summary>
    /// 일반 전파 소리 재생
    /// </summary>
    void PlayNormalSound()
    {
        if (audioSource == null || normalRadioSound == null) return;

        if (audioSource.clip != normalRadioSound)
        {
            audioSource.clip = normalRadioSound;
            audioSource.Play();

            if (showDebugLog)
                Debug.Log("[Spirit Box] 일반 전파 소리");
        }
    }

    /// <summary>
    /// 귀품 소리 재생
    /// </summary>
    void PlayHauntedSound(AudioClip hauntedClip)
    {
        if (audioSource == null) return;

        if (hauntedClip == null)
        {
            PlayNormalSound();
            return;
        }

        if (audioSource.clip != hauntedClip)
        {
            audioSource.clip = hauntedClip;
            audioSource.Play();

            if (showDebugLog)
                Debug.Log("[Spirit Box] 귀품 소리 감지!");
        }
    }

    void OnDisable()
    {
        Deactivate();
    }
}