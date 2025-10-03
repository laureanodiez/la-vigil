using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class triggerActivacion : MonoBehaviour
{
    [Header("Eventos")]
    [SerializeField] private GameObject trigger;
    [SerializeField] private GameObject objeto;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            objeto.SetActive(true);
        }
    }
}
