using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyLife : MonoBehaviour
{
    public float explodeTime = 0.25f; // Tiempo antes de destruir el enemigo
    public float prefabDestroyTime = 1.0f; // Tiempo antes de destruir el prefab generado
    public GameObject spawnPrefab; // Prefab que se generará al destruir al enemigo

    void Start()
    {
        // Inicialización si es necesario
    }

    void Update()
    {
        // Lógica adicional si es necesario
    }

    // Método para detectar colisiones con el player
    private void OnTriggerEnter(Collider other)
    {
        // Verifica si el objeto que entra en el trigger tiene el tag "Player"
        if (other.CompareTag("Player"))
        {
            Invoke("ExplodeEnemy", explodeTime);
        }
    }

    private void ExplodeEnemy()
    {
        // Genera el prefab en la posición y rotación del enemigo
        if (spawnPrefab != null)
        {
            GameObject spawnedObject = Instantiate(spawnPrefab, transform.position, transform.rotation);
            // Destruye el prefab después de un tiempo
            Destroy(spawnedObject, prefabDestroyTime);
        }

        // Elimina este objeto (el enemigo)
        Destroy(gameObject);
    }
}
