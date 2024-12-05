using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatformChecker : MonoBehaviour
{
    [SerializeField] private Transform objectToAttach; // Objeto a ser padre de la plataforma
    [SerializeField] private float rayLength = 1.5f; // Longitud del Raycast
    [SerializeField] private LayerMask platformLayer; // Capa para detectar plataformas
    private Transform currentPlatform; // Plataforma actual
    private Vector3[] rayOffsets = {
        Vector3.zero,                    // Centro
        new Vector3(0.2f, 0, 0),         // Derecha
        new Vector3(-0.2f, 0, 0),        // Izquierda
        new Vector3(0, 0, 0.2f),         // Adelante
        new Vector3(0, 0, -0.2f)         // Atrás
    };

    void LateUpdate()
    {
        bool platformDetected = false;

        foreach (var offset in rayOffsets)
        {
            Vector3 rayOrigin = objectToAttach.position + offset;
            RaycastHit hit;

            // Realiza el Raycast con la capa específica de plataformas
            if (Physics.Raycast(rayOrigin, Vector3.down, out hit, rayLength, platformLayer))
            {
                // Dibuja el raycast en verde si impacta algo
                Debug.DrawLine(rayOrigin, hit.point, Color.green);

                // Verifica si es una nueva plataforma
                
                platformDetected = true;

                if (currentPlatform != hit.collider.transform)
                {
                    currentPlatform = hit.collider.transform;
                    objectToAttach.SetParent(currentPlatform);
                }
                
                break; // Rompe el bucle si ya detectó la plataforma
            }
            else
            {
                // Dibuja el raycast en rojo si no impacta nada
                Debug.DrawLine(rayOrigin, rayOrigin + Vector3.down * rayLength, Color.red);
            }
        }

        // Si no se detectó plataforma
        if (!platformDetected && currentPlatform != null)
        {
            objectToAttach.SetParent(null);
            currentPlatform = null;
        }
    }
}
