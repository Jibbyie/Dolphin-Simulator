using UnityEngine;

public class DolphinDialogue : MonoBehaviour
{
    [SerializeField] private float interactRadius = 3f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private Transform player;

    [SerializeField] private DialogueNode[] conversation;

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite positiveSprite;
    [SerializeField] private Sprite negativeSprite;

    [SerializeField] private Sprite portraitDefault;
    [SerializeField] private Sprite portraitPositive;
    [SerializeField] private Sprite portraitNegative;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] talkClips;

    [SerializeField] private bool oneShot = false;

    private bool conversationCompleted = false;
    private bool inConversation = false;
    private int currentNode = 0;

    public bool IsInConversation => inConversation;

    private void Awake()
    {
        if (player == null)
        {
            var healthCtrl = FindFirstObjectByType<PlayerHealthController>();
            if (healthCtrl != null)
                player = healthCtrl.transform;
        }

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (player == null) return;

        float dist = Vector3.Distance(player.position, transform.position);

        if (inConversation && dist > interactRadius)
        {
            ForceEnd();
            return;
        }

        if (conversationCompleted && oneShot)
            return;

        if (dist > interactRadius) return;

        if (Input.GetKeyDown(interactKey) && !inConversation)
            BeginConversation();
    }

    private void BeginConversation()
    {
        if (conversation == null || conversation.Length == 0)
            return;

        inConversation = true;
        currentNode = 0;
        ShowNode();
    }

    public void ShowNode()
    {
        var node = conversation[currentNode];

        SetEmotionSprite(node.emotion);
        PlayTalkSFX();

        DialogueUIController.Instance.ShowNode(
            this,
            name,
            node.npcLine,
            node.options,
            node.emotion
        );
    }

    public void ChooseOption(DialogueOption option)
    {
        if (option.nextNodeIndex < 0)
        {
            if (option.givesAura)
                AuraPointsManager.AddBonusPoints(5, transform.position, "QUIP");

            if (option.givesAmmo)
                GiveAmmoToAllWeapons(5);

            conversationCompleted = true;
            EndConversation();
            return;
        }

        currentNode = option.nextNodeIndex;
        ShowNode();
    }

    private void EndConversation()
    {
        inConversation = false;

        if (spriteRenderer != null)
            spriteRenderer.sprite = defaultSprite;

        DialogueUIController.Instance.CloseDialogue();
    }

    public void ForceEnd()
    {
        inConversation = false;

        if (spriteRenderer != null)
            spriteRenderer.sprite = defaultSprite;

        DialogueUIController.Instance.CloseDialogue();
    }

    private void GiveAmmoToAllWeapons(int amount)
    {
        var shooter = FindFirstObjectByType<FirstPersonShooter>();
        if (shooter == null) return;

        var wm = FindFirstObjectByType<WeaponManager>();
        if (wm == null) return;

        var weapons = wm.GetAllWeapons();
        if (weapons == null) return;

        for (int i = 0; i < weapons.Count; i++)
        {
            var weapon = weapons[i];
            if (!FirstPersonShooter.IsRangedWeapon(weapon.weaponType))
                continue;

            shooter.AddReserveAmmoToWeapon(weapon, amount);
        }
    }

    public Sprite GetPortraitForEmotion(DialogueEmotion emotion)
    {
        switch (emotion)
        {
            case DialogueEmotion.Positive: return portraitPositive ? portraitPositive : portraitDefault;
            case DialogueEmotion.Negative: return portraitNegative ? portraitNegative : portraitDefault;
            default: return portraitDefault;
        }
    }

    private void SetEmotionSprite(DialogueEmotion emotion)
    {
        if (spriteRenderer == null) return;

        switch (emotion)
        {
            case DialogueEmotion.Positive:
                spriteRenderer.sprite = positiveSprite ? positiveSprite : defaultSprite;
                break;
            case DialogueEmotion.Negative:
                spriteRenderer.sprite = negativeSprite ? negativeSprite : defaultSprite;
                break;
            default:
                spriteRenderer.sprite = defaultSprite;
                break;
        }
    }

    private void PlayTalkSFX()
    {
        if (audioSource == null) return;
        if (talkClips == null || talkClips.Length == 0) return;

        var clip = talkClips[Random.Range(0, talkClips.Length)];
        if (clip != null)
            audioSource.PlayOneShot(clip);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
