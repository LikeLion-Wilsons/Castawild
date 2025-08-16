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

        [Header("Page0")]
        [SerializeField] GameObject rootPage0;
        [SerializeField] Button btnStart;
        [SerializeField] Button btnOption;
        [SerializeField] Button btnExit;
        //-----

        [Header("Page1")]
        [SerializeField] GameObject rootPage1;
        [SerializeField] TMP_InputField sessionNameInput;
        [SerializeField] TMP_InputField nicknameInput;
        [SerializeField] Toggle singleToggle;
        [SerializeField] Toggle multiToggle;
        [SerializeField] Button btnBack;
        [SerializeField] Button btnOK;
        [SerializeField] TextMeshProUGUI StatusText;
        //-----

        private NetworkRunner _runner;
        private static string _shutdownStatus;

        void Awake()
        {
            //닉네임.
            var nickname = PlayerTempData.nickname;
            if (string.IsNullOrEmpty(nickname))
            {
                nickname = "Player" + Random.Range(10000, 100000);
            }

            nicknameInput.text = nickname;

            //세션.
            sessionNameInput.text = "TestRoom";
            StatusText.text = string.Empty;
            _shutdownStatus = null;

            //Page0.
            SetPage(0);
            btnStart.onClick.AddListener(() => SetPage(1));
            btnExit.onClick.AddListener(OnClickExit);
            singleToggle.onValueChanged.AddListener(_ => OnClickSound());

            //page1.
            btnBack.onClick.AddListener(() => SetPage(0));
            btnOK.onClick.AddListener(StartGame);
        }
        private void Start()
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayBGM(Sound.Env_Title);
            }
        }
        public async void StartGame()
        {
            OnClickSound();
            btnBack.interactable = false;
            btnOK.interactable = false;
            singleToggle.interactable = false;
            multiToggle.interactable = false;
            nicknameInput.interactable = false;
            sessionNameInput.interactable = false;
            PlayerTempData.nickname = nicknameInput.text;

            _runner = GameObject.Instantiate(RunnerPrefab);
            var events = _runner.GetComponent<NetworkEvents>();
            events.OnShutdown.AddListener(OnShutdown);

            var sceneInfo = new NetworkSceneInfo();
            sceneInfo.AddSceneRef(SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex));

            var startArguments = new StartGameArgs()
            {
                GameMode = singleToggle.isOn ? GameMode.Single : GameMode.AutoHostOrClient,
                Scene = sceneInfo,
                SessionName = sessionNameInput.text,
                //PlayerCount = MaxPlayerCount,
            };

            StatusText.text = "Connecting...";


            var task = _runner.StartGame(startArguments);
            await task;

            if (task.Result.Ok)
            {
                StatusText.text = "";
                rootUI.SetActive(false);
                SoundManager.Instance.PlayBGM(Sound.Env_Day);
            }
            else
            {
                Debug.Log($"Connection Failed: {task.Result.ShutdownReason}");
            }
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


        private void OnClickExit()
        {
            OnClickSound();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void SetPage(int page)
        {
            OnClickSound();
            rootPage0.SetActive(page == 0);
            rootPage1.SetActive(page == 1);
        }

        void OnClickSound()
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayLocalSound2D(PlayerRef.None, Sound.UI_ButtonClick);
            }
        }
    }
}