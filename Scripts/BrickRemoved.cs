using System.Collections;
using UnityEngine;

public class BrickRemoved : MonoBehaviour
{
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    public float delayBeforeReset = 3.0f;
    public float returnSpeed = 2.0f;
    public float returnSpeedRotation = 6.0f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip[] audioClipHit;

    private Coroutine resetCoroutine = null;

    void Start()
    {
        // Enregistrer la position et la rotation initiales
        initialPosition = transform.position;
        initialRotation = transform.rotation;

        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        // Pas besoin de lancer la coroutine ici, on la lancera seulement quand la balle impacte
    }

    void OnCollisionEnter(Collision collision)
    {
        PlayRandomHitSound();
    }

    private void PlayRandomHitSound()
    {
        if (audioClipHit.Length > 0)
        {
            AudioClip ramdomClip = audioClipHit[Random.Range(0, audioClipHit.Length)];
            audioSource.PlayOneShot(ramdomClip);
        }
    }
    public void StartReset()
    {
        // Si une coroutine de reset est déjà en cours, l'arrêter avant d'en lancer une nouvelle
        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
        }

        // Lancer une nouvelle coroutine de reset
        resetCoroutine = StartCoroutine(ResetAfterDelay());
    }

    private IEnumerator ResetAfterDelay()
    {
        // Attendre avant de commencer la reconstruction
        yield return new WaitForSeconds(delayBeforeReset);

        // Supprimer le Rigidbody pour réinitialiser la brique
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            Destroy(rb);
        }
        Debug.Log("RB détruit");

        // Réinitialiser la position et la rotation progressivement
        while (Vector3.Distance(transform.position, initialPosition) > 0.01f || Quaternion.Angle(transform.rotation, initialRotation) > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, initialPosition, returnSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, initialRotation, returnSpeedRotation * Time.deltaTime);

            yield return null; // Attendre la fin de la frame avant de continuer
        }

        // Finalement, s'assurer que la brique est exactement à la position et rotation initiales
        transform.position = initialPosition;
        transform.rotation = initialRotation;

        // Reset de la variable de coroutine une fois terminée
        resetCoroutine = null;
    }
}
