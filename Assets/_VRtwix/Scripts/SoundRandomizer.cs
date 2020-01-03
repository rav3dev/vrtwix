using UnityEngine;
[RequireComponent(typeof(AudioSource))]

public class SoundRandomizer : MonoBehaviour
{
    public AudioSource audioSource;
    public void PlayOneShotAndRandomize(AudioClip Sound)
    {
        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.PlayOneShot(Sound);
    }
}
