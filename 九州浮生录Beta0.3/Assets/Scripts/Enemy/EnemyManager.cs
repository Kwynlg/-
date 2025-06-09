/****************************************************
    文件：EnemyManager.cs
	作者：Kaiy
    邮箱: 3120713254@qq.com
    日期：#CreateTime#	
	功能：Nothing
*****************************************************/

using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public GameObject[] bloods; //受击血液
    public GameObject audioHit;

    public bool isHit = false; //是否受击

    private void OnTriggerEnter(Collider other)
    {
        if (!isHit && other.CompareTag("Weapon"))
        {
            if (PlayerStateCtrl.Instance.isAtt)
            {
                Debug.Log("受击");
                isHit = true;

                // 获取碰撞点
                Vector3 hitPoint = other.ClosestPointOnBounds(transform.position);
                // 如果碰撞体有ContactPoint信息，优先用ContactPoint
                if (other is BoxCollider || other is SphereCollider || other is CapsuleCollider)
                {
                    hitPoint = other.ClosestPoint(transform.position);
                }

                GameObject go = Instantiate(
                    bloods[Random.Range(0, bloods.Length)],
                    hitPoint,
                    Quaternion.identity
                );
                Destroy(go, 3.5f);

                GameObject hitaudio = Instantiate(
                    audioHit,
                    hitPoint,
                    Quaternion.identity
                );
                Destroy(hitaudio, 3.5f);
            }
        } 
    }

    private void OnTriggerExit(Collider other)
    {
        isHit = false;
    }
}