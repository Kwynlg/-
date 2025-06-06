/****************************************************
    文件：PlayerStateCtrl.cs
	作者：Kaiy
    邮箱: 3120713254@qq.com
    日期：#CreateTime#	
	功能：Nothing
*****************************************************/

using Unity.VisualScripting;
using UnityEngine;


public class PlayerStateCtrl : MonoBehaviour
{
    private Animator anim;

    private void Start()
    {
        anim = transform.GetComponent<Animator>();
    }


    private void Update()
    {
        PlayerSkill();
    }

    public void PlayerSkill()
    {
        //闪避 左
        if (Input.GetKeyUp(KeyCode.Q))
        {
            anim.SetTrigger("roll_l");
            transform.AddComponent<AffterImage3D>();
        }

        //闪避 右
        if (Input.GetKeyUp(KeyCode.E))
        {
            anim.SetTrigger("roll_r");
            transform.AddComponent<AffterImage3D>();
        }

        //技能1
        if (Input.GetKeyUp(KeyCode.Alpha1))
        {
            anim.SetTrigger("sk1");
        }
        //技能2
        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            anim.SetTrigger("sk2");
        }
        //技能3
        if (Input.GetKeyUp(KeyCode.Alpha3))
        {
            anim.SetTrigger("sk3");
        }
        //技能4
        if (Input.GetKeyUp(KeyCode.Alpha4))
        {
            anim.SetTrigger("sk4");
        }
    }
}