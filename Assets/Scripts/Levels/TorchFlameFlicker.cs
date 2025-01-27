using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FlickeringFlame : MonoBehaviour
{
    [Header("Flicker Settings")]
    public float FlickerSpeed = 0.15f;
    public float InnerRadiusMin = 3.5f;
    public float InnerRadiusMax = 4f;
    public float OuterRadiusMin = 6.5f;
    public float OuterRadiusMax = 7f;

    float _flickerTimer;

    [SerializeField] Light2D light2D;

    void Start()
    {
        SetNewFlickerTarget();
    }

    void Update()
    {
        FlickerLight();
    }

    void FlickerLight()
    {
        _flickerTimer += Time.deltaTime;
        if (_flickerTimer >= FlickerSpeed)
        {
            SetNewFlickerTarget();
            _flickerTimer = 0f;
        }
    }

    void SetNewFlickerTarget()
    {
        light2D.pointLightInnerRadius = Random.Range(InnerRadiusMin, InnerRadiusMax);
        light2D.pointLightOuterRadius = Random.Range(OuterRadiusMin, OuterRadiusMax);
    }
}
