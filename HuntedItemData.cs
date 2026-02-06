using UnityEngine;

/// <summary>
/// UV Light 색 enum
/// 
/// 규칙:
///   일반물품: UV 비추면 UV의 색 그대로 반사 (SpotLight 색)
///   귀품:     UV 비추면 귀품의 고유 반응색이 표시됨 (UV 색과 다른 색!)
/// 
/// 예:
///   UV Red를 비춤
///   → 일반물품: 빨간색으로 보임
///   → 귀품(반응색 Yellow): 노란색으로 보임!
/// </summary>
public enum UVColor
{
    None = 0,  // 반응색 없음 (UV에 반응 안 하는 귀품)
    Red = 1,  // 빨강
    Yellow = 2,  // 노랑
    Blue = 3   // 파랑
}

/// <summary>
/// 귀품 특성 데이터
/// 
/// 각 귀품은 아래 속성들의 조합으로 검수기별 반응을 결정.
/// 귀품이라도 모든 검수기에 반응하지 않을 수 있음.
/// 
/// "확정귀품": Inspector에서 isHaunted 체크, 반응 고정
/// "일반물품(미지수)": 스폰 시 랜덤하게 귀품 특성 부여 (매번 다름)
/// </summary>
[System.Serializable]
public class HauntedItemData
{
    // ── 귀품 여부 ──
    [Tooltip("귀품이면 체크. false이면 아래 전부 무시")]
    public bool isHaunted = false;

    [Tooltip("확정귀품이면 체크. false이면 스폰 시 랜덤 부여 (일반물품 미지수)")]
    public bool isFixedHaunted = false;

    // ── 각 검수기별 반응 여부 ──
    [Tooltip("Flashlight: 그림자 안 생김 (true면 그림자 없음)")]
    public bool noShadow = false;

    [Tooltip("Candle: 횃불이 흔들림")]
    public bool candleFlicker = false;

    [Tooltip("Salt: 연기 발생")]
    public bool saltSmoke = false;

    [Tooltip("Cross: 형태 왜곡")]
    public bool crossGlitch = false;

    [Tooltip("Spirit Box: 귀신 소리 트리거")]
    public bool triggersSpiritBox = false;

    [Tooltip("Spirit Box: 귀신 소리 (커스텀 AudioClip)")]
    public AudioClip spiritBoxSound;

    // ── UV Light 반응색 ──
    [Tooltip("UV 반응색. None = UV에 반응 없음")]
    public UVColor uvReactionColor = UVColor.None;

    /// <summary>
    /// 랜덤 귀품 특성 부여 (일반물품용)
    /// 50% 확률로 귀품, 각 검수기 반응도 랜덤 
    /// </summary>
    public void RandomizeHaunted()
    {
        if (isFixedHaunted) return; // 확정귀품은 건드리지 않음

        // 50% 확률로 귀품
        isHaunted = Random.value > 0.5f;

        if (!isHaunted)
        {
            // 일반물품이면 모두 false
            noShadow = false;
            candleFlicker = false;
            saltSmoke = false;
            crossGlitch = false;
            triggersSpiritBox = false;
            uvReactionColor = UVColor.None;
        }
        else
        {
            // 귀품이면 각 반응 랜덤 (30% 확률)
            noShadow = Random.value > 0.7f;
            candleFlicker = Random.value > 0.7f;
            saltSmoke = Random.value > 0.7f;
            crossGlitch = Random.value > 0.7f;
            triggersSpiritBox = Random.value > 0.7f;

            // UV 반응색 (None 포함 4가지 중 랜덤)
            int uvRandom = Random.Range(0, 4);
            uvReactionColor = (UVColor)uvRandom;
        }
    }
}