/****************************************************
    文件：OldWuLing.cs
	作者：Kaiy
    邮箱: 3120713254@qq.com
    日期：#CreateTime#	
	功能：Nothing
*****************************************************/

using UnityEngine;

public class OldWuLing : MonoBehaviour
{
    public int id = 1; // 对话ID

    public string _name = "OldWuLing"; // 角色名
    public string ThisName = "武林尊者"; // 角色显示名

    public bool isHas = false;
    public AudioClip[] audioClip; // 对话音频

    private void Update()
    {
        if (id == 2 && isHas && Input.GetKeyUp(KeyCode.F))
        {
            //Debug.Log("玩家进入范围"+id);
            DialogueEvents();
            id++;
        }
        else if (id == 3 && isHas && Input.GetKeyUp(KeyCode.F))
        {
            id = 3;
            DialogueManager.Instance.ShowDialogue(_name, id, audioClip[id-1], ThisName); // 显示对话内容
            GlobalObj.Instance._Panel_Dialogue.SetActive(true);
            GlobalObj.Instance._Panel_Tips.SetActive(false);
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!isHas)
            {
                if (id == 1)
                {
                    Debug.Log("玩家进入范围"+id);
                    DialogueEvents();
                    id++;
                }
            }
            else if (id == 3)
            {
                id = 3;
                DialogueEvents();
            }
            isHas = true;

        }
    }

    public void DialogueEvents()
    {
        isHas = true; // 设置对话已触发
        DialogueManager.Instance.ShowDialogue(_name, id, audioClip[id-1], ThisName); // 显示对话内容
        GlobalObj.Instance._Panel_Dialogue.SetActive(true);
        GlobalObj.Instance._Panel_Tips.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        isHas = false;
        GlobalObj.Instance._Panel_Dialogue.SetActive(false);
        GlobalObj.Instance._Panel_Tips.SetActive(false);
    }
}