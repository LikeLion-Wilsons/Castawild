using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TestNetworkManager : MonoBehaviour
{
    [SerializeField] private NetworkRunner runnerPrefab;

    private NetworkRunner runner;

    private async void Start()
    {
        // NetworkRunner 프리팹 인스턴스 생성
        runner = Instantiate(runnerPrefab);
        DontDestroyOnLoad(runner.gameObject);

        // 씬 로딩 자동 관리 설정
        var sceneManager = runner.gameObject.AddComponent<NetworkSceneManagerDefault>();

        // 게임 시작
        var result = await runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.AutoHostOrClient,
            SessionName = "MyTestSession",
            Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex),
            SceneManager = sceneManager
        });

        if (!result.Ok)
            Debug.LogError($"StartGame failed: {result.ShutdownReason}");
        else
            Debug.Log("NetworkRunner started");
    }
}
