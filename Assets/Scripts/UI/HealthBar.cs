using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Min(0f)]
    public float updateSpeedInSeconds;
    [Min(0f)]
    public float positionOffset;

    [SerializeField]
    private Image _healthBarImage;
    [SerializeField]
    private Image _manaBarImage;

    private Health health;

    public void SetHealth(Health health)
    {
        this.health = health;
        _healthBarImage.fillAmount = this.health.currentHealth;
        _manaBarImage.fillAmount = this.health.currentMana;
        this.health.OnHPChanged += HandleHPChanged;
        this.health.OnMPChanged += HandleMPChanged;
    }


    private void HandleHPChanged(float hpPercent)
    {
        StartCoroutine(ChangeHPToPercent(hpPercent));
    }

    private void HandleMPChanged(float mpPercent)
    {
        StartCoroutine(ChangeMPToPercent(mpPercent));
    }

    private IEnumerator ChangeHPToPercent(float percent)
    {
        var oldPercent = _healthBarImage.fillAmount;
        float elapsed = 0f;

        while(elapsed < updateSpeedInSeconds)
        {
            elapsed += Time.deltaTime;
            _healthBarImage.fillAmount = Mathf.Lerp(oldPercent, percent, elapsed / updateSpeedInSeconds);
            yield return null;
        }

        _healthBarImage.fillAmount = percent;
    }
    private IEnumerator ChangeMPToPercent(float percent)
    {
        var oldPercent = _manaBarImage.fillAmount;
        float elapsed = 0f;

        while(elapsed < updateSpeedInSeconds)
        {
            elapsed += Time.deltaTime;
            _manaBarImage.fillAmount = Mathf.Lerp(oldPercent, percent, elapsed / updateSpeedInSeconds);
            yield return null;
        }

        _manaBarImage.fillAmount = percent;
    }

    private void LateUpdate()
    {
        transform.position = Camera.main.WorldToScreenPoint( health.transform.position + Vector3.up * positionOffset);
    }

    private void OnDestroy()
    {
        health.OnHPChanged -= HandleHPChanged;
        health.OnMPChanged -= HandleMPChanged;
    }
}