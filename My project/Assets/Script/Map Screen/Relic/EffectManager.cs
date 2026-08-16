using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance;


    private void Awake()
    {
        Instance = this;
    }


    public void PlayEffect(
        GameObject effectPrefab,
        Vector3 position)
    {
        GameObject effect =
            Instantiate(
                effectPrefab,
                position,
                Quaternion.identity);


        Destroy(
            effect,
            GetEffectDuration(effect));
    }


    private float GetEffectDuration(GameObject effect)
    {
        ParticleSystem[] particles =
            effect.GetComponentsInChildren<ParticleSystem>();


        float maxDuration = 0f;


        foreach (ParticleSystem particle in particles)
        {
            float duration =
                particle.main.duration +
                particle.main.startLifetime.constantMax;


            if (duration > maxDuration)
                maxDuration = duration;
        }


        return maxDuration;
    }
}