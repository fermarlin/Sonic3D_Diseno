using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyLife : MonoBehaviour
{
    public float explodeTime=.25f;

    void Start()
    {
        // Inicialización si es necesario
    }

    // Update is called once per frame
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
            Invoke("ExplodeEnemy",explodeTime);
        }
    }

    private void ExplodeEnemy(){
            Destroy(gameObject); // Elimina este objeto (el enemigo)

    }
}
