using System;
using System.Collections;

using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyHealth : MonoBehaviour, IDamagable, IHealtheable
{
  [SerializeField] int _maxHealth = 50;
  [SerializeField] HealthBar _healthBar;
  int _currentHealth;
  [SerializeField] Animator _anim;
  [SerializeField] Rigidbody _rb;
  [SerializeField] GameObject _rootGameObject;
  [SerializeField] float _damageCooldown = 1f;
  [SerializeField] AudioEvent deathEvent = null;
  [SerializeField, Range(0f, 100f)] float _dropChance = 70f;
  [SerializeField] float offsetSpawnItemY = 0f;

  [SerializeField] SkinnedMeshRenderer _skinnedMeshRenderer;
  [SerializeField] Material _defaultMaterial;
  [SerializeField] Material _deathMaterial;

  Collider _damageCollider;
  Enemy _controller;
  EnemyAnimation _animation;
  float _lastDamageTime;

  EnemyNavigation _navigation;

  private void Awake()
  {
    _currentHealth = _maxHealth;
    if (_anim == null) _anim = GetComponent<Animator>();
    if (_rb == null) _rb = GetComponent<Rigidbody>();
    if (_rootGameObject == null) _rootGameObject = gameObject;
    _damageCollider = GetComponent<Collider>();
    if (_skinnedMeshRenderer != null && _defaultMaterial != null)
    {
      _skinnedMeshRenderer.material = _defaultMaterial;
    }
  }

  private void Start()
  {
    _healthBar.Disable();
    _controller = _rootGameObject.GetComponent<Enemy>();
    _animation = _controller.GetComponent<EnemyAnimation>();
    _navigation = _controller.GetComponent<EnemyNavigation>();
  }

  public void Damage(int damage)
  {
    if (Time.time >= _lastDamageTime + _damageCooldown)
    {
      //Debug.Log($"El player le da un da�o de {damage} a {gameObject.name}");
      _currentHealth -= damage;
      if (_currentHealth <= 0)
      {
        _currentHealth = 0;
        GameManager.Instance.AudioManager.PlayAudio(deathEvent);
        StartCoroutine(Die());
      }
      else
      {
        _animation.SetAnimator("Hit");
      }
      _healthBar.UpdateHealthBar();
      _lastDamageTime = Time.time;
    }
  }

  public bool IsDead() => _currentHealth <= 0;

  IEnumerator Die()
  {
    //_anim.SetTrigger("Die");
    _healthBar.Disable();
    _controller.SetState(new EnemyDeathState(_controller, _animation, _navigation));
    //_rb.isKinematic = true;
    //_rb.detectCollisions = false;
    _rb.constraints = RigidbodyConstraints.None;

    if (_skinnedMeshRenderer != null && _deathMaterial != null)
    {
      _skinnedMeshRenderer.material = _deathMaterial;
      StartCoroutine(Desintegrate());
    }

    yield return new WaitForSeconds(3f);
    TrySpawnLootItem();
    Destroy(_rootGameObject);
  }

  IEnumerator Desintegrate()
  {
    float duration = 3f;
    float timer = 0f;

    while (timer < duration)
    {
      float fadeValue = Mathf.Lerp(1f, 0f, timer / duration);
      _deathMaterial.SetFloat("_Fadein", fadeValue);
      timer += Time.deltaTime;
      yield return null;
    }
    _deathMaterial.SetFloat("_Fadein", 0f);
  }

  public int GetCurrentHealth() => _currentHealth;
  public int GetMaxHealth() => _maxHealth;

  public void EnableHealthBar() => _healthBar.Enable();
  public void DisableHealthBar() => _healthBar.Disable();

  public void DisableDamage()
  {
    _damageCollider.enabled = false;
  }

  public void TrySpawnLootItem()
  {
    if (Random.Range(0f, 100f) < _dropChance)
    {
      int itemID = ItemManager.Instance.GetRandomItemID();
      Item item = ItemManager.Instance.GetItemFromID(itemID);
      int amount = 1;
      if (item is Projectile || item is Consumible)
      {
        amount = ItemManager.Instance.GetRandomAmmountOfItem(itemID);
      }

            ItemManager.Instance.GenerateItemInWorldSpace(itemID, amount, transform.position + new Vector3(0f, offsetSpawnItemY, 0f));
        }
  }
}
