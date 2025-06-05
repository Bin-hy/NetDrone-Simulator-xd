using UnityEngine;

public class Sound_Name
{
    public const string File_Path = "Audio/";
    public const string WELCOME = "欢迎来到教程";
    public const string FINISHED_CUR = "恭喜你 完成当前步骤";
    public const string FINISHED_ALL = "恭喜你 完成所有流程";

    public const string File_Network_Path = "AudioClip/network/";

    
}

public class Sound_Effect_Name
{
    public const string Path_AudioEffect = "AudioClip/Effect/";
    // Resources.Load<AudioClip>(Sound_Effect_Name.Path_AudioEffect + Sound_Effect_Name.ErrorEffect);
    public const string ErrorEffect = "ErrorEffect";
}
public class Sound
{

    public AudioClip clip;
    public AudioSource source;
    public bool loop;

    /// <summary>
    /// 播放进度
    /// </summary>
    public float progress
    {
        get
        {
            if (source == null || clip == null)
                return 0f;
            return (float)source.timeSamples / (float)clip.samples;
        }
    }

    /// <summary>
    /// 判断是否完成播放
    /// </summary>
    public bool finished
    {
        get
        {
            return !loop && progress >= 1f;
        }
    }

    /// <summary>
    /// 播放与暂停
    /// </summary>
    public bool playing
    {
        get
        {
            return source != null && source.isPlaying;
        }
        set
        {
            if (value)
            {
                if (!source.isPlaying)
                {
                    source.UnPause();
                }
            }
            else
            {
                if (source.isPlaying)
                {
                    source.Pause();
                }
            }
        }
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="clip"></param>
    public Sound(AudioClip clip, AudioSource source)
    {
        this.clip = clip;
        this.source = source;
        this.source.clip = clip;
        this.source.Play();
    }

    public void Update()
    {
        if (source != null)
        {
            source.loop = loop;
        }
        if (finished)
        {
            Finish();
        }
    }

    /// <summary>
    /// 播放完成的话
    /// </summary>
    public void Finish()
    {
        Object.Destroy(source);
        source = null;//null的话会被自动清除
    }
}