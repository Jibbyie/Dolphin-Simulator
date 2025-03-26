using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CubeInteraction : MonoBehaviour
{
    [Header("Quest Setup")]
    public GameObject Enemy;

    [Header("UI Elements")]
    public GameObject dialogueUI;
    public TMP_Text dialogueText;
    public TMP_Text missionText;
    public TextMeshPro questFloatingText;
    public Button acceptButton;
    public Button declineButton;
    public CanvasGroup missionCompleteUI;

    [Header("Audio")]
    public AudioSource missionAcceptedSound;
    public AudioSource missionCompleteSound;
    public AudioSource backgroundMusic;            
    public AudioSource combatMusic;           
    
    private bool playerInRange = false;
    private bool missionDeclined = false;
    private bool missionGiven = false;

    // Dialogue system
    private DialogueNode currentNode;
    private DialogueNode rootNode;

    void Start()
    {
        dialogueUI.SetActive(false);
        acceptButton.onClick.AddListener(AcceptMission);
        declineButton.onClick.AddListener(DeclineMission);

        acceptButton.gameObject.SetActive(false);
        declineButton.gameObject.SetActive(false);

        if (questFloatingText != null)
        {
            questFloatingText.gameObject.SetActive(true);
            questFloatingText.text = "Yo! Come over here you bum!";
        }

        BuildDialogueTree();
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            dialogueUI.SetActive(true);

            if (questFloatingText != null)
                questFloatingText.gameObject.SetActive(false);

            if (SimpleQuestManager.Instance.enemyDefeated && SimpleQuestManager.Instance.missionAccepted && !SimpleQuestManager.Instance.rewardClaimed)
            {

                if (questFloatingText != null)
                    questFloatingText.gameObject.SetActive(false);

                dialogueText.text = "Good work. The reef owes you one.";
                missionText.text = "";
                SimpleQuestManager.Instance.rewardClaimed = true;
                acceptButton.gameObject.SetActive(false);
                declineButton.gameObject.SetActive(false);

                if (combatMusic != null)
                    combatMusic.Stop();

                if (backgroundMusic != null && !backgroundMusic.isPlaying)
                    backgroundMusic.Play();

                if (missionCompleteSound != null)
                    missionCompleteSound.Play();

                if (missionCompleteUI != null)
                    StartCoroutine(FadeMissionCompleteText());

                return;
            }

            if (!missionGiven)
            {
                ShowNode(rootNode);
                return;
            }
        }

        if (questFloatingText != null)
        {
            Vector3 camDir = questFloatingText.transform.position - Camera.main.transform.position;
            questFloatingText.transform.rotation = Quaternion.LookRotation(camDir);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            dialogueUI.SetActive(false);

            if (!SimpleQuestManager.Instance.missionAccepted && !missionDeclined && questFloatingText != null)
                questFloatingText.gameObject.SetActive(true);
        }
    }

    public void OnEnemyDefeated()
    {
        if (questFloatingText != null)
        {
            questFloatingText.gameObject.SetActive(true);
            questFloatingText.text = "You handled that! Come back foo!";
        }
    }

    void AcceptMission()
    {
        dialogueText.text = "Aight. Go deal with him.";
        SimpleQuestManager.Instance.missionAccepted = true;

        if (Enemy != null)
            Enemy.SetActive(true);

        if (missionAcceptedSound != null)
            missionAcceptedSound.Play();

        if (backgroundMusic != null)
            backgroundMusic.Stop();

        if (combatMusic != null && !combatMusic.isPlaying)
            combatMusic.Play();


        acceptButton.gameObject.SetActive(false);
        declineButton.gameObject.SetActive(false);
        missionText.text = "";

        if (questFloatingText != null)
            questFloatingText.gameObject.SetActive(false);

        StartCoroutine(CloseDialogueAfterDelay(2f));
    }

    void DeclineMission()
    {
        missionDeclined = true;
        dialogueText.text = "Suit yourself...";

        if (questFloatingText != null)
            questFloatingText.gameObject.SetActive(false);

        CleanupMissionUI();
    }

    void CleanupMissionUI()
    {
        acceptButton.gameObject.SetActive(false);
        declineButton.gameObject.SetActive(false);
        missionText.text = "";
    }
    IEnumerator FadeMissionCompleteText()
    {
        missionCompleteUI.alpha = 0;
        missionCompleteUI.gameObject.SetActive(true);

        float fadeDuration = 1.5f;   
        float visibleDuration = 2.5f;

        // Fade in
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            missionCompleteUI.alpha = Mathf.Lerp(0, 1, t / fadeDuration);
            yield return null;
        }

        yield return new WaitForSeconds(visibleDuration);

        // Fade out
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            missionCompleteUI.alpha = Mathf.Lerp(1, 0, t / fadeDuration);
            yield return null;
        }

        missionCompleteUI.gameObject.SetActive(false);
    }



    IEnumerator CloseDialogueAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        dialogueUI.SetActive(false);
    }

    // --------------------------
    // Dialogue System Additions
    // --------------------------

    void BuildDialogueTree()
    {
        // Final mission dialogue
        DialogueNode missionNode = new DialogueNode("Yo. I got a job for you...");
        missionNode.onSelected = ShowMissionOptions;

        // "You, bum." branch
        DialogueNode bumReply = new DialogueNode("You, bum.");
        bumReply.options.Add(new DialogueOption { text = "Watch who you're talking to", nextNode = missionNode });
        bumReply.options.Add(new DialogueOption { text = "Man, what you want", nextNode = missionNode });

        // Root interaction
        rootNode = new DialogueNode("Yo! Come over here you bum!");
        rootNode.options.Add(new DialogueOption { text = "Who you calling a bum?", nextNode = bumReply });
        rootNode.options.Add(new DialogueOption { text = "What you want foo?", nextNode = missionNode });
    }

    void ShowNode(DialogueNode node)
    {
        currentNode = node;
        dialogueText.text = node.message;
        missionText.text = "";

        acceptButton.onClick.RemoveAllListeners();
        declineButton.onClick.RemoveAllListeners();

        if (node.options.Count >= 1)
        {
            acceptButton.gameObject.SetActive(true);
            acceptButton.GetComponentInChildren<TMP_Text>().text = node.options[0].text;
            acceptButton.onClick.AddListener(() => ShowNode(node.options[0].nextNode));
        }
        else acceptButton.gameObject.SetActive(false);

        if (node.options.Count >= 2)
        {
            declineButton.gameObject.SetActive(true);
            declineButton.GetComponentInChildren<TMP_Text>().text = node.options[1].text;
            declineButton.onClick.AddListener(() => ShowNode(node.options[1].nextNode));
        }
        else declineButton.gameObject.SetActive(false);

        node.onSelected?.Invoke();
    }

    void ShowMissionOptions()
    {
        missionGiven = true;
        dialogueText.text = "Yo. I got a job for you...";
        missionText.text = "Take down that rogue red cube. He's been wildin'. Bring back his pearl chain.";

        acceptButton.gameObject.SetActive(true);
        declineButton.gameObject.SetActive(true);

        acceptButton.GetComponentInChildren<TMP_Text>().text = "<color=green>Bet, I'm on it</color>";
        declineButton.GetComponentInChildren<TMP_Text>().text = "<color=red>Nah, I'm good</color>";

        acceptButton.onClick.RemoveAllListeners();
        declineButton.onClick.RemoveAllListeners();

        acceptButton.onClick.AddListener(AcceptMission);
        declineButton.onClick.AddListener(DeclineMission);
    }
}
