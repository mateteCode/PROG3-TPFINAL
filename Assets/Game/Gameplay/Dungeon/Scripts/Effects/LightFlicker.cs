using UnityEngine;

public class LightFlicker : MonoBehaviour
{
  [Header("Tiempo")]
  public float minDelay = 0.05f;
  public float maxDelay = 0.3f;

  [Header("Intensidad")]
  public float minIntensity = 0.3f;
  public float maxIntensity = 1f;

  private Light _light;
  private float _timer;

  void Start()
  {
    _light = GetComponent<Light>();
    SetRandomTimer();
  }

  void Update()
  {
    _timer -= Time.deltaTime;

    if (_timer <= 0)
    {
      // Cambia intensidad y encendido/apagado aleatoriamente
      _light.intensity = Random.Range(minIntensity, maxIntensity);
      _light.enabled = Random.value > 0.1f; // 10% de probabilidad de apagarse

      SetRandomTimer();
    }
  }

  void SetRandomTimer()
  {
    _timer = Random.Range(minDelay, maxDelay);
  }
}