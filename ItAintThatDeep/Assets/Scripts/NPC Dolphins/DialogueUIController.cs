using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueUIController : MonoBehaviour
{
    public static DialogueUIController Instance;

    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI npcNameText;
    [SerializeField] private TextMeshProUGUI npcLineText;
    [SerializeField] private Image portraitImage;

    [SerializeField] private Button option1;
    [SerializeField] private Button option2;
    [SerializeField] private Button option3;

    private TextMeshProUGUI[] optionTexts;
    private DolphinDialogue activeDolphin;
    private bool isOpen = false;

    private void Awake()
    {
        Instance = this;
        dialoguePanel.SetActive(false);

        optionTexts = new TextMeshProUGUI[]
        {
            option1.GetComponentInChildren<TextMeshProUGUI>(),
            option2.GetComponentInChildren<TextMeshProUGUI>(),
            option3.GetComponentInChildren<TextMeshProUGUI>()
        };
    }

    private void Update()
    {
        if (!isOpen || activeDolphin == null)
            return;

        if (Input.GetKeyDown(KeyCode.Z))
            option1.onClick.Invoke();

        if (Input.GetKeyDown(KeyCode.X))
            option2.onClick.Invoke();

        if (Input.GetKeyDown(KeyCode.C))
            option3.onClick.Invoke();

        if (Input.GetKeyDown(KeyCode.Escape))
            activeDolphin.ForceEnd();
    }

    public void ShowNode(
        DolphinDialogue dolphin,
        string npcName,
        string npcLine,
        DialogueOption[] options,
        DialogueEmotion emotion)
    {
        activeDolphin = dolphin;

        npcNameText.text = npcName;
        npcLineText.text = npcLine;

        if (portraitImage != null)
            portraitImage.sprite = dolphin.GetPortraitForEmotion(emotion);

        dialoguePanel.SetActive(true);
        isOpen = true;

        option1.gameObject.SetActive(false);
        option2.gameObject.SetActive(false);
        option3.gameObject.SetActive(false);

        for (int i = 0; i < options.Length; i++)
        {
            var b = i == 0 ? option1 : i == 1 ? option2 : option3;
            var t = optionTexts[i];

            string keyLabel = i == 0 ? "[Z] " : i == 1 ? "[X] " : "[C] ";

            b.gameObject.SetActive(true);
            t.text = keyLabel + options[i].playerLine;

            int index = i;
            b.onClick.RemoveAllListeners();
            b.onClick.AddListener(() =>
            {
                activeDolphin.ChooseOption(options[index]);
            });
        }
    }

    public void CloseDialogue()
    {
        dialoguePanel.SetActive(false);
        activeDolphin = null;
        isOpen = false;
    }
}
