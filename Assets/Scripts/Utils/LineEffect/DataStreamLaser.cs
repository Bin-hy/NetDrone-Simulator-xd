using UnityEngine;

public class DataStreamLaser : MonoBehaviour
{
    [Header("激光设置")]
    public Transform pointA;      // 发射端（无人机）
    public Transform pointB;      // 接收端
    public float laserWidth = 0.2f;
    public Color laserColor = Color.cyan;

    [Header("数据流动效果")]
    public float flowSpeed = -1f;  // 数据流动速度
    public Texture flowTexture;   // 流动纹理
    public ParticleSystem dataParticles; // 流动粒子

    private LineRenderer lineRenderer;
    private Material laserMaterial;
    private float uvOffset;

    void Start()
    {
        // 初始化LineRenderer
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = laserWidth;
        lineRenderer.endWidth = laserWidth;

        // 创建动态材质
        laserMaterial = new Material(Shader.Find("Unlit/Transparent"));
        laserMaterial.mainTexture = flowTexture;
        laserMaterial.color = laserColor;
        lineRenderer.material = laserMaterial;

        // 配置粒子系统
        if (dataParticles != null)
        {
            var main = dataParticles.main;
            main.startSpeed = 0;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
        }
    }

    void Update()
    {
        if (pointA == null || pointB == null) return;

        // 更新激光位置
        lineRenderer.SetPosition(0, pointA.position);
        lineRenderer.SetPosition(1, pointB.position);

        // UV动画 - 创建数据流动效果
        uvOffset += Time.deltaTime * flowSpeed;
        laserMaterial.mainTextureOffset = new Vector2(uvOffset, 0);

        // 更新流动粒子
        UpdateParticles();
    }

    void UpdateParticles()
    {
        if (dataParticles == null) return;

        // 计算A到B的方向和距离
        Vector3 direction = pointB.position - pointA.position;
        float distance = direction.magnitude;
        direction.Normalize();

        // 调整粒子生命周期
        var main = dataParticles.main;
        main.startLifetime = distance / 5f; // 根据距离调整生命周期

        // 发射粒子
        if (!dataParticles.isPlaying)
        {
            dataParticles.Play();
        }

        // 粒子沿直线运动
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[dataParticles.particleCount];
        int count = dataParticles.GetParticles(particles);

        for (int i = 0; i < count; i++)
        {
            // 计算粒子当前位置比例 (0-1)
            float lifeProgress = 1 - (particles[i].remainingLifetime / particles[i].startLifetime);

            // 更新粒子位置
            particles[i].position = Vector3.Lerp(pointA.position, pointB.position, lifeProgress);
        }

        dataParticles.SetParticles(particles, count);
    }
}