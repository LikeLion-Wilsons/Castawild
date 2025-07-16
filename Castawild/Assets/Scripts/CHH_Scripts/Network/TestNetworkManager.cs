using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TestNetworkManager : MonoBehaviour
{
    [SerializeField] private NetworkRunner runnerPrefab;

    private NetworkRunner runner;

    private async void Start()
    {
        // 현재 씬 정보 확인
        int buildIndex = SceneManager.GetActiveScene().buildIndex;
        string sceneName = SceneManager.GetActiveScene().name;
        Debug.Log($"현재 씬 이름: {sceneName}, 인덱스: {buildIndex}");

        // NetworkRunner 프리팹 인스턴스 생성
        runner = Instantiate(runnerPrefab);
        DontDestroyOnLoad(runner.gameObject);

        // 씬 자동 로딩 매니저 연결
        var sceneManager = runner.gameObject.AddComponent<NetworkSceneManagerDefault>();

        // NetworkSceneInfo 생성 (씬 동기화용)
        var sceneInfo = new NetworkSceneInfo();
        sceneInfo.AddSceneRef(SceneRef.FromIndex(buildIndex));

        // StartGame 설정
        var result = await runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.AutoHostOrClient,
            SessionName = "MyTestSession",
            Scene = sceneInfo,
            SceneManager = sceneManager
        });

        if (!result.Ok)
        {
            Debug.LogError($"StartGame failed: {result.ShutdownReason}");
        }
        else
        {
            Debug.Log("NetworkRunner started");
        }
    }
}
