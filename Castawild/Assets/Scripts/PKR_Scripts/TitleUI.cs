using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
        
        public TMP_InputField sessionnameInput;
        public TMP_InputField nicknameInput;
        public Toggle singleToggle;
        private NetworkRunner _runner;
        private static string _shutdownStatus;

        private void OnEnable()
        {
            //닉네임 임시저장.
            var nickname = PlayerTempData.nickname;
            if (string.IsNullOrEmpty(nickname))
            {
                nickname = "Player" + Random.Range(10000, 100000);
            }
            nicknameInput.text = nickname;
            
            //세션네임.
            sessionnameInput.text = "TestRoom";
            
            // Try to load previous shutdown status
            StatusText.text = _shutdownStatus != null ? _shutdownStatus : string.Empty;
            _shutdownStatus = null;
        }
        
        private void Update()
        {
            if (panelGroup.gameObject.activeSelf)
            {
                startGroup.SetActive(_runner == null);
                disconnectGroup.SetActive(_runner != null);

                //Cursor.lockState = CursorLockMode.None;
                //Cursor.visible = true;
            }
            else
            {
                //Cursor.lockState = CursorLockMode.Locked;
                //Cursor.visible = false;
            }
        }
        public async void StartGame()
        {
            await Disconnect();
            PlayerTempData.nickname = nicknameInput.text;
            nicknameInput.interactable = false;
            sessionnameInput.interactable = false;
            _runner = GameObject.Instantiate(RunnerPrefab);
            var events = _runner.GetComponent<NetworkEvents>();
            events.OnShutdown.AddListener(OnShutdown);

            var sceneInfo = new NetworkSceneInfo();
            sceneInfo.AddSceneRef(SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex));

            var startArguments = new StartGameArgs()
            {
                GameMode = singleToggle.isOn ? GameMode.Single : GameMode.AutoHostOrClient,
                Scene = sceneInfo,
                SessionName = sessionnameInput.text,
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