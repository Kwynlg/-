/****************************************************
    文件：UIManager.cs
	作者：Kaiy
    邮箱: 3120713254@qq.com
    日期：#CreateTime#	
	功能：Nothing
*****************************************************/

using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour 
{
    public Image hpPointImage;
    public Image hpPointEffect;

    public EnemyManager enemy;

    private void Awake()
    {
        // 确保EnemyManager实例存在
        if (enemy == null)
        {
            Debug.LogError("EnemyManager instance is not initialized.");
            return;
        }
        // 初始化血量UI
        hpPointImage.fillAmount = enemy.curHp / enemy.maxHp;
        hpPointEffect.fillAmount = hpPointImage.fillAmount; // 初始效果与当前血量一致
    }
    private void Update()
    {
        Debug.Log(enemy.curHp);
        Debug.Log(enemy.maxHp);

        hpPointImage.fillAmount = enemy.curHp / enemy.maxHp;

        if (hpPointEffect.fillAmount > hpPointImage.fillAmount)
        {
            hpPointEffect.fillAmount -= 0.001f; //减速效果
        }
        else
        {
            hpPointEffect.fillAmount = hpPointImage.fillAmount; //保持一致
        }
    }
}