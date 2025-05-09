using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class EfectoHeridaPostProcess : MonoBehaviour
{
  [Header("Referencias")]
  public Volume volume;
  public float duracionTransicion = 1.5f;

  [Header("Valores de Herida (Lift, Gamma, Gain)")]
  public Vector4 liftHerida = new Vector4(0.4f, -0.1f, -0.1f, 0f);
  public Vector4 gammaHerida = new Vector4(1.1f, 0.5f, 0.5f, 0f);
  public Vector4 gainHerida = new Vector4(1.6f, 0.9f, 0.9f, 0f);

  private Vector4 liftInicial;
  private Vector4 gammaInicial;
  private Vector4 gainInicial;

  private LiftGammaGain liftGammaGain;
  private bool estaHerido = false;

  void Start()
  {
    if (volume == null)
    {
      Debug.LogError("El Volume no está asignado en el Inspector.");
      enabled = false;
      return;
    }

    if (!volume.profile.TryGet(out liftGammaGain))
    {
      Debug.LogError("El perfil del Volume no tiene un efecto de Lift, Gamma, Gain (URP).");
      enabled = false;
      return;
    }

    liftInicial = liftGammaGain.lift.value;
    gammaInicial = liftGammaGain.gamma.value;
    gainInicial = liftGammaGain.gain.value;
  }

  public void ActivarEfectoHerida()
  {
    if (estaHerido) return;

    estaHerido = true;
    //volume.enabled = true;

    StartCoroutine(TransicionHerida());
  }

  private IEnumerator TransicionHerida()
  {
    float tiempoTranscurrido = 0f;

    while (tiempoTranscurrido < duracionTransicion)
    {
      tiempoTranscurrido += Time.deltaTime;
      float t = Mathf.Clamp01(tiempoTranscurrido / duracionTransicion);

      liftGammaGain.lift.Override(Vector4.Lerp(liftInicial, liftHerida, t));
      liftGammaGain.gamma.Override(Vector4.Lerp(gammaInicial, gammaHerida, t));
      liftGammaGain.gain.Override(Vector4.Lerp(gainInicial, gainHerida, t));

      yield return null;
    }

    StartCoroutine(TransicionNormal());
  }

  private IEnumerator TransicionNormal()
  {
    float tiempoTranscurrido = 0f;

    while (tiempoTranscurrido < duracionTransicion)
    {
      tiempoTranscurrido += Time.deltaTime;
      float t = Mathf.Clamp01(tiempoTranscurrido / duracionTransicion);

      liftGammaGain.lift.Override(Vector4.Lerp(liftHerida, liftInicial, t));
      liftGammaGain.gamma.Override(Vector4.Lerp(gammaHerida, gammaInicial, t));
      liftGammaGain.gain.Override(Vector4.Lerp(gainHerida, gainInicial, t));

      yield return null;
    }

    estaHerido = false;
  }
}