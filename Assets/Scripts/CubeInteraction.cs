using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CubeInteraction : MonoBehaviour
{
    public GameObject dialogueUI; // Assign a UI Panel in Unity
    public TMP_Text dialogueText; // Assign a UI Text for displaying cube responses
    public Button hiButton;
    public Button byeButton;
    public Transform cubeTransform; // Assign the cube's transform
    public float runSpeed = 2f; // Speed at which the cube runs away
    public AudioSource badFanfare;
    public AudioSource goodFanfare;

    private bool playerInRange = false;
    private bool isRunningAway = false;

    void Start()
    {
        dialogueUI.SetActive(false); // Hide UI initially
        hiButton.onClick.AddListener(SayHi);
        byeButton.onClick.AddListener(SayBye);
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E)) // Press E to interact
        {
            dialogueUI.SetActive(true);
        }

        if (isRunningAway)
        {
            cubeTransform.Translate(Vector3.forward * runSpeed * Time.deltaTime); // Move the cube backward
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("Say hi!");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            Debug.Log("Get closer to say hi!");
            dialogueUI.SetActive(false);
        }
    }

    void SayHi()
    {
        dialogueText.text = "Rude! Don't talk to a girl like that! Bye felicia!";
        if (badFanfare != null)
        {
            badFanfare.Play();
        }
        StartCoroutine(CloseDialogueAndRunAway(3f));
    }

    void SayBye()
    {
        dialogueText.text = "Why thank you.. what's your shell-number?";
        if (goodFanfare != null)
        {
            goodFanfare.Play();
        }
    }

    IEnumerator CloseDialogueAndRunAway(float delay)
    {
        yield return new WaitForSeconds(delay);
        dialogueUI.SetActive(false);
        isRunningAway = true;
    }
}
