using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para cargar escenas

public class IntroManager : MonoBehaviour
{
    public AudioSource musicSource; // Variable para el componente de audio

    public AudioSource audioSource; // Variable para el componente de audio
    private bool hasPlayed = false; // Controla si el sonido ya fue reproducido
    public Animator an;
    void Start()
    {
        if (audioSource == null)
        {
            Debug.LogError("AudioSource no asignado en el inspector");
        }
    }

    void Update()
    {
        // Comprueba si se presionó la tecla Enter y si el audio aún no se ha reproducido
        if (Input.GetKeyDown(KeyCode.Return) && !hasPlayed)
        {
            musicSource.Stop();
            audioSource.Play(); // Reproduce el sonido
            hasPlayed = true; // Marca que el sonido ha sido reproducido
            an.enabled=true;
        }

        // Comprueba si el audio terminó de reproducirse
        if (!audioSource.isPlaying && hasPlayed)
        {
            SceneManager.LoadScene(1); // Cambia a la escena 1
        }
    }
}
