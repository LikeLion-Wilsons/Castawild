using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Test
{
    public class TitleUI : MonoBehaviour
    {
        public NetworkRunner RunnerPrefab;
        public GameObject rootUI;
        public TextMeshProUGUI StatusText;
        public GameObject startGroup;
        public GameObject disconnectGroup;
        public CanvasGroup panelGroup;
        private NetworkRunner _runner;
        private static string _shutdownStatus;
        public async void StartGame()
        {
            await Disconnect();
            
            _runner = GameObject.Instantiate(RunnerPrefab);
            var events = _runner.GetComponent<NetworkEvents>();
            events.OnShutdown.AddListener(OnShutdown);

            var sceneInfo = new NetworkSceneInfo();
            sceneInfo.AddSceneRef(SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex));

            var startArguments = new StartGameArgs()
            {
                GameMode = GameMode.AutoHostOrClient, //Single이 있.네?. 테스트필요.
                Scene = sceneInfo,
                //SessionName = "TestRoom",
                //PlayerCount = MaxPlayerCount,
            };
            
            StatusText.text = "Connecting...";


            var task = _runner.StartGame(startArguments);
            await task;
            
            if (task.Result.Ok)
            {
                StatusText.text = "";
                rootUI.SetActive(false);
            }
            else
            {
                Debug.Log($"Connection Failed: {task.Result.ShutdownReason}");
            }
        }
        public async void DisconnectClicked()
        {
            await Disconnect();
        }
        
        private void Update()
        {
            if (panelGroup.gameObject.activeSelf)
            {
                startGroup.SetActive(_runner == null);
                disconnectGroup.SetActive(_runner != null);

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
        private void OnEnable()
        {

            // Try to load previous shutdown status
            StatusText.text = _shutdownStatus != null ? _shutdownStatus : string.Empty;
            _shutdownStatus = null;
        }
        public async Task Disconnect()
        {
            if (_runner == null)
                return;

            StatusText.text = "Disconnecting...";
            panelGroup.interactable = false;

            // Remove shutdown listener since we are disconnecting deliberately
            var events = _runner.GetComponent<NetworkEvents>();
            events.OnShutdown.RemoveListener(OnShutdown);

            await _runner.Shutdown();
            _runner = null;

            // Reset of scene network objects is needed, reload the whole scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        private void OnShutdown(NetworkRunner runner, ShutdownReason reason)
        {
            // Unexpected shutdown happened (e.g. Host disconnected)

            // Save status into static variable, it will be used in OnEnable after scene load
            _shutdownStatus = $"Shutdown: {reason}";
            Debug.LogWarning(_shutdownStatus);

            // Reset of scene network objects is needed, reload the whole scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}