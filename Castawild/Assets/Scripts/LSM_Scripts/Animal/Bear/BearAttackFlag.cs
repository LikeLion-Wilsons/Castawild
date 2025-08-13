using UnityEngine;

public class BearAttackFlag : MonoBehaviour
{
    CwBear bearObject;

    private void Awake()
    {
        bearObject = GetComponentInParent<CwBear>();
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Player player = GetComponentInParent<Player>();
            //태이크 데미지 호출
            if (player != null)
            {
                //player.TakeDamage(true, bearObject.Attack); 
            }
        }
    }    
}
