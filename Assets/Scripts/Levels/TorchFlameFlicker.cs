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

    float _innerRadiusTarget;
    float _outerRadiusTarget;
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
        // light2D.pointLightInnerRadius = Mathf.Lerp(light2D.pointLightInnerRadius, _innerRadiusTarget, FlickerSpeed * Time.deltaTime);
        // light2D.pointLightOuterRadius = Mathf.Lerp(light2D.pointLightOuterRadius, _outerRadiusTarget, FlickerSpeed * Time.deltaTime);
        
        _flickerTimer += Time.deltaTime;
        if (_flickerTimer >= FlickerSpeed)
        {
            // Debug.Log("changed flicker");
            SetNewFlickerTarget();
            _flickerTimer = 0f;
        }
    }

    void SetNewFlickerTarget()
    {
        // _innerRadiusTarget = Random.Range(InnerRadiusMin, InnerRadiusMax);
        // _outerRadiusTarget = Random.Range(OuterRadiusMin, OuterRadiusMax);
        
        light2D.pointLightInnerRadius = Random.Range(InnerRadiusMin, InnerRadiusMax);
        light2D.pointLightOuterRadius = Random.Range(OuterRadiusMin, OuterRadiusMax);
      
        // Debug.Log($"INNER: {_innerRadiusTarget}");
        // Debug.Log($"OUTER: {_outerRadiusTarget}");
    }
}
