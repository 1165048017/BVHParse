using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

using NAudio;
using NAudio.Wave;

public class BVHDriver : MonoBehaviour
{
    [Header("Loader settings")]
    [Tooltip("This is the target avatar for which the animation should be loaded. Bone names should be identical to those in the BVH file and unique. All bones should be initialized with zero rotations. This is usually the case for VRM avatars.")]
    public Animator targetAvatar;
    [Tooltip("This is the path to the BVH file that should be loaded. Bone offsets are currently being ignored by this loader.")]
    public InputField BVHPath;
    public Text BVHLog;
    [Tooltip("This is the path to the audio file that should be loaded. Bone offsets are currently being ignored by this loader.")]
    public InputField audioPath;
    public Text audioLog;
    [Tooltip("Audio Source")]
    public AudioSource audioSource;
    [Tooltip("Play Button")]
    public Button playBtn;
    [Tooltip("If the flag above is disabled, the frame rate given in the BVH file will be overridden by this value.")]
    public float frameRate = 60.0f;
    [Tooltip("If the BVH first frame is T(if not,make sure the defined skeleton is T).")]
    public bool FirstT = true;

    [Serializable]
    public struct BoneMap
    {
        public string bvh_name;
        public HumanBodyBones humanoid_bone;
    }
    [Tooltip("If the flag above is disabled, the frame rate given in the BVH file will be overridden by this value.")]
    public BoneMap[] bonemaps; // the corresponding bones between unity and bvh
    private BVHParser bp = null;
    private Animator anim;   
   
    // This function doesn't call any Unity API functions and should be safe to call from another thread
    public void parseFile()
    {        
        string bvhData = File.ReadAllText(BVHPath.text);
        bp = new BVHParser(bvhData);        
        frameRate = 1f / bp.frameTime;
    }

    private Dictionary<string, Quaternion> bvhT;
    private Dictionary<string, Vector3> bvhOffset;
    private Dictionary<string, string> bvhHireachy;
    private Dictionary<HumanBodyBones, Quaternion> unityT;

    private int frameIdx;
    private float scaleRatio = 0.0f;

    private bool audioPlay=false;
    private bool bvhPlay=false;

    IEnumerator Mp3toClip(string mp3Position)
    {
        //load mp3 from nAudio:https://my.oschina.net/u/150705/blog/3124980
        WWW www = new WWW(mp3Position);
        yield return www;

        if (www.isDone)
        {
            var stream = new MemoryStream(www.bytes);
            var reader = new Mp3FileReader(stream);

            var outstream = new MemoryStream();
            WaveFileWriter.WriteWavFileToStream(outstream, reader);

            //加载为AudioClip
            audioSource.clip = WavUtility.ToAudioClip(outstream.ToArray(), 0);
        }
        yield break;
    }

    private IEnumerator LoadMusic(string filepath)
    {
        filepath = "file://" + filepath;
        using (var uwr = UnityWebRequestMultimedia.GetAudioClip(filepath, AudioType.WAV))
        {
            yield return uwr.SendWebRequest();
            if (uwr.isNetworkError)
            {
                Debug.LogError(uwr.error);
            }
            else
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(uwr);
                // use audio clip
                audioSource.clip = clip;
            }
        }
    }
    private Vector3 initRootPos;
    public void GetDataClick ()
    {
        anim = targetAvatar.GetComponent<Animator>();
        initRootPos = anim.GetBoneTransform(HumanBodyBones.Hips).position;

        if (File.Exists(BVHPath.text))
        {
            BVHLog.text = "bvh exists";
            playBtn.interactable = true;
        }
        else
        {
            BVHLog.text = "bvh not exists";
        }

        if (File.Exists(audioPath.text))
        {
            audioLog.text = "audio exists";
            audioPlay = true;

            string url = string.Format("file://{0}", audioPath.text);
            if (Path.GetExtension(audioPath.text) == ".mp3")
            {
                StartCoroutine(Mp3toClip(url));
            }
            else
            {
                WWW www = new WWW(url);
                audioSource.clip = www.GetAudioClip(false, false);
                //StartCoroutine(LoadMusic(audioPath.text));
            }
        }
        else
        {
            audioLog.text = "audio not exists";
            audioPlay = false;
        }

        parseFile();
        Application.targetFrameRate = (Int16)frameRate;

        bvhT = bp.getKeyFrame(0);
        bvhOffset = bp.getOffset(1.0f);
        bvhHireachy = bp.getHierachy();
        
        unityT = new Dictionary<HumanBodyBones, Quaternion>();
        foreach(BoneMap bm in bonemaps)
        {
            unityT.Add(bm.humanoid_bone, anim.GetBoneTransform(bm.humanoid_bone).rotation);
        }

        float unity_leftleg = (anim.GetBoneTransform(HumanBodyBones.LeftLowerLeg).position - anim.GetBoneTransform(HumanBodyBones.LeftUpperLeg).position).sqrMagnitude +
            (anim.GetBoneTransform(HumanBodyBones.LeftFoot).position - anim.GetBoneTransform(HumanBodyBones.LeftLowerLeg).position).sqrMagnitude;
        float bvh_leftleg = 0.0f;
        foreach(BoneMap bm in bonemaps) {
            if(bm.humanoid_bone==HumanBodyBones.LeftUpperLeg || bm.humanoid_bone == HumanBodyBones.LeftLowerLeg)
            {
                bvh_leftleg = bvh_leftleg + bvhOffset[bm.bvh_name].sqrMagnitude;
            }
        }
        scaleRatio = unity_leftleg / bvh_leftleg;        
        frameIdx = 0;
    }

    public void PlayClick()
    {
        bvhPlay = true;
    }

    public Canvas UICanva;
    private void Update()
    {
        // play audio
        if (bvhPlay && audioPlay)
        {
            audioSource.Play();
            audioPlay = false;
            UICanva.enabled = false;
        }
        // play BVH
        if (bvhPlay)
        {
            Dictionary<string, Quaternion> currFrame = bp.getKeyFrame(frameIdx);//frameIdx 2871
            if (frameIdx < bp.frames - 1)
            {
                frameIdx++;
            }
            else
            {
                // restart play bvh and audio
                frameIdx = 0;
                audioSource.Stop();
                audioPlay = true;
            }
            foreach (BoneMap bm in bonemaps)
            {                
                // if the first frame is T pose (example 13_29.bvh)
                if (FirstT)
                {
                    Transform currBone = anim.GetBoneTransform(bm.humanoid_bone);
                    currBone.rotation = (currFrame[bm.bvh_name] * Quaternion.Inverse(bvhT[bm.bvh_name])) * unityT[bm.humanoid_bone];
                }
                // if the inherit skeleton is T (example temp.bvh)
                else
                {
                    Transform currBone = anim.GetBoneTransform(bm.humanoid_bone);
                    currBone.rotation = currFrame[bm.bvh_name] * unityT[bm.humanoid_bone];
                }
            }
           // get bvh bone positions
            Dictionary<string, Vector3> bvhPos = new Dictionary<string, Vector3>();            
            foreach (string bname in currFrame.Keys)
            {
                if (bname == "pos")
                {
                    bvhPos.Add(bp.root.name, new Vector3(currFrame["pos"].x, currFrame["pos"].y, currFrame["pos"].z));
                }
                else
                {
                    if (bvhHireachy.ContainsKey(bname) && bname != bp.root.name)
                    {
                        Vector3 curpos = bvhPos[bvhHireachy[bname]] + currFrame[bvhHireachy[bname]] * bvhOffset[bname];
                        bvhPos.Add(bname, curpos);
                    }
                }
            }
            // set avatar's root postion
            if (bvhPos[bp.root.name] == Vector3.zero)
            {
                anim.GetBoneTransform(HumanBodyBones.Hips).position = initRootPos;
            }
            else
            {
                anim.GetBoneTransform(HumanBodyBones.Hips).position = bvhPos[bp.root.name] * scaleRatio;
            }

            // draw bvh skeleton
            foreach (string bname in bvhHireachy.Keys)
            {
                Color color = new Color(1.0f, 0.0f, 0.0f);
                Debug.DrawLine(bvhPos[bname], bvhPos[bvhHireachy[bname]], color);
            }
        }
        
    }
}
