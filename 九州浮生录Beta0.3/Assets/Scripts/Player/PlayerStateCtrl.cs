/****************************************************
    文件：PlayerStateCtrl.cs
	作者：Kaiy
    邮箱: 3120713254@qq.com
    日期：#CreateTime#	
	功能：Nothing
*****************************************************/

using Unity.VisualScripting;
using UnityEngine;


public class PlayerStateCtrl : Singleton<PlayerStateCtrl>
{
    public double Hp = 200; //玩家血量
    public double Mp = 100; //玩家魔法值

    public bool isAtt = false; //是否在攻击中

    //动画组件
    private Animator anim;

    public float Interval = 0f;

    private void Start()
    {
        anim = transform.GetComponent<Animator>();
    }

    private void Update()
    {
        Interval += Time.deltaTime; //增加时间间隔
        if (Interval>1f)
        {
            isAtt = false; // 默认未攻击
            Interval = 0f; // 重置时间间隔
        }

        // 攻击
        if (Input.GetMouseButtonDown(0))
        {
            anim.SetTrigger("att1");
            isAtt = true;
        }
        else if (Input.GetMouseButtonDown(1))
        {
            anim.SetTrigger("att2");
            isAtt = true;
        }

        // 技能
        if (PlayerSkill())
        {
            isAtt = true;
        }
    }

    public void PlayerAtt()
    {
        isAtt = true;
    }

    /// <summary>
    /// 检查是否释放了技能，若释放返回true
    /// </summary>
    public bool PlayerSkill()
    {
        bool skillUsed = false;

        //闪避 左
        if (Input.GetKeyUp(KeyCode.Q))
        {
            anim.SetTrigger("roll_l");
            transform.AddComponent<AffterImage3D>();
            skillUsed = true;
        }

        //闪避 右
        if (Input.GetKeyUp(KeyCode.E))
        {
            anim.SetTrigger("roll_r");
            transform.AddComponent<AffterImage3D>();
            skillUsed = true;
        }

        //轻功
        if (Input.GetKeyUp(KeyCode.P))
        {
            anim.SetTrigger("art");
            skillUsed = true;
        }

        //技能1
        if (Input.GetKeyUp(KeyCode.Alpha1))
        {
            anim.SetTrigger("sk1");
            skillUsed = true;
        }
        //技能2
        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            anim.SetTrigger("sk2");
            skillUsed = true;
        }
        //技能3
        if (Input.GetKeyUp(KeyCode.Alpha3))
        {
            anim.SetTrigger("sk3");
            skillUsed = true;
        }
        //技能4
        if (Input.GetKeyUp(KeyCode.Alpha4))
        {
            anim.SetTrigger("sk4");
            skillUsed = true;
        }

        return skillUsed;
    }
}