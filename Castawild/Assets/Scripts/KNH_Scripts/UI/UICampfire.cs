using Fusion;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UICampfire : UIPart
{
    [SerializeField] GameObject uiCanvas;
    UI_Manager uiManager;
    InventoryDataManager inventoryData;
    Campfire campFire;
    public NetworkCampFire netWorkCampfire;
    [Header("슬롯")]
    public Item_Panel cookPot;
    public Item_Panel result;

    public GameObject fireImage;
    [Header("연료")]
    [SerializeField] int selectedFuelIndex = 0;//선택된 연료
    [SerializeField] List<int> fuelList = new List<int>();//연료로 사용 가능한 아이템 ID 리스트
    [SerializeField] Image fuelIcon;

    [Header("시간")]
    public float currentTime;//남은 지속 시간
    float addTime;//더할 시간(나뭇가지 : 20초, 통나무 : 40초)
    int min;
    int sec;
    [SerializeField] TextMeshProUGUI timerText;

    public Image arrowImage;

    void Start()
    {
        uiManager = uiCanvas.GetComponent<UI_Manager>();
        SetFuelIcon();
        timerText.text = 0 + "m " + 0 + "s";
    }

    void Update()
    {
        //나중에 수정
        if (uiManager == null) return;
        if (uiManager.currentCampFire != null)
            campFire = uiManager.currentCampFire.GetComponent<Campfire>();
        if (campFire == null) return;
        netWorkCampfire = campFire.GetComponent<NetworkCampFire>();

        if (!netWorkCampfire.isFire)//불이 꺼져 있을 때
        {
            arrowImage.fillAmount = 0f;
        }
        else if (!netWorkCampfire.isCooking)//요리중이 아닐 때
        {
            arrowImage.fillAmount = 0f;
        }
        //현재 연료 남은 시간
        if(netWorkCampfire.fireTime < 0)
        {
            fireImage.SetActive(false);
        }
    }
    public void AddFuelButton()
    {
        //사운드 재생
        var runner = NetworkRunner.GetRunnerForScene(SceneManager.GetActiveScene());
        SoundManager.Instance.PlayLocalSound3D(runner.LocalPlayer, Sound.UI_ButtonClick, campFire.player.transform.position);

        inventoryData = uiManager.player.GetComponent<InventoryDataManager>();
        int fuelCount = inventoryData.GetItemCount(fuelList[selectedFuelIndex]);//나뭇가지 개수 확인

        if (fuelCount <= 0) return;//연료 부족하면 무시

        inventoryData.UseItem(fuelList[selectedFuelIndex], 1);//연료 사용

        //시간 증가 처리
        if (fuelList[selectedFuelIndex] == 0) addTime = 20;
        else if (fuelList[selectedFuelIndex] == 3) addTime = 40;

        netWorkCampfire.RPC_AddFireTime(addTime);

        //불 켜기
        fireImage.SetActive(true);
        campFire.SetFireActive(true);
    }
    public void LeftButtonClick()
    {
        var runner = NetworkRunner.GetRunnerForScene(SceneManager.GetActiveScene());
        SoundManager.Instance.PlayLocalSound3D(runner.LocalPlayer, Sound.UI_ButtonClick, campFire.player.transform.position);

        if (selectedFuelIndex <= 0) return;
        selectedFuelIndex--;
        SetFuelIcon();
    }

    public void RightButtonClick()
    {
        var runner = NetworkRunner.GetRunnerForScene(SceneManager.GetActiveScene());
        SoundManager.Instance.PlayLocalSound3D(runner.LocalPlayer, Sound.UI_ButtonClick, campFire.player.transform.position);
        if (selectedFuelIndex + 1 >= fuelList.Count) return;
        selectedFuelIndex++;
        SetFuelIcon();
    }

    void SetFuelIcon()
    {
        fuelIcon.sprite = ItemDataBase.Instance.GetItemByID(fuelList[selectedFuelIndex]).image;
    }

    public void SetTimerText(int min, int sec)
    {
        //Debug.Log(min.ToString() + "m " + sec.ToString() + "s");
        timerText.text = min.ToString() + "m " + sec.ToString() + "s";
    }

    public void CookingProgressBar(float timeLeft)
    {
        double totalDuration = 10.0;
        double elapsed = totalDuration - timeLeft;
        float progress = Mathf.Clamp01((float)(elapsed / totalDuration));
        arrowImage.fillAmount = progress;
    }

    public void SetFireIcon(bool tof)
    {
        fireImage.SetActive(tof);
    }
}
