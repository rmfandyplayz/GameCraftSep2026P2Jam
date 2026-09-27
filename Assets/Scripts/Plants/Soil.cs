using UnityEngine;

public class Soil : MonoBehaviour
{
    public enum SoilVariant
    {
        Regular,
        Speed,
        Health,
        Damage
    }

    [Header("Regular Soil")]
    public GameObject seed;
    [SerializeField] Sprite regularSprite;

    [Header("Rare Soils")]
    [SerializeField] GameObject speedSeed;
    [SerializeField] Sprite speedSprite;
    [SerializeField] GameObject healthSeed;
    [SerializeField] Sprite healthSprite;
    [SerializeField] GameObject damageSeed;
    [SerializeField] Sprite damageSprite;

    [Header("References")]
    [SerializeField] SpriteRenderer soilRenderer;

    [Header("UI")]
    public GameObject interactPopup; //andy may replace this, this for now is the game object to indicate we can plant

    //not meant to be touched in inspector but i made this public for testing
    public bool canInteract = true;

    private Player player;
    private GameObject selectedSeed;

    public SoilVariant CurrentVariant { get; private set; }

    private void Awake()
    {
        if (soilRenderer == null)
            soilRenderer = GetComponent<SpriteRenderer>();

        // Existing soil prefabs do not need the regular sprite assigned again.
        if (regularSprite == null && soilRenderer != null)
            regularSprite = soilRenderer.sprite;

        SetVariant(SoilVariant.Regular);
    }

    private void OnEnable()
    {
        if (GameManager.Instance == null)
            return;

        GameManager.Instance.OnDayBegin += HandleDayBegin;
        GameManager.Instance.OnNightBegin += HandleNightBegin;
    }

    private void OnDisable()
    {
        if (GameManager.Instance == null)
            return;

        GameManager.Instance.OnDayBegin -= HandleDayBegin;
        GameManager.Instance.OnNightBegin -= HandleNightBegin;
    }

    private void HandleDayBegin()
    {
        canInteract = true;
    }

    private void HandleNightBegin()
    {
        canInteract = false;
    }

    public void SetVariant(SoilVariant variant)
    {
        CurrentVariant = variant;

        switch (variant)
        {
            case SoilVariant.Speed:
                ApplyRareVariant(speedSeed, speedSprite);
                break;
            case SoilVariant.Health:
                ApplyRareVariant(healthSeed, healthSprite);
                break;
            case SoilVariant.Damage:
                ApplyRareVariant(damageSeed, damageSprite);
                break;
            default:
                selectedSeed = seed;
                SetSprite(regularSprite);
                break;
        }
    }

    private void ApplyRareVariant(GameObject rareSeed, Sprite rareSprite)
    {
        // An incompletely configured rare soil stays usable as regular soil.
        if (rareSeed == null || rareSprite == null)
        {
            CurrentVariant = SoilVariant.Regular;
            selectedSeed = seed;
            SetSprite(regularSprite);
            return;
        }

        selectedSeed = rareSeed;
        SetSprite(rareSprite);
    }

    private void SetSprite(Sprite sprite)
    {
        if (soilRenderer != null)
            soilRenderer.sprite = sprite;
    }

    private void Update()
    {
        if (player == null)
            return;

        bool canPlant = canInteract;

        interactPopup.SetActive(canPlant); //if u can plant has UI pop up

        if (canPlant && player.InteractPressed)
        {
            Instantiate(selectedSeed, transform.position, Quaternion.identity);
            canInteract = false;
            interactPopup.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Player enteringPlayer = other.GetComponent<Player>();

        if (enteringPlayer != null)
            player = enteringPlayer;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Player leavingPlayer = other.GetComponent<Player>();

        if (leavingPlayer == player)
        {
            interactPopup.SetActive(false); //disables ui popup
            player = null;
        }
    }
}
