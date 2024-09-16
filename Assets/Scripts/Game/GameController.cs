
using Cinemachine;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum GameState
{
    GAME_INIT,
    GAME_COUNTDOWN,
    GAME_STARTED_PLAYER_MODE,
    GAME_STARTED_SPECTATOR_MODE,
}

public class GameController : MonoBehaviour
{
    [SerializeField]
    private CarController _prefabCar;

    [SerializeField]
    private CinemachineVirtualCamera _camera;

    [SerializeField]
    private Transform[] _vehicleStartingLocations;

    [SerializeField]
    private Material _originalPaintMaterial;

    [field: SerializeField]
    public int maxLapCount { get; private set; }

    public static GameController Instance;
    public CarController playerCar { get; private set; }
    public System.Random Random { get; private set; }

    private int _focusIndex;
    public int focusIndex
    {
        get => _focusIndex;
        set
        {
            _focusIndex = Mathf.Clamp(value, 0, _cars.Count - 1);
            for (int i = 0; i < _cars.Count; ++i)
            {
                _cars[i].hasFocus = _focusIndex == i;
            }
            _camera.Follow = _cars[_focusIndex].transform;
            _camera.LookAt = _cars[_focusIndex].transform;
            GameUIController.Instance.carSessionData = _cars[_focusIndex].sessionData;
        }
    }

    private CheckpointController[] _checkpoints;
    private PlayerInput _playerInput;
    private InputAction _actionSelectPrev;
    private InputAction _actionSelectNext;
    public GameState gameState { get; private set; }

    public float startTime { get; private set; }
    private int _countDown;

    private List<CarController> _cars;

    public GameController() : base()
    {
        maxLapCount = 3;
    }

    private void Awake()
    {
        Instance = this;
        Random = new System.Random(Guid.NewGuid().GetHashCode());
        _checkpoints = GetComponentsInChildren<CheckpointController>();
        for (int i = 0; i < _checkpoints.Length; ++i)
        {
            _checkpoints[i].checkpointId = i;
        }

        _playerInput = GetComponent<PlayerInput>();
        _actionSelectPrev = _playerInput.actions["Prev"];
        _actionSelectNext = _playerInput.actions["Next"];
    }
    /*
        private CarController createNewCar(VehicleTemplate template)
        {
            Material newMaterial = Instantiate(_originalPaintMaterial);
            newMaterial.color = template.color;

            CarController newCar = Instantiate(_prefabCar);
            newCar.transform.position = template.position.position;
            foreach (MeshRenderer meshRenderer in newCar.GetComponentsInChildren<MeshRenderer>(true))
            {
                List<Material> materials = new List<Material>();
                meshRenderer.GetMaterials(materials);
                for (int i = 0; i < materials.Count; ++i) {
                    if (materials[i].name.Contains(_originalPaintMaterial.name))
                    {
                        materials[i] = newMaterial;
                    }
                }
                meshRenderer.SetMaterials(materials);
            }
            newCar.gameObject.SetActive(true);
            return newCar;
        }
    */
    private CarController createNewCar(Vector3 position, int index)
    {
        CarController newCar = Instantiate(_prefabCar);
        newCar.transform.position = position;
        newCar.gameObject.SetActive(true);
        newCar.carProfile = GlobalDataManager.Instance.carProfiles[
            Random.Next(GlobalDataManager.Instance.carProfiles.Length)];
        newCar.isPlayer = false;
        newCar.isAI = true;
        newCar.sessionData.rank = index;
        newCar.bodyMeshRenderer.material.color = new Color(0f, 1f, 1f, 1f);
        return newCar;
    }

    private void Start()
    {
        if (GlobalDataManager.Instance == null)
        {
            SceneManager.LoadScene(0);
            return;
        }

        _cars = new List<CarController>();
        for (int i = 0; i < _vehicleStartingLocations.Length; ++i)
        {
            _cars.Add(createNewCar(_vehicleStartingLocations[i].position, i));
        }

        if (GlobalDataManager.Instance.spectatorMode)
        {
            focusIndex = 0;
        }
        else
        {
            int playerIndex = _cars.Count - 1;
            playerCar = _cars[playerIndex];

            playerCar.carProfile = GlobalDataManager.Instance.carProfiles[
                GlobalDataManager.Instance.carProfileIndex];
            playerCar.isPlayer = true;
            playerCar.isAI = false;
            playerCar.bodyMeshRenderer.material.color = Color.white;

            focusIndex = playerIndex;
        }

        gameState = GameState.GAME_INIT;
    }

    void Update()
    {
        if (GlobalDataManager.Instance == null)
        {
            return;
        }

        switch (gameState)
        {
            case GameState.GAME_INIT:
                if (_cars.TrueForAll(c => c.sessionData != null))
                {
                    GameUIController.Instance.enabled = true;
                    startTime = Time.time;
                    _countDown = 3;
                    GameUIController.Instance.SetCountdown(_countDown);
                    gameState = GameState.GAME_COUNTDOWN;
                }
                break;

            case GameState.GAME_COUNTDOWN:
                if (Time.time - startTime > 1)
                {
                    --_countDown;
                    if (_countDown == 0)
                    {
                        foreach (CarController car in _cars)
                        {
                            car.NextCheckpoint(_checkpoints[0]);
                            car.enabled = true;
                        }
                        if (GlobalDataManager.Instance.spectatorMode)
                        {
                            gameState = GameState.GAME_STARTED_SPECTATOR_MODE;
                        }
                        else
                        {
                            gameState = GameState.GAME_STARTED_PLAYER_MODE;
                        }
                        GameUIController.Instance.StartTimerDisplay();
                    }
                    startTime = Time.time;
                    GameUIController.Instance.SetCountdown(_countDown);
                }
                break;

            case GameState.GAME_STARTED_PLAYER_MODE:
                if (playerCar.sessionData.lapCounter >= maxLapCount)
                {
                    GameUIController.Instance.ShowGameOverMessageBoxAsPlayer();
                }
                break;

            case GameState.GAME_STARTED_SPECTATOR_MODE:
                if (_actionSelectNext.triggered)
                {
                    focusIndex = (focusIndex + 1) % _cars.Count;
                }
                if (_actionSelectPrev.triggered)
                {
                    focusIndex = (focusIndex + _cars.Count - 1) % _cars.Count;
                }
                if (_cars.TrueForAll(c => c.sessionData.lapCounter >= maxLapCount))
                {
                    GameUIController.Instance.ShowGameOverMessageBoxAsPlayer();
                }
                break;

            default:
                break;
        }
    }

    public void OnCheckpoint(CarController car, int checkpointIndex, Vector3 prevContactPoint)
    {
        bool nextCheckpoint = false;
        int previousIndex = checkpointIndex - 1 + (checkpointIndex == 0 ? _checkpoints.Length : 0);

        if (car.sessionData.checkpointCounter < 0 && checkpointIndex == 0)
        {
            nextCheckpoint = true;
            car.sessionData.checkpointCounter = 0;
        }
        else if (car.sessionData.checkpointCounter == previousIndex)
        {
            nextCheckpoint = true;
            car.sessionData.checkpointCounter = (car.sessionData.checkpointCounter + 1) % _checkpoints.Length;
            if (car.sessionData.checkpointCounter == 0)
            {
                car.sessionData.lapCounter++;
            }
        }

        if (nextCheckpoint)
        {
            car.sessionData.lastCheckpointTime = Time.time;
            car.NextCheckpoint(_checkpoints[(checkpointIndex + 1) % _checkpoints.Length], prevContactPoint);

            List<CarController> sortedCars = new List<CarController>(_cars);
            sortedCars.Sort(compareCarRank);
            for (int i = 0; i < sortedCars.Count; ++i)
            {
                sortedCars[i].sessionData.rank = i;
            }
        }
    }

    private int compareCarRank(CarController car1, CarController car2)
    {
        CarSessionData data1 = car1.sessionData;
        CarSessionData data2 = car2.sessionData;
        if (data1.lapCounter != data2.lapCounter)
        {
            return data2.lapCounter.CompareTo(data1.lapCounter);
        }
        else if (data1.checkpointCounter != data2.checkpointCounter)
        {
            return data2.checkpointCounter.CompareTo(data1.checkpointCounter);
        }
        else
        {
            return data1.lastCheckpointTime.CompareTo(data2.lastCheckpointTime);
        }
    }

    public void RestartGame()
    {
        GameUIController.Instance.ShowMessageBox(
            "Restart the game?",
            () => SceneManager.LoadScene(1));
    }

    public void QuitGame()
    {
        GameUIController.Instance.ShowMessageBox(
            "Quit the game?",
            () => SceneManager.LoadScene(0));
    }
}
