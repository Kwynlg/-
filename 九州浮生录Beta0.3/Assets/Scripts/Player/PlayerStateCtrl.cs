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

    public bool isAttacking = false; //是否在攻击中
    public bool IsAttacking => isAttacking; //只读属性供外部访问

    //动画组件
    private Animator anim;

    public float attackInterval = 0f;
    private float attackDuration = 0.7f; //攻击动画持续时间，可根据动画长度调整

    private void Start()
    {
        anim = transform.GetComponent<Animator>();
    }

    public float GetAtt()
    {
        // 返回玩家的攻击力
        return 1;
    }

    private void Update()
    {
        // 判断当前动画状态是否为技能或攻击动画
        bool isSkillOrAttackAnim = false;
        if (anim != null)
        {
            AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            // 假设所有攻击和技能动画的StateName都以"att"或"sk"或"roll"或"art"开头
            string stateName = stateInfo.IsName("att1") || stateInfo.IsName("att2") ||
                               stateInfo.IsName("sk1") || stateInfo.IsName("sk2") ||
                               stateInfo.IsName("sk3") || stateInfo.IsName("sk4") ||
                               stateInfo.IsName("roll_l") || stateInfo.IsName("roll_r") ||
                               stateInfo.IsName("art") ? stateInfo.shortNameHash.ToString() : "";
            if (!string.IsNullOrEmpty(stateName))
            {
                isSkillOrAttackAnim = true;
            }
        }

        // 动画播放时为攻击状态
        isAttacking = isSkillOrAttackAnim || isAttacking;

        // 攻击状态计时
        if (isAttacking && !isSkillOrAttackAnim)
        {
            attackInterval += Time.deltaTime;
            if (attackInterval >= attackDuration)
            {
                isAttacking = false;
                attackInterval = 0f;
            }
        }

        // 攻击输入
        if (!isAttacking)
        {
            if (Input.GetMouseButtonDown(0))
            {
                anim.SetTrigger("att1");
                StartAttack();
            }
            else if (Input.GetMouseButtonDown(1))
            {
                anim.SetTrigger("att2");
                StartAttack();
            }
        }

        // 技能输入
        if (PlayerSkill())
        {
            StartAttack();
        }
    }

    private void StartAttack()
    {
        isAttacking = true;
        attackInterval = 0f;
    }

    public void PlayerAtt()
    {
        StartAttack();
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