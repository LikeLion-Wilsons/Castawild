using UnityEngine;

public class ThrowObject : MonoBehaviour
{
    private Vector3 targetPos;
    bool isThrown = false;

    // target X 좌표에 도달하면 x방향 힘 제거
    private void Update()
    {
        //if (transform.position.x - targetPos.x <= 0.01f && GetComponent<Rigidbody>() != null)
        //{
        //    Rigidbody rigid = GetComponent<Rigidbody>();
        //    Vector3 vec = rigid.linearVelocity;

        //    vec.x = 0f;
        //    rigid.linearVelocity = vec;
        //}
    }

    public void Throw(CwPlayer player)
    {
        if (GetComponent<Rigidbody>() != null)
            return;

        Rigidbody rigid = gameObject.AddComponent<Rigidbody>();
        transform.parent = null;

        // 화면 중앙 → 월드 좌표의 타겟 지점 구하기
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        Ray ray = Camera.main.ScreenPointToRay(screenCenter);

        // 화면 중앙 향하는 방향
        targetPos = ray.GetPoint(30f);
        Vector3 direction = (targetPos - transform.position).normalized;
        direction.y = 0f;
        direction += Vector3.up * player.throwUpForce;

        rigid.AddForce(direction.normalized * player.throwForce, ForceMode.Impulse);
    }
}