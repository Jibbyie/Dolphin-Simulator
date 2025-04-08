// ----- START OF CORRECTED CubeInteraction.cs -----
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
    public AudioSource dialogueAudioSource;
    public AudioClip[] dolphinSounds; // Keep this if you use it


    private bool playerInRange = false;
    private bool missionDeclined = false;
    private bool missionGiven = false;

    // Dialogue system
    private DialogueNode currentNode;
    private DialogueNode rootNode;

    // --- Reference to player's movement script ---
    private DolphinMovement playerMovement;

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
        // --- Check for Interaction Input ---
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && playerMovement != null) // Check playerMovement exists
        {
            // --- FREEZE PLAYER MOVEMENT ---
            playerMovement.SetMovementFreeze(true); // Use the new method

            dialogueUI.SetActive(true);

            if (questFloatingText != null)
                questFloatingText.gameObject.SetActive(false);

            // --- Handle Reward Claim ---
            if (SimpleQuestManager.Instance != null && SimpleQuestManager.Instance.enemyDefeated && SimpleQuestManager.Instance.missionAccepted && !SimpleQuestManager.Instance.rewardClaimed)
            {
                HandleRewardClaim(); // Encapsulated logic
                return; // Exit Update early after handling reward
            }

            // --- Handle Initial Quest Dialogue ---
            // Reset missionGiven flag if player interacts again after declining but before accepting
            if (missionDeclined && !SimpleQuestManager.Instance.missionAccepted)
            {
                missionGiven = false;
                missionDeclined = false; // Allow re-prompting
            }

            if (!missionGiven)
            {
                ShowNode(rootNode);
                return; // Exit Update early after showing initial dialogue
            }

            // (Optional: Add logic here if you want pressing E again to *resume* a conversation
            // or show a different message if the quest is already active but not complete)
        }

        // --- Update Floating Text Rotation ---
        if (questFloatingText != null && questFloatingText.gameObject.activeSelf)
        {
            Vector3 camDir = questFloatingText.transform.position - Camera.main.transform.position;
            // Avoid division by zero or tiny vectors if camera is exactly at text position
            if (camDir.sqrMagnitude > 0.001f)
            {
                questFloatingText.transform.rotation = Quaternion.LookRotation(camDir);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            // --- Cache player movement script ---
            playerMovement = other.GetComponent<DolphinMovement>();
            if (playerMovement == null)
            {
                Debug.LogError("Player entered trigger but has no DolphinMovement script!", other.gameObject);
            }
            else
            {
                // Successfully cached player movement
            }

            // Update and show floating text state
            if (questFloatingText != null && !dialogueUI.activeSelf) // Only show if dialogue isn't already up
            {
                UpdateFloatingTextState();
                questFloatingText.gameObject.SetActive(true);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            // If dialogue was active, close it AND re-enable movement
            if (dialogueUI.activeSelf)
            {
                dialogueUI.SetActive(false); // Close UI first

                if (playerMovement != null && !playerMovement.IsMovementEnabled())
                {
                    // --- RE-ENABLE MOVEMENT only if it was frozen ---
                    playerMovement.SetMovementFreeze(false);
                }
            }

            // Clear the cached reference ONLY when exiting trigger
            playerMovement = null;

            // Hide floating text when leaving range
            if (questFloatingText != null)
            {
                questFloatingText.gameObject.SetActive(false);
            }
        }
    }

    void HandleRewardClaim()
    {
        if (questFloatingText != null)
            questFloatingText.gameObject.SetActive(false);

        dialogueText.text = "Good work. The reef owes you one.";
        missionText.text = "";
        if (SimpleQuestManager.Instance != null) SimpleQuestManager.Instance.rewardClaimed = true;
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

        // Close dialogue and re-enable movement after delay
        StartCoroutine(CloseDialogueAfterDelay(2f, true)); // Pass true to re-enable movement
    }

    public void OnEnemyDefeated()
    {
        // Update text state when enemy defeated
        if (SimpleQuestManager.Instance != null) SimpleQuestManager.Instance.enemyDefeated = true;
        UpdateFloatingTextState();
        // If player happens to be in range, update the visible text immediately
        if (playerInRange && questFloatingText != null && !dialogueUI.activeSelf)
        {
            questFloatingText.gameObject.SetActive(true);
        }
    }

    void AcceptMission()
    {
        // Note: Movement is already frozen from pressing E

        // Hide UI immediately before cinematic
        CleanupMissionUI();
        dialogueUI.SetActive(false); // Hide main UI

        StartCoroutine(PlayMissionCinematic()); // Cinematic will re-enable movement at the end

        if (SimpleQuestManager.Instance != null) SimpleQuestManager.Instance.missionAccepted = true;
        missionDeclined = false; // Reset declined state if accepted

        if (Enemy != null)
            Enemy.SetActive(true);

        if (missionAcceptedSound != null)
            missionAcceptedSound.Play();

        if (backgroundMusic != null)
            backgroundMusic.Stop();

        if (combatMusic != null && !combatMusic.isPlaying)
            combatMusic.Play();

        if (questFloatingText != null)
            questFloatingText.gameObject.SetActive(false); // Ensure floating text is hidden
    }

    void DeclineMission()
    {
        missionDeclined = true; // Track that the player declined this session
        missionGiven = false; // Reset so next 'E' press starts dialogue again
        dialogueText.text = "Suit yourself...";

        CleanupMissionUI(); // Hide buttons etc.

        // Close dialogue and re-enable movement after delay
        StartCoroutine(CloseDialogueAfterDelay(1.5f, true)); // Pass true to re-enable movement

        // Keep floating text hidden after declining until player leaves/re-enters trigger
        if (questFloatingText != null)
            questFloatingText.gameObject.SetActive(false);
    }

    // Helper to hide buttons and mission text
    void CleanupMissionUI()
    {
        acceptButton.gameObject.SetActive(false);
        declineButton.gameObject.SetActive(false);
        missionText.text = "";
    }

    // Updates the floating text based on current quest progress
    void UpdateFloatingTextState()
    {
        if (questFloatingText == null || SimpleQuestManager.Instance == null) return;

        if (SimpleQuestManager.Instance.rewardClaimed)
        {
            questFloatingText.text = "Respect.";
        }
        else if (SimpleQuestManager.Instance.enemyDefeated)
        {
            questFloatingText.text = "You handled that! Come back foo!";
        }
        else if (SimpleQuestManager.Instance.missionAccepted)
        {
            questFloatingText.text = "Go get 'em!"; // Or hide it
            questFloatingText.gameObject.SetActive(false); // Option: hide while quest active
        }
        else
        {
            // Default text or "Changed your mind?" if they declined this session
            questFloatingText.text = missionDeclined ? "Changed your mind?" : "Yo! Wanna make some quick pearls?";
        }
    }

    void PlayRandomDolphinSound()
    {
        if (dolphinSounds != null && dolphinSounds.Length > 0 && dialogueAudioSource != null)
        {
            int randomIndex = Random.Range(0, dolphinSounds.Length);
            dialogueAudioSource.PlayOneShot(dolphinSounds[randomIndex]);
        }
    }


    IEnumerator PlayMissionCinematic()
    {
        // Movement should already be frozen by the initial 'E' press interaction

        // Hide dialogue UI (already done in AcceptMission, but safe to repeat)
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
        if (cam != null && Enemy != null) // Check Enemy exists before panning
            cam.PlayCinematicPan(Enemy.transform, 3.5f); // Duration of the pan itself
        else if (Enemy == null)
            Debug.LogError("Enemy Transform not assigned for cinematic pan!", this);


        // --- Calculate timings for overlay fade ---
        float panDuration = 3.5f; // Match the camera pan duration
        float fadeInDuration = 1.0f;
        float holdDuration = panDuration - fadeInDuration - 0.5f; // Hold for most of pan
        float fadeOutDuration = 0.5f;
        float totalDuration = fadeInDuration + holdDuration + fadeOutDuration;
        // Ensure totalDuration fits within or slightly exceeds panDuration
        totalDuration = Mathf.Max(totalDuration, panDuration);


        float timer = 0f;

        while (timer < totalDuration)
        {
            timer += Time.deltaTime;

            // Update overlay alpha based on timer progress
            if (missionOverlayTextGroup != null)
            {
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
                else if (timer < totalDuration)
                {
                    float t = (timer - fadeInDuration - holdDuration) / fadeOutDuration;
                    missionOverlayTextGroup.alpha = Mathf.Lerp(1, 0, t);
                }
                else
                { // Ensure alpha is 0 at the end
                    missionOverlayTextGroup.alpha = 0;
                }
            }

            yield return null; // Wait for next frame
        }

        // Cleanup overlay UI
        if (missionOverlayTextGroup != null)
        {
            missionOverlayTextGroup.alpha = 0;
            missionOverlayTextGroup.gameObject.SetActive(false);
        }


        // --- RE-ENABLE MOVEMENT after cinematic finishes ---
        if (playerMovement != null)
        {
            playerMovement.SetMovementFreeze(false);
        }
        else
        {
            // Attempt to find and re-enable if reference was lost (less ideal)
            DolphinMovement foundMovement = FindObjectOfType<DolphinMovement>();
            if (foundMovement != null) foundMovement.SetMovementFreeze(false);
        }
    }


    IEnumerator FadeMissionCompleteText()
    {
        if (missionCompleteUI == null) yield break; // Safety check

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
        missionCompleteUI.alpha = 1; // Ensure fully visible

        yield return new WaitForSeconds(visibleDuration);

        // Fade out
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            missionCompleteUI.alpha = Mathf.Lerp(1, 0, t / fadeDuration);
            yield return null;
        }
        missionCompleteUI.alpha = 0; // Ensure fully invisible

        missionCompleteUI.gameObject.SetActive(false);
    }


    // --- Updated to optionally re-enable movement ---
    IEnumerator CloseDialogueAfterDelay(float delay, bool reEnableMovement)
    {
        yield return new WaitForSeconds(delay);

        // Only close UI if it's still active (player might have moved away)
        if (dialogueUI.activeSelf)
        {
            dialogueUI.SetActive(false);
        }

        // Re-enable movement if requested AND the player reference is still valid (they haven't left trigger)
        if (reEnableMovement && playerMovement != null)
        {
            playerMovement.SetMovementFreeze(false);
        }

        // After closing dialogue, update floating text state and show if still in range
        if (playerInRange)
        {
            UpdateFloatingTextState();
            if (questFloatingText != null)
            {
                questFloatingText.gameObject.SetActive(true);
            }
        }
    }

    // --------------------------
    // Dialogue System Implementation (Using EXTERNAL DialogueNode/Option classes)
    // --------------------------

    void BuildDialogueTree()
    {
        // Final mission dialogue
        DialogueNode missionNode = new DialogueNode("I got a bounty for you... 100 pearls and some Aura. There's a rogue dolphin stirrin' up trouble near the coral ridge.");
        missionNode.onSelected = ShowMissionOptions; // Assign delegate

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
        if (node == null)
        {
            Debug.LogError("Tried to show a null DialogueNode!");
            // Optionally close UI and re-enable movement here as a fallback
            dialogueUI.SetActive(false);
            if (playerMovement != null) playerMovement.SetMovementFreeze(false);
            return;
        }

        currentNode = node;
        dialogueText.text = node.message;
        missionText.text = ""; // Clear mission text unless it's the mission options node

        acceptButton.onClick.RemoveAllListeners(); // Clear previous listeners
        declineButton.onClick.RemoveAllListeners();

        // Configure Accept/First Option Button
        if (node.options != null && node.options.Count >= 1 && node.options[0] != null)
        {
            acceptButton.gameObject.SetActive(true);
            acceptButton.GetComponentInChildren<TMP_Text>().text = node.options[0].text; // Use TMP_Text
            acceptButton.onClick.AddListener(() => // Use lambda to call ShowNode with next node
            {
                PlayRandomDolphinSound(); // Play sound on click
                ShowNode(node.options[0].nextNode);
            });
        }
        else
        {
            acceptButton.gameObject.SetActive(false); // Hide if no first option
        }

        // Configure Decline/Second Option Button
        if (node.options != null && node.options.Count >= 2 && node.options[1] != null)
        {
            declineButton.gameObject.SetActive(true);
            declineButton.GetComponentInChildren<TMP_Text>().text = node.options[1].text; // Use TMP_Text
            declineButton.onClick.AddListener(() => // Use lambda
            {
                PlayRandomDolphinSound(); // Play sound on click
                ShowNode(node.options[1].nextNode);
            });

        }
        else
        {
            declineButton.gameObject.SetActive(false); // Hide if no second option
        }

        // Invoke the node's specific action, if any (like showing mission details)
        node.onSelected?.Invoke();
    }

    // This specific action is assigned to the 'missionNode'
    void ShowMissionOptions()
    {
        missionGiven = true;
        // dialogueText.text = "So... there's a rogue dolphin out there actin' wild. Name's Dilly The Kid. Real slippery."; // Message is already set by ShowNode
        missionText.text = "Take him out, bring back his pearl chain, and you'll earn 100 pearls and some serious Aura.";

        // Buttons are already configured by ShowNode based on the missionNode's state before onSelected is called.
        // We just need to override the *listeners* for the final Accept/Decline actions.
        acceptButton.gameObject.SetActive(true); // Ensure visible
        declineButton.gameObject.SetActive(true); // Ensure visible

        // Override button text for Accept/Decline options
        acceptButton.GetComponentInChildren<TMP_Text>().text = "<color=#4CAF50>Bet, I'm on it</color>";
        declineButton.GetComponentInChildren<TMP_Text>().text = "<color=red>Nah cuzz, I'm good</color>";

        acceptButton.onClick.RemoveAllListeners(); // Remove listener set by ShowNode
        declineButton.onClick.RemoveAllListeners(); // Remove listener set by ShowNode

        acceptButton.onClick.AddListener(AcceptMission);   // Add final AcceptMission action
        declineButton.onClick.AddListener(DeclineMission); // Add final DeclineMission action
    }
}
// ----- END OF CORRECTED CubeInteraction.cs -----

// ----- MAKE SURE these classes are defined elsewhere (e.g., DialogueNode.cs) and NOT here -----
// [System.Serializable]
// public class DialogueNode { ... }
// [System.Serializable]
// public class DialogueOption { ... }
// ----- END OF REMOVED DUPLICATE DEFINITIONS -----