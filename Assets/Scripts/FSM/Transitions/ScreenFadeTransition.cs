using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A transition that fades the screen out during the exit phase and in during the enter phase
/// </summary>
public class ScreenFadeTransition : IStateTransition
{
    private Canvas canvas;
    private Image fade;
    private float fadeTime;
 
    /// <summary>
    /// Create the transition
    /// </summary>
    /// <param name="fadeTime">Time in seconds to complete both parts of the phase</param>
    public ScreenFadeTransition(float fadeTime)
    {
        // Set up the fade cover
        var screenFadePrefab = Resources.Load<Canvas>("ScreenFade");
        canvas = UnityEngine.Object.Instantiate(screenFadePrefab);
        var coverGO = canvas.transform.Find("Cover");
        fade = coverGO.GetComponent<Image>();
        this.fadeTime = fadeTime;
    }
 
    public IEnumerable Exit()
    {
        // Fade out
        foreach (var e in TweenAlpha(0, 1, fadeTime / 2))
        {
            yield return e;
        }
    }
 
    public IEnumerable Enter()
    {
        // Fade in
        foreach (var e in TweenAlpha(1, 0, fadeTime / 2))
        {
            yield return e;
        }
 
        // Clean up the fade cover
        UnityEngine.Object.Destroy(canvas.gameObject);
    }
 
    // Tween the alpha of the fade cover
    private IEnumerable TweenAlpha(
        float fromAlpha,
        float toAlpha,
        float duration
    )
    {
        var startTime = Time.time;
        var endTime = startTime + duration;
        while (Time.time < endTime)
        {
            var sinceStart = Time.time - startTime;
            var percent = sinceStart / duration;
            var color = fade.color;
            color.a = Mathf.Lerp(fromAlpha, toAlpha, percent);
            fade.color = color;
            yield return null;
        }
    }
}