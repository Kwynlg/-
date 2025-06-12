using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueManager : Singleton<DialogueManager>
{
    // 对话文本
    public TMP_Text txtBody;

    // 对话音频
    public AudioSource audioSource;

    // 存储的文本数据（角色→(ID→内容)）
    public Dictionary<string, Dictionary<int, string>> DialogueDic = new Dictionary<string, Dictionary<int, string>>();

    private void Start()
    {
        InitDialogueContent();
    }

    /// <summary>
    /// 显示对话内容
    /// </summary>
    /// <param name="name">角色名</param>
    /// <param name="id">对话ID</param>
    public void ShowDialogue(string name, int id, AudioClip clips, string ThisName)
    {
        if (DialogueDic.TryGetValue(name, out Dictionary<int, string> idMap))
        {
            if (idMap.TryGetValue(id, out string content))
            {
                txtBody.text = $"{ThisName}: {content}";
                AudioManager.Instance.audioSource = audioSource; // 确保 AudioManager 的 audioSource 被设置
                audioSource.clip = clips;
                audioSource.Play();
                return;
            }
            Debug.LogWarning($"角色 '{name}' 中不存在 ID 为 {id} 的对话");
        }
        else
        {
            Debug.LogWarning($"未找到角色 '{name}' 的对话数据");
            foreach (var character in DialogueDic.Keys)
            {
                Debug.Log($"已加载角色：{character}");
            }
        }
    }

    /// <summary>
    /// 初始表内容
    /// </summary>
    public void InitDialogueContent()
    {
        TextAsset textAsset = Resources.Load<TextAsset>("对话数据表");
        if (textAsset == null)
        {
            Debug.LogError("未找到对话数据表资源");
            return;
        }

        string[] lines = textAsset.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            // 只分割前两个逗号，剩下的都作为内容
            string[] fields = line.Split(new[] { ',' }, 3);
            if (fields.Length < 3)
            {
                Debug.LogWarning($"无效的数据行格式：{line}");
                continue;
            }

            try
            {
                string name = fields[0].Trim();
                if (!int.TryParse(fields[1].Trim(), out int id))
                {
                    Debug.LogWarning($"无效的ID格式：{fields[1]}");
                    continue;
                }
                string content = fields[2].Trim();

                if (!DialogueDic.TryGetValue(name, out Dictionary<int, string> idMap))
                {
                    DialogueDic[name] = new Dictionary<int, string>();
                }

                if (DialogueDic[name].ContainsKey(id))
                {
                    Debug.LogWarning($"角色 '{name}' 的 ID {id} 重复");
                    continue;
                }

                DialogueDic[name][id] = content;
                Debug.Log($"已加载：角色 '{name}', ID {id}, 内容 '{content}'");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"解析对话数据出错：{line}\n{e.Message}");
            }
        }
    }

    public int GetNameLength(string name)
    {
        return DialogueDic[name].Values.Count;
    }
}