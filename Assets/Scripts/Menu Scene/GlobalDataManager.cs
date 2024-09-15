using ScriptableObjects;
using UnityEngine;

public class GlobalDataManager : MonoBehaviour
{
    [field: SerializeField]
    public CarProfile[] carProfiles { get; private set; }

    public static GlobalDataManager Instance;
    public bool initialized { get; private set; }

    [HideInInspector]
    public int carProfileIndex;

    [HideInInspector]
    public bool spectatorMode;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Instance = this;
    }
}
