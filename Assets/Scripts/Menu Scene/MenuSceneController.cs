
using Cinemachine;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuSceneController : MonoBehaviour
{
    [SerializeField]
    private Transform _car;

    [SerializeField]
    private float _carRotationSpeed = 30;

    [SerializeField]
    private string _sceneLevel;

    public static MenuSceneController Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        _car.Rotate(Vector3.up, 30 * Time.deltaTime);
    }

    public void StartGame()
    {
        GlobalDataManager.Instance.spectatorMode = false;
        SceneManager.LoadScene(1);
    }

    private void onSpectateGame()
    {
        GlobalDataManager.Instance.spectatorMode = true;
        SceneManager.LoadScene(1);
    }

    public void SpectateGame()
    {
        MenuSceneUIController.Instance.ShowMessageBox(
            "Spectate game?", onSpectateGame);
    }
}
