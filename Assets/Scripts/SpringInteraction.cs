using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringInteraction : MonoBehaviour
{
    
    public AudioClip springSound; 
    AudioSource aud;

    private void Start(){
        aud = GetComponent<AudioSource>();

    }


    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player"))
        {
            PlaySound(springSound);
        }
    }

        public void PlaySound(AudioClip clip)
        {
            aud.clip = clip;
            aud.Play();
        }
}
