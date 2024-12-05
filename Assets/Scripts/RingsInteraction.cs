using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingsInteraction : MonoBehaviour
{
    public float prefabDestroyTime = .2f; // Tiempo antes de destruir el prefab generado
    public GameObject spawnPrefab; // Prefab que se generará al destruir al enemigo

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player"))
        {
            
            Invoke("GetRing", prefabDestroyTime);


        }
    }

    public void GetRing(){
        // Genera el prefab en la posición y rotación del enemigo
        if (spawnPrefab != null)
        {
            GameObject spawnedObject = Instantiate(spawnPrefab, transform.position, transform.rotation);
            // Destruye el prefab después de un tiempo
            Destroy(spawnedObject, prefabDestroyTime+2);
        }

            Destroy(gameObject);
    }



}
