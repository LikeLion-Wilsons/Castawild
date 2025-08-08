using System; // Action을 사용하기 위해 필요

public static class WeatherEvents
{
    // 비 상태가 변경될 때 호출될 이벤트입니다.
    // bool 인자는 비가 오는지(true) 아닌지(false)를 전달합니다.
    public static Action<bool> OnRainStateChanged;
}