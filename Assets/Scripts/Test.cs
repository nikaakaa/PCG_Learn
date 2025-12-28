using UnityEngine;

public class Test : MonoBehaviour {
    public ComputeShader myShader;
    public float offsetValue = 5.0f;
    private int size = 64;

    void Start() {
        int totalCount = size * size * size;
        float[] data = new float[totalCount]; // 准备测试数据

        // 1. 创建 ComputeBuffer (数量, 每个元素的大小)
        ComputeBuffer buffer = new ComputeBuffer(totalCount, sizeof(float));
        buffer.SetData(data); // 上传到 GPU

        // 2. 找到内核索引并设置参数
        int kernel = myShader.FindKernel("CSMain");
        myShader.SetBuffer(kernel, "resultBuffer", buffer);
        myShader.SetFloat("offset", offsetValue);
        myShader.SetInt("size", size);

        // 3. 调度执行 (Dispatch)
        // 既然我们每组有 8x8x8 个线程，那么组数就是 size / 8
        int groups = size / 8; 
        myShader.Dispatch(kernel, groups, groups, groups);

        // 4. 将结果拷回 CPU (仅供演示，实际开发中尽量留在 GPU 渲染)
        buffer.GetData(data);
        Debug.Log("第一个点的值变为: " + data[0]);

        // 5. 释放内存
        buffer.Release();
    }
}