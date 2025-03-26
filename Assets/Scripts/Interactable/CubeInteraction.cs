using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CubeInteraction : MonoBehaviour
{
    public GameObject Enemy;


    public GameObject dialogueUI;
    public TMP_Text dialogueText;
    public TMP_Text missionText;

    public Button acceptButton;
    public Button declineButton;

    private bool playerInRange = false;
    private bool missionGiven = false;

    void Start()
    {
        dialogueUI.SetActive(false);
        acceptButton.onClick.AddListener(AcceptMission);
        declineButton.onClick.AddListener(DeclineMission);

        acceptButton.gameObject.SetActive(false);
        declineButton.gameObject.SetActive(false);
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            dialogueUI.SetActive(true);

            if (SimpleQuestManager.Instance.enemyDefeated && SimpleQuestManager.Instance.missionAccepted && !SimpleQuestManager.Instance.rewardClaimed)
            {
                dialogueText.text = "Good work. The reef owes you one.";
                missionText.text = "";
                SimpleQuestManager.Instance.rewardClaimed = true;
                acceptButton.gameObject.SetActive(false);
                declineButton.gameObject.SetActive(false);
                return;
            }

            if (!missionGiven)
            {
                dialogueText.text = "Yo. I got a job for you...";
                missionText.text = "Take down that rogue red cube. He's been wildin'.";
                acceptButton.gameObject.SetActive(true);
                declineButton.gameObject.SetActive(true);
                missionGiven = true;
            }
        }

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            dialogueUI.SetActive(false);
        }
    }

    void AcceptMission()
    {
        dialogueText.text = "Aight. Go deal with him.";
        SimpleQuestManager.Instance.missionAccepted = true;

        if (Enemy != null)
        {
            Enemy.SetActive(true);
        }

        acceptButton.gameObject.SetActive(false);
        declineButton.gameObject.SetActive(false);
        missionText.text = "";

        StartCoroutine(CloseDialogueAfterDelay(2f));
    }



    void DeclineMission()
    {
        dialogueText.text = "Suit yourself...";
        CleanupMissionUI();
    }

    void CleanupMissionUI()
    {
        acceptButton.gameObject.SetActive(false);
        declineButton.gameObject.SetActive(false);
        missionText.text = "";
    }

    IEnumerator CloseDialogueAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        dialogueUI.SetActive(false);
    }

}
