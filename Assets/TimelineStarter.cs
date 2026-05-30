using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

[RequireComponent(typeof(PlayableDirector))]
public class TimelineStarter : MonoBehaviour
{
    private PlayableDirector director;

    void Awake()
    {
        director = GetComponent<PlayableDirector>();
    }

    IEnumerator Start()
    {
        // Esperamos exactamente un frame para que el DeltaTime gigante
        // generado por la activación de la escena se resetee a la normalidad.
        yield return null; 
        
        // Ahora es 100% seguro reproducir la cinemática en GameTime
        director.Play();
    }
}