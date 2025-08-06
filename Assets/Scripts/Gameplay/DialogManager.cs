using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;

public class DialogueManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject dialogPanel;
    public Image portrait;
    public TMP_Text dialogText;

    [Header("Audio")]
    public AudioSource sfxTyping;

    [Header("References")]
    public PlayerController player;       // arrastra tu Player
    public Light2D globalLight;             // tu Light (no 2D)
    public GameObject constancioSprite;   // GameObject con SpriteRenderer de Constancio
    public GameObject noteObject;         // GameObject nota, inactivo al inicio

    [Header("Portrait Sprites")]
    public Sprite constancioPortrait;
    public Sprite quimiPortrait;

    private Queue<string> lines;
    private bool isTyping = false;
    private float typingSpeed = 0.03f;

    void Awake() {
        lines = new Queue<string>();
        dialogPanel.SetActive(false);
        constancioSprite.SetActive(true);
        noteObject.SetActive(false);
    }

    public void StartConversation() {
        // 1) Llena la cola con todas las líneas, alternando personajes
        lines.Clear();
        lines.Enqueue("Constancio: Agarraste el teléfono del mostrador?");
        lines.Enqueue("Quimi: Vos de nuevo? Por favor, decime dónde estoy.");
        lines.Enqueue("Constancio: En La Vigil.");
        lines.Enqueue("Quimi: Ya sé eso, pero...");
        lines.Enqueue("Constancio: Me vas a tener que escuchar bien. Hay solo una manera de salir y tenés que tener mucho cuidado con...");

        // La cortina del corte de luz y continuación será manejada al final
        ShowNextLine();
    }

    public void ShowNextLine() {
        if (isTyping) {
            // Fini­sh immediately
            StopAllCoroutines();
            dialogText.text = lines.Peek();
            isTyping = false;
            return;
        }

        if (lines.Count == 0) {
            EndFirstConversation();
            return;
        }

        string line = lines.Dequeue();
        StartCoroutine(TypeLine(line));
    }

    IEnumerator TypeLine(string line) {
        isTyping = true;
        dialogText.text = "";
        dialogPanel.SetActive(true);
        player.canMove = false;

        // Cambiar retrato según personaje
        if (line.StartsWith("Constancio")) {
            portrait.sprite = constancioPortrait;
        } else {
            portrait.sprite = quimiPortrait;
        }

        foreach (char c in line) {
            dialogText.text += c;
            sfxTyping.Play();
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    void EndFirstConversation() {
        // Cortar luz
        StartCoroutine(LightCutSequence());
    }

    IEnumerator LightCutSequence() {
        // 1) Instant off
        globalLight.enabled = false;
        // play sound de corte
        yield return new WaitForSeconds(1.5f);
        // 2) Vuelve la luz
        globalLight.enabled = true;
        // play sonido de luz volviendo
        dialogPanel.SetActive(false);
        constancioSprite.SetActive(false);

        // Activar nota
        noteObject.SetActive(true);

        // 3) Segunda conversación con la nota
        lines.Clear();
        lines.Enqueue("“Subsuelo, miau”");
        lines.Enqueue("Quimi: Tengo que salir de acá lo antes posible, tengo que ir al subsuelo");
        ShowNextLine();  // vuelve a ShowNextLine
    }

    public void HandleSpace() {
        ShowNextLine();
    }
}
