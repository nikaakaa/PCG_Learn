using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 程序化网格分部构建器
/// 提供 AddTriangle、AddQuad、AddPentagon 等方法用于累积构建网格数据
/// 支持 2D（无高度）和 3D（带高度侧面）两种模式
/// </summary>
public class ProceduralMeshPart
{
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Vector2> uvs = new List<Vector2>();
    private List<Color> colors = new List<Color>();

    // 缓存数组，FillArrays 后可用
    public Vector3[] Vertices { get; private set; }
    public int[] Triangles { get; private set; }
    public Vector2[] UVs { get; private set; }
    public Color[] Colors { get; private set; }

    /// <summary>
    /// 将 List 数据填充为数组，供 Mesh 使用
    /// </summary>
    public void FillArrays()
    {
        Vertices = vertices.ToArray();
        Triangles = triangles.ToArray();
        UVs = uvs.ToArray();
        Colors = colors.ToArray();
    }

    /// <summary>
    /// 清除所有数据
    /// </summary>
    public void Clear()
    {
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
        colors.Clear();
    }

    #region 2D 模式 (平面)

    /// <summary>
    /// 添加三角形（2D 平面，顺序为顺时针或逆时针一致即可）
    /// </summary>
    public void AddTriangle(Vector3 a, Vector3 b, Vector3 c)
    {
        int startIndex = vertices.Count;
        vertices.Add(a);
        vertices.Add(b);
        vertices.Add(c);

        triangles.Add(startIndex);
        triangles.Add(startIndex + 1);
        triangles.Add(startIndex + 2);

        // 默认 UV：根据 XZ 平面映射
        uvs.Add(new Vector2(a.x, a.z));
        uvs.Add(new Vector2(b.x, b.z));
        uvs.Add(new Vector2(c.x, c.z));

        // 默认颜色：白色
        colors.Add(Color.white);
        colors.Add(Color.white);
        colors.Add(Color.white);
    }

    /// <summary>
    /// 添加四边形（2D 平面，由两个三角形组成）
    /// 顶点顺序：a-b-c-d 形成四边形（a 左下、b 左上、c 右上、d 右下 或其他一致顺序）
    /// </summary>
    public void AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        // 拆分为两个三角形：a-b-c 和 a-c-d
        AddTriangle(a, b, c);
        AddTriangle(a, c, d);
    }

    /// <summary>
    /// 添加五边形（2D 平面，由三个三角形组成）
    /// 顶点顺序：a-b-c-d-e 顺时针或逆时针
    /// </summary>
    public void AddPentagon(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 e)
    {
        // 拆分为三个三角形：a-b-c、a-c-d、a-d-e
        AddTriangle(a, b, c);
        AddTriangle(a, c, d);
        AddTriangle(a, d, e);
    }

    #endregion

    #region 3D 模式 (带高度 + 侧面)

    /// <summary>
    /// 添加三角形（3D 模式，含顶面和侧面）
    /// </summary>
    public void AddTriangle(Vector3 a, Vector3 b, Vector3 c, float height)
    {
        // 顶面
        Vector3 aTop = a + Vector3.up * height;
        Vector3 bTop = b + Vector3.up * height;
        Vector3 cTop = c + Vector3.up * height;
        AddTriangle(aTop, bTop, cTop);

        // 底面（反向）
        AddTriangle(c, b, a);

        // 侧面（三条边）
        AddSideQuad(a, b, height);
        AddSideQuad(b, c, height);
        AddSideQuad(c, a, height);
    }

    /// <summary>
    /// 添加四边形（3D 模式，含顶面和侧面）
    /// </summary>
    public void AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float height)
    {
        // 顶面
        Vector3 aTop = a + Vector3.up * height;
        Vector3 bTop = b + Vector3.up * height;
        Vector3 cTop = c + Vector3.up * height;
        Vector3 dTop = d + Vector3.up * height;
        AddQuad(aTop, bTop, cTop, dTop);

        // 底面（反向）
        AddQuad(d, c, b, a);

        // 侧面（四条边）
        AddSideQuad(a, b, height);
        AddSideQuad(b, c, height);
        AddSideQuad(c, d, height);
        AddSideQuad(d, a, height);
    }

    /// <summary>
    /// 添加五边形（3D 模式，含顶面和侧面）
    /// </summary>
    public void AddPentagon(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 e, float height)
    {
        // 顶面
        Vector3 aTop = a + Vector3.up * height;
        Vector3 bTop = b + Vector3.up * height;
        Vector3 cTop = c + Vector3.up * height;
        Vector3 dTop = d + Vector3.up * height;
        Vector3 eTop = e + Vector3.up * height;
        AddPentagon(aTop, bTop, cTop, dTop, eTop);

        // 底面（反向）
        AddPentagon(e, d, c, b, a);

        // 侧面（五条边）
        AddSideQuad(a, b, height);
        AddSideQuad(b, c, height);
        AddSideQuad(c, d, height);
        AddSideQuad(d, e, height);
        AddSideQuad(e, a, height);
    }

    /// <summary>
    /// 添加侧面四边形（从 bottom1-bottom2 底边向上延伸 height）
    /// </summary>
    private void AddSideQuad(Vector3 bottom1, Vector3 bottom2, float height)
    {
        Vector3 top1 = bottom1 + Vector3.up * height;
        Vector3 top2 = bottom2 + Vector3.up * height;
        // 侧面四边形：bottom1 -> bottom2 -> top2 -> top1
        AddTriangle(bottom1, bottom2, top2);
        AddTriangle(bottom1, top2, top1);
    }

    #endregion

    #region 工具方法

    /// <summary>
    /// 获取当前顶点数量
    /// </summary>
    public int VertexCount => vertices.Count;

    /// <summary>
    /// 获取当前三角形索引数量
    /// </summary>
    public int TriangleCount => triangles.Count;

    #endregion
}
