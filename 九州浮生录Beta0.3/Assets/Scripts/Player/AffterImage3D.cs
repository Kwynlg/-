/****************************************************
    文件：AffterImage3D.cs
	作者：Kaiy
    邮箱: 3120713254@qq.com
    日期：#CreateTime#	
	功能：Nothing
*****************************************************/

using UnityEngine;

/// <summary>
/// 残影
/// </summary>
public class AffterImage3D : MonoBehaviour
{
    public GameObject[] clones;
    public float lifetime = 2f;

    private void Start()
    {
        var renders = GetComponentsInChildren<SkinnedMeshRenderer>();
        clones = new GameObject[renders.Length];
        for (int i =0;i<renders.Length;i++)
        {
            var sr = renders[i];
            var go = new GameObject();
            clones[i] = go;
            go.AddComponent<MeshRenderer>().material = sr.material;
            Mesh mesh = new Mesh();
            renders[i].BakeMesh(mesh);
            go.AddComponent<MeshFilter>().mesh = mesh;

            go.transform.position = sr.transform.position;
            go.transform.rotation = sr.transform.rotation;
        }
    }

    private void Update()
    {
        lifetime -= Time.deltaTime;
        if(lifetime <= 0)
            Destroy(this);
    }

    private void OnDestroy()
    {
        foreach (var item in clones)
        {
            Destroy(item);
        }
        clones = null;
    }
}