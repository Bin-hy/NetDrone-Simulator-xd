using UnityEngine;
using System.Collections; // 用于 IEnumerator

public class LaserController : MonoBehaviour
{
    [Header("激光设置")]
    public Transform pointA;      // 发射端（无人机），在FireLaser中传入
    public Transform pointB;      // 接收端，在FireLaser中传入
    public float laserWidth = 0.2f;
    public Color laserColor = Color.cyan;

    [Header("数据流动效果")]
    public float flowSpeed = -1f;  // 数据流动速度
    public Texture flowTexture;   // 流动纹理
    public ParticleSystem dataParticles; // 流动粒子

    private LineRenderer lineRenderer;
    private Material laserMaterial;
    private float uvOffset;
    private bool isLaserActive = false; // 激光是否激活的状态
    private Coroutine autoStopCoroutine; // 用于存储自动停止的协程引用

    void Awake() // 更改为Awake，确保在Start之前初始化
    {
        // 初始化LineRenderer
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null) // 如果没有LineRenderer，则添加一个
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = laserWidth;
        lineRenderer.endWidth = laserWidth;
        lineRenderer.enabled = false; // 初始时禁用，等待FireLaser调用

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
            dataParticles.Stop(); // 初始时停止粒子系统
            dataParticles.Clear(); // 清除残留粒子
        }
    }

    void Update()
    {
        if (!isLaserActive || pointA == null || pointB == null) return; // 只有当激光激活且两点都存在时才更新

        // 更新激光位置
        lineRenderer.SetPosition(0, pointA.position);
        lineRenderer.SetPosition(1, pointB.position);

        // UV动画 - 创建数据流动效果
        uvOffset += Time.deltaTime * flowSpeed;
        laserMaterial.mainTextureOffset = new Vector2(uvOffset, 0);

        // 更新流动粒子
        UpdateParticles();
    }

    /// <summary>
    /// 发射数据流激光。
    /// </summary>
    /// <param name="startTransform">激光发射的起点Transform。</param>
    /// <param name="endTransform">激光射击的终点Transform。</param>
    /// <param name="duration">激光持续时间（秒）。如果设置为0或负数，激光将不会自动消失，需要主动停止。</param>
    public void FireLaser(Transform startTransform, Transform endTransform, float duration = 0f)
    {
        // 如果激光已经激活，先停止旧的
        if (isLaserActive)
        {
            StopLaser();
        }

        pointA = startTransform;
        pointB = endTransform;

        if (pointA == null || pointB == null)
        {
            Debug.LogWarning("Laser Fire failed: Start or End Transform is null.");
            return;
        }

        isLaserActive = true;
        lineRenderer.enabled = true; // 激活LineRenderer

        // 重新设置LineRenderer的宽度和颜色，以便外部调整能生效
        lineRenderer.startWidth = laserWidth;
        lineRenderer.endWidth = laserWidth;
        laserMaterial.color = laserColor;

        // 播放粒子系统
        if (dataParticles != null)
        {
            dataParticles.Play();
        }

        // 如果设置了持续时间，启动自动停止协程
        if (duration > 0)
        {
            autoStopCoroutine = StartCoroutine(AutoStopLaser(duration));
        }
    }

    /// <summary>
    /// 主动停止数据流激光。
    /// </summary>
    public void StopLaser()
    {
        if (!isLaserActive) return; // 如果未激活，则直接返回

        isLaserActive = false;
        lineRenderer.enabled = false; // 禁用LineRenderer

        // 停止粒子系统
        if (dataParticles != null)
        {
            dataParticles.Stop();
            dataParticles.Clear(); // 清除残留粒子
        }

        // 停止所有正在运行的自动停止协程
        if (autoStopCoroutine != null)
        {
            StopCoroutine(autoStopCoroutine);
            autoStopCoroutine = null;
        }
    }

    private IEnumerator AutoStopLaser(float delay)
    {
        yield return new WaitForSeconds(delay);
        StopLaser(); // 时间到，自动停止激光
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
        // 根据距离调整生命周期，确保粒子能从A点运动到B点
        // 这里的5f是一个经验值，你可以根据粒子速度和效果调整
        main.startLifetime = distance / (Mathf.Abs(flowSpeed) * 5f + 0.1f); // 防止除以0

        // 确保粒子系统正在播放
        if (!dataParticles.isPlaying)
        {
            dataParticles.Play();
        }

        // 获取粒子数组并手动更新位置
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[dataParticles.particleCount];
        int count = dataParticles.GetParticles(particles);

        for (int i = 0; i < count; i++)
        {
            // 计算粒子当前位置比例 (0-1)
            // remainingLifetime从startLifetime递减到0
            float lifeProgress = 1 - (particles[i].remainingLifetime / particles[i].startLifetime);

            // 更新粒子位置，使其在A和B之间线性插值
            particles[i].position = Vector3.Lerp(pointA.position, pointB.position, lifeProgress);
        }

        dataParticles.SetParticles(particles, count);
    }
}