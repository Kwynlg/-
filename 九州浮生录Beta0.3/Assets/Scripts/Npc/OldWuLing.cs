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
    [Header("对话参数")]
    [Tooltip("对话ID，初始为1")]
    public int id = 1;
    [Tooltip("角色英文名")]
    public string _name = "OldWuLing";
    [Tooltip("角色显示名")]
    public string ThisName = "武林尊者";
    [Tooltip("对话音频")]
    public AudioClip[] audioClip;

    private bool isHas = false; // 是否已触发对话
    private bool playerInRange = false; // 玩家是否在范围内

    private const int MaxDialogueId = 3; // 最大对话ID

    private void Update()
    {
        // 仅在玩家范围内且对话已激活时检测输入
        if (playerInRange && isHas && CanProceedToNextDialogue())
        {
            ProceedToNextDialogue();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other)) return;

        playerInRange = true;

        // 仅在未触发对话且为特定ID时启动对话
        if (!isHas && (id == 1 || id == MaxDialogueId))
        {
            Debug.Log($"玩家进入范围，当前对话ID: {id}");
            StartDialogue();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other)) return;

        playerInRange = false;
        ResetDialogueState();
        HideDialogueUI();
    }

    /// <summary>
    /// 判断是否为玩家
    /// </summary>
    private bool IsPlayer(Collider other)
    {
        return other.CompareTag("Player");
    }

    /// <summary>
    /// 启动对话
    /// </summary>
    private void StartDialogue()
    {
        isHas = true;
        ShowDialogue();
    }

    /// <summary>
    /// 检查是否可以进入下一句对话
    /// </summary>
    private bool CanProceedToNextDialogue()
    {
        // 假设AudioManager.Instance.PlayAudio()返回true表示音频播放完毕
        return AudioManager.Instance.PlayAudio() || Input.GetKeyDown(KeyCode.F);
    }

    /// <summary>
    /// 进入下一句对话
    /// </summary>
    private void ProceedToNextDialogue()
    {
        id = Mathf.Min(id + 1, MaxDialogueId);
        ShowDialogue();
    }

    /// <summary>
    /// 显示对话内容和UI
    /// </summary>
    private void ShowDialogue()
    {
        int clipIndex = Mathf.Clamp(id - 1, 0, audioClip.Length - 1);
        DialogueManager.Instance.ShowDialogue(_name, id, audioClip[clipIndex], ThisName);
        ShowDialogueUI();
    }

    /// <summary>
    /// 显示对话相关UI
    /// </summary>
    private void ShowDialogueUI()
    {
        GlobalObj.Instance._Panel_Dialogue.SetActive(true);
        GlobalObj.Instance._Panel_Tips.SetActive(true);
    }

    /// <summary>
    /// 隐藏对话相关UI
    /// </summary>
    private void HideDialogueUI()
    {
        GlobalObj.Instance._Panel_Dialogue.SetActive(false);
        GlobalObj.Instance._Panel_Tips.SetActive(false);
    }

    /// <summary>
    /// 重置对话状态
    /// </summary>
    private void ResetDialogueState()
    {
        isHas = false;
    }
}