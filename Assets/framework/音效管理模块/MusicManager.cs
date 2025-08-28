using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MusicManager : Singleton<MusicManager>
{
    private AudioSource _audioSource;
    
    private float bgmValue=0;
    private float soundValue=1;

    private List<AudioSource> audioList = new List<AudioSource>();//用于记录使用的组件，便于进行停止或回收操作
    private bool isPlaySound=true;
    
    private MusicManager()
    {
        MonoManager.Instance.AddUpdateEvent(Update);
    }

    private void Update()
    {
        if(!isPlaySound)
            return;
        
        //逆向遍历可以避免正向遍历修改可能出现的问题
        //将播放完的音效进行回收
        for (int i = audioList.Count-1; i >=0; i--)
        {
            if (!audioList[i].isPlaying)
            {
                audioList[i].clip = null;
                PoolMgr.Instance.PushObj(audioList[i].gameObject);
                audioList.Remove(audioList[i]);
            }
        }
    }
    
    //播放背景音
    public void PlayBGM(string bgmName)
    {
        if (_audioSource == null)
        {
            GameObject obj = new GameObject("MusicManager");
            _audioSource = obj.AddComponent<AudioSource>();
            GameObject.DontDestroyOnLoad(obj);
        }

        ABManager.Instance.LoadResourceAsync<AudioClip>("music",bgmName,true, (clip) =>
        {
            _audioSource.clip = clip;
            _audioSource.loop = true;
            _audioSource.volume = bgmValue;
            _audioSource.Play();
        });
    }
    
    //暂停背景音
    public void PauseBGM()
    {
        if(_audioSource==null)
            return;
        _audioSource.Pause();
    }
    
    //停止背景音
    public void StopBGM()
    {
        if(_audioSource==null)
            return;
        _audioSource.Stop();
    }
    
    //设置音乐大小
    public void SetBGMValue(float value)
    {
        bgmValue = value;
        if (_audioSource==null)
            return;
        _audioSource.volume = bgmValue;
    }
    
    
    //播放音效
    public void PlaySound(string soundName,UnityAction<AudioSource> callBack)
    {
        ABManager.Instance.LoadResourceAsync<AudioClip>("music", soundName, false, (clip) =>
        {
            AudioSource audioSource = PoolMgr.Instance.GetObj("Music/SoundObj",10).GetComponent<AudioSource>();
            //使用Stop是因为如果用的是之前正在使用的对象，就要去停止
            audioSource.Stop();
            
            audioSource.clip = clip;
            audioSource.loop = false;
            audioSource.volume = soundValue;
            audioSource.Play();
            //如果是之前使用过的对象就不需要加入列表中，否则就加入列表
            if(!audioList.Contains(audioSource))
                audioList.Add(audioSource);
            callBack?.Invoke(audioSource);
        });
    }
    
    //停止播放音效（循环音效）
    public void StopSound(AudioSource audioSource)
    {
        if (audioList.Contains(audioSource))
        {
            audioSource.Stop();
            audioSource.clip = null;
            audioList.Remove(audioSource);
            PoolMgr.Instance.PushObj(audioSource.gameObject);
        }
    }
    
    //暂停或继续播放所有音效
    public void PauseOrPlaySound(bool isPLay)
    {
        if (!isPLay)
        {
            isPlaySound = false;
            for (int i = 0; i < audioList.Count; i++)
            {
                 audioList[i].Pause();                           
            }
        }
        else
        {
            isPlaySound = true;
            for (int i = 0; i < audioList.Count; i++)
            {
                audioList[i].Play();
            }
        }
        
    }
    
    //修改音效大小
    public void SetSoundValue(float value)
    {
        bgmValue = value;
        for (int i = 0; i < audioList.Count; i++)
        {
            audioList[i].volume = bgmValue;
        }
    }
    
    //清空音效相关记录，过场景时要在进行清除对象池之前去调用
    public void ClearSound()
    {
        for (int i = 0; i < audioList.Count; i++)
        {
            audioList[i].Stop();
            audioList[i].clip = null;
            PoolMgr.Instance.PushObj(audioList[i].gameObject);
        }
        audioList.Clear();
    }
    
    
}
