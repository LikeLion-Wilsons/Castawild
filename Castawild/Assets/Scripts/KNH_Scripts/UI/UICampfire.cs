using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICampfire : UIPart
{
    [SerializeField] GameObject uiCanvas;
    UI_Manager uiManager;
    InventoryDataManager inventoryData;
    Campfire campFire;

    [SerializeField] GameObject fireImage;
    [Header("연료")]
    [SerializeField] int selectedFuelIndex = 0;//선택된 연료
    [SerializeField] List<int> fuelList = new List<int>();//연료로 사용 가능한 아이템 ID 리스트
    [SerializeField] Image fuelIcon;

    [Header("시간")]
    float currentTime;//남은 지속 시간
    float addTime;//더할 시간(나뭇가지 : 20초, 통나무 : 40초)
    int min;
    int sec;
    [SerializeField] TextMeshProUGUI timerText;

    void Start()
    {
        uiManager = uiCanvas.GetComponent<UI_Manager>();
        SetFuelIcon();
    }

    void Update()
    {
        if (uiManager == null) return;
        if (uiManager.currentCampFire != null)
            campFire = uiManager.currentCampFire.GetComponent<Campfire>();
        if (campFire == null) return;

        //타이머 네트워크 동기화 필요(UI 닫혀있을 때에도 타이머 감소)
        if (currentTime > 0)
        {
            currentTime -= Time.deltaTime;

            SetTimerText();
        }
        else
        {
            campFire.SetFireActive(false);
            fireImage.SetActive(false);
        }
    }
    public void AddFuelButton()
    {
        inventoryData = uiManager.player.GetComponent<InventoryDataManager>();
        int fuelCount = inventoryData.GetItemCount(fuelList[selectedFuelIndex]);//나뭇가지 개수 확인

        if (fuelCount <= 0) return;//연료 부족하면 무시

        inventoryData.UseItem(fuelList[selectedFuelIndex], 1);//연료 사용

        //시간 증가 처리
        if (fuelList[selectedFuelIndex] == 0) addTime = 20;
        else if (fuelList[selectedFuelIndex] == 3) addTime = 40;

        AddTime(addTime);

        //불 켜기
        fireImage.SetActive(true);
        campFire.SetFireActive(true);
    }
    public void LeftButtonClick()
    {
        if (selectedFuelIndex <= 0) return;
        selectedFuelIndex--;
        SetFuelIcon();
    }

    public void RightButtonClick()
    {
        if (selectedFuelIndex + 1 >= fuelList.Count) return;
        selectedFuelIndex++;
        SetFuelIcon();
    }

    void SetFuelIcon()
    {
        fuelIcon.sprite = ItemDataBase.Instance.GetItemByID(fuelList[selectedFuelIndex]).image;
    }

    void AddTime(float time)
    {
        currentTime += time;
    }

    void SetTimerText()
    {
        min = (int)currentTime / 60;
        sec = (int)currentTime % 60;
        timerText.text = min.ToString() + "m " + sec.ToString() + "s";
    }
}
