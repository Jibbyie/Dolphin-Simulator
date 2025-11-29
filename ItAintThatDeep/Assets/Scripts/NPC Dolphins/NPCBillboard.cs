using UnityEngine;
using TMPro;

[DisallowMultipleComponent]
public class NPCBillboard : MonoBehaviour
{
    [SerializeField] private Transform target;

    [SerializeField] private float attentionRadius = 10f;
    [SerializeField] private float barkCooldown = 4f;

    private float barkTimer = 0f;
    private bool inAttentionRange = false;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] barkClips;

    [SerializeField] private float barkTextYOffset = 2.0f;
    [SerializeField] private string[] barkLines;

    private TextMeshPro barkTMP;

    [SerializeField] private GameObject minimapPrefab;
    [SerializeField] private float markerYOffset = 2f;
    private Transform markerInstance;

    private DolphinDialogue dialogue;

    private void Awake()
    {
        dialogue = GetComponent<DolphinDialogue>();

        if (target == null)
        {
            var p = FindFirstObjectByType<PlayerHealthController>();
            if (p != null) target = p.transform;
        }

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        barkTMP = new GameObject("BarkText").AddComponent<TextMeshPro>();
        barkTMP.fontSize = 3f;
        barkTMP.alignment = TextAlignmentOptions.Center;
        barkTMP.color = Color.white;
        barkTMP.gameObject.SetActive(false);

        if (minimapPrefab != null)
        {
            GameObject marker = Instantiate(minimapPrefab);
            markerInstance = marker.transform;
            marker.layer = LayerMask.NameToLayer("MiniMap");
            marker.name = $"{gameObject.name}_Marker";
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        float dist = Vector3.Distance(transform.position, target.position);
        inAttentionRange = dist <= attentionRadius;

        if (dialogue != null && dialogue.IsInConversation)
        {
            barkTMP.gameObject.SetActive(false);
            barkTimer = barkCooldown;
            return;
        }

        if (!inAttentionRange)
        {
            barkTMP.gameObject.SetActive(false);
        }
        else
        {
            Vector3 faceDir = target.position - transform.position;
            faceDir.y = 0f;

            if (faceDir.sqrMagnitude > 0.0001f)
            {
                Quaternion lookRot = Quaternion.LookRotation(faceDir.normalized, Vector3.up);
                transform.rotation = lookRot;

                if (markerInstance != null)
                    markerInstance.rotation = lookRot;
            }

            Vector3 pos = transform.position;
            pos.y += barkTextYOffset;
            barkTMP.transform.position = pos;

            barkTMP.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);

            barkTimer -= Time.deltaTime;
            if (barkTimer <= 0f)
            {
                barkTimer = barkCooldown;

                if (barkClips != null && barkClips.Length > 0)
                    audioSource.PlayOneShot(barkClips[Random.Range(0, barkClips.Length)]);

                if (barkLines != null && barkLines.Length > 0)
                {
                    barkTMP.text = barkLines[Random.Range(0, barkLines.Length)];
                    barkTMP.gameObject.SetActive(true);
                }
            }
        }

        if (markerInstance != null)
        {
            Vector3 pos = transform.position;
            pos.y += markerYOffset;
            markerInstance.position = pos;
        }
    }

    private void OnDisable()
    {
        if (barkTMP != null) barkTMP.gameObject.SetActive(false);
        if (markerInstance != null) markerInstance.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (barkTMP != null) Destroy(barkTMP.gameObject);
        if (markerInstance != null) Destroy(markerInstance.gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attentionRadius);
    }
}
