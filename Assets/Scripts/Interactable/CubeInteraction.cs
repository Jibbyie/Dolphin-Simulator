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
    public CanvasGroup missionOverlayTextGroup;
    public TMP_Text missionOverlayText;


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
            questFloatingText.text = "Yo! Wanna make some quick pearls?";
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

            // Show floating text again only if mission wasn't accepted or declined
            if (!SimpleQuestManager.Instance.missionAccepted && !missionDeclined && questFloatingText != null)
            {
                questFloatingText.gameObject.SetActive(true);
            }
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
        StartCoroutine(PlayMissionCinematic());

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
    IEnumerator PlayMissionCinematic()
    {
        // Disable movement
        DolphinMovement movement = FindObjectOfType<DolphinMovement>();
        if (movement != null)
            movement.enabled = false;

        // Hide dialogue UI
        if (dialogueUI != null)
            dialogueUI.SetActive(false);

        // Set mission overlay text
        if (missionOverlayText != null)
            missionOverlayText.text = "Take his *** out.";

        // Fade in overlay during pan
        if (missionOverlayTextGroup != null)
        {
            missionOverlayTextGroup.alpha = 0;
            missionOverlayTextGroup.gameObject.SetActive(true);
        }

        ThirdPersonCamera cam = Camera.main.GetComponent<ThirdPersonCamera>();
        if (cam != null)
            cam.PlayCinematicPan(Enemy.transform, 3.5f);

        float fadeInDuration = 1.5f;
        float holdDuration = 1.5f;
        float fadeOutDuration = 1f;
        float totalDuration = fadeInDuration + holdDuration + fadeOutDuration;

        float timer = 0f;

        while (timer < totalDuration)
        {
            timer += Time.deltaTime;

            // Fade in
            if (timer < fadeInDuration)
            {
                missionOverlayTextGroup.alpha = Mathf.Lerp(0, 1, timer / fadeInDuration);
            }
            // Hold
            else if (timer < fadeInDuration + holdDuration)
            {
                missionOverlayTextGroup.alpha = 1;
            }
            // Fade out
            else
            {
                float t = (timer - fadeInDuration - holdDuration) / fadeOutDuration;
                missionOverlayTextGroup.alpha = Mathf.Lerp(1, 0, t);
            }

            yield return null;
        }

        missionOverlayTextGroup.alpha = 0;
        missionOverlayTextGroup.gameObject.SetActive(false);

        // Re-enable movement
        if (movement != null)
            movement.enabled = true;
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
        DialogueNode missionNode = new DialogueNode("I got a bounty for you... 100 pearls and some Aura. There's a rogue dolphin stirrin' up trouble near the coral ridge.");
        missionNode.onSelected = ShowMissionOptions;

        // Extra lore/weirdness node
        DialogueNode weirdLoreNode = new DialogueNode("Ever since that outlaw showed up, reef’s been twitchin’. Fish whisper his name like a ghost.");
        weirdLoreNode.options.Add(new DialogueOption { text = "Sounds serious.", nextNode = missionNode });
        weirdLoreNode.options.Add(new DialogueOption { text = "I'm listening...", nextNode = missionNode });

        // Extra joke/attitude deflection
        DialogueNode deflectNode = new DialogueNode("Don’t act clueless. You out here floatin’ like you got no direction.");
        deflectNode.options.Add(new DialogueOption { text = "Fair enough.", nextNode = missionNode });
        deflectNode.options.Add(new DialogueOption { text = "Alright, hit me with it.", nextNode = missionNode });

        // Deep insult comeback
        DialogueNode insultReply = new DialogueNode("You, driftin’ around like a lost sardine.");
        insultReply.options.Add(new DialogueOption { text = "Watch your tone.", nextNode = deflectNode });
        insultReply.options.Add(new DialogueOption { text = "Whatever, talk.", nextNode = weirdLoreNode });

        // Side tangent
        DialogueNode sideTrack = new DialogueNode("What I want? Justice. Respect. Pearls. But mostly that fool gone.");
        sideTrack.options.Add(new DialogueOption { text = "That all?", nextNode = weirdLoreNode });
        sideTrack.options.Add(new DialogueOption { text = "Let’s skip to the bounty.", nextNode = missionNode });

        // Root node: first interaction
        rootNode = new DialogueNode("You lookin’ for pearls, huh? Got a job, if you got the gills for it.");
        rootNode.options.Add(new DialogueOption { text = "Who you calling gilled?", nextNode = insultReply });
        rootNode.options.Add(new DialogueOption { text = "You got work?", nextNode = sideTrack });
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
        dialogueText.text = "So... there's a rogue dolphin out there actin' wild. Name's Dilly The Kid. Real slippery.";
        missionText.text = "Take him out, bring back his pearl chain, and you'll earn 100 pearls and some serious Aura.";


        acceptButton.gameObject.SetActive(true);
        declineButton.gameObject.SetActive(true);

        acceptButton.GetComponentInChildren<TMP_Text>().text = "<color=#4CAF50>Bet, I'm on it</color>";
        declineButton.GetComponentInChildren<TMP_Text>().text = "<color=red>Nah cuzz, I'm good</color>";

        acceptButton.onClick.RemoveAllListeners();
        declineButton.onClick.RemoveAllListeners();

        acceptButton.onClick.AddListener(AcceptMission);
        declineButton.onClick.AddListener(DeclineMission);
    }
}
