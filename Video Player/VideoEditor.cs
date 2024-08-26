#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using UnityEditor.SceneManagement;


public class EngineVideoEditor
{
    public VideoEditor ExecuteVideoEditor(List<Texture2D> TextureArray, GameObject FrameParent, string FolderAnimationName, string FolderMaterialsName, string AnimationName, float VideoLength)
    {
        return new VideoEditor (TextureArray, FrameParent, FolderAnimationName, FolderMaterialsName, AnimationName, VideoLength);
    }
}

[System.Serializable]
public class VideoEditor
{
    private List<Texture2D> textureArray;
    private GameObject frameParent;
    private string folderAnimationName;
    private string folderMaterialsName;
    private string animationName;
    private float videoLength;

    static float timeIntervalStorage;
    static int numberOfElements;

    public VideoEditor(List<Texture2D> TextureArray, GameObject FrameParent, string FolderAnimationName, string FolderMaterialsName, string AnimationName, float VideoLength)
    {
        textureArray = TextureArray;
        frameParent = FrameParent;
        folderAnimationName = FolderAnimationName;
        folderMaterialsName = FolderMaterialsName;
        animationName = AnimationName;
        videoLength = VideoLength;


        OnInspectorGUI();
    }

    //static bool animatorControllerCreated = false;
    public void OnInspectorGUI()
    {

        int maxFrame = textureArray.Count;

        toDoGameObject(maxFrame);


        void toDoGameObject(int maxFrame)
        {
            GameObject frame;

            AnimationClip animationClipON = new AnimationClip();
            AnimationClip animationClipOFF = new AnimationClip();

            SetLoopTime(animationClipON, true);
            SetLoopTime(animationClipOFF, false);

            var curveBindingsON = AnimationUtility.GetCurveBindings(animationClipON);
            foreach (var curveBindingON in curveBindingsON)
            {
                Console.WriteLine($"CurveBinding: PropertyName={curveBindingON.propertyName}, Path={curveBindingON.path}");
            }

            var curveBindingsOFF = AnimationUtility.GetCurveBindings(animationClipOFF);
            foreach (var curveBindingOFF in curveBindingsOFF)
            {
                Console.WriteLine($"CurveBinding: PropertyName={curveBindingOFF.propertyName}, Path={curveBindingOFF.path}");
            }

            CreateAnimationFolder();
            AssetDatabase.CreateAsset(animationClipON, $"Assets/{folderAnimationName}/{animationName}ON.anim");
            AssetDatabase.CreateAsset(animationClipOFF, $"Assets/{folderAnimationName}/{animationName}OFF.anim");

            CreateMaterialsFolder();

            float timeInterval = videoLength / maxFrame; //96 / 14 = 6,8 96 * 14 = 
            timeIntervalStorage = timeInterval;

            for (numberOfElements = 0; numberOfElements < maxFrame; numberOfElements++)
            {
                frame = new GameObject($"Frame ({numberOfElements + 1})");

                frame.transform.parent = frameParent.transform;

                // Create RectTransform
                RectTransform rectTransform = frame.AddComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(100, 100);

                frame.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, 0, 0);
                frame.GetComponent<RectTransform>().rotation = new Quaternion(0, 0, 0, 0);
                frame.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);

                //Create Material
                Material material = new Material(Shader.Find("Standard"));
                material.mainTexture = textureArray[numberOfElements];
                AssetDatabase.CreateAsset(material, $"Assets/{folderAnimationName}/{folderMaterialsName}/{frame.name}.mat");

                // Create Renderer
                MeshRenderer renderer = frame.AddComponent<MeshRenderer>();
                renderer.material = material;

                // Create MeshFilter
                MeshFilter meshFilter = frame.AddComponent<MeshFilter>();
                Mesh quadMesh = Resources.GetBuiltinResource<Mesh>("Quad.fbx");
                meshFilter.sharedMesh = quadMesh;

                //frame = GameObject.CreatePrimitive(PrimitiveType.Quad);
                meshFilter.sharedMesh = frame.GetComponent<MeshFilter>().sharedMesh;


                renderer.enabled = false;
                //frame.SetActive(false);

                //Animator animator = frame.GetComponent<Animator>();
                //if (animator == null)
                //{
                //    animator = frame.AddComponent<Animator>();
                //}

                //animator.SetFloat("Alpha", material.color.a);

                toDoAnimationON(frame, animationClipON, timeInterval);
                toDoAnimationOFF(frame, animationClipOFF);

                Debug.Log("Working... " + numberOfElements);
            }
        }

        void toDoAnimationON(GameObject frame, AnimationClip animationClip, float timeInterval)
        {
            EditorCurveBinding curveBinding = new EditorCurveBinding();
            curveBinding.type = typeof(MeshRenderer);
            curveBinding.path = GetHierarchyPathWithoutTopObject(frame);
            curveBinding.propertyName = "m_Enabled";
            //curveBinding.propertyName = "m_IsActive";


            AnimationCurve curve = new AnimationCurve();


            curve.AddKey(timeIntervalStorage - 0.01f, 0.0f);
            curve.AddKey(timeIntervalStorage, 1.0f);
            curve.AddKey(timeIntervalStorage + timeInterval, 0.0f);

            timeIntervalStorage = timeInterval + timeIntervalStorage;


            AnimationUtility.SetEditorCurve(animationClip, curveBinding, curve);
        }

        void toDoAnimationOFF(GameObject frame, AnimationClip animationClip)
        {
            EditorCurveBinding curveBinding = new EditorCurveBinding();
            curveBinding.type = typeof(MeshRenderer);
            //curveBinding.type = typeof(GameObject);
            curveBinding.path = GetHierarchyPathWithoutTopObject(frame);
            curveBinding.propertyName = "m_Enabled";
            //curveBinding.propertyName = "m_IsActive";

            AnimationCurve curve = new AnimationCurve();

            // Frame off
            curve.AddKey(0.0f, 0.0f);

            AnimationUtility.SetEditorCurve(animationClip, curveBinding, curve);
        }

        void SetLoopTime(AnimationClip clip, bool loopTime)
        {
            AnimationClipSettings clipSettings = AnimationUtility.GetAnimationClipSettings(clip);
            clipSettings.loopTime = loopTime;
            AnimationUtility.SetAnimationClipSettings(clip, clipSettings);
        }

        void CreateAnimationFolder()
        {
            string path = $"Assets/{folderAnimationName}";

            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder("Assets", $"{folderAnimationName}");
            }
            else
            {
                return;
            }
        }

        void CreateMaterialsFolder()
        {
            string path = $"Assets/{folderAnimationName}/{folderMaterialsName}";

            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder($"Assets/{folderAnimationName}", $"{folderMaterialsName}");
            }
            else
            {
                return;
            }
        }

        string GetHierarchyPathWithoutTopObject(GameObject obj)
        {
            if (obj == null)
            {
                return "";
            }

            Transform parrentTransform = obj.transform.parent;

            if (parrentTransform == null)
            {
                return "No parent";
            }

            string path = obj.name;

            while (parrentTransform.parent != null)
            {
                obj = parrentTransform.gameObject;
                parrentTransform = obj.transform.parent;
                path = obj.name + "/" + path;
            }

            return path;
        }

        void OnProgressUpdated(int current, int max, string label)
        {
            Debug.Log($"Progress updated: {current}/{max}, Label: {label}");
        }


        //if (GUI.changed)
        //{
        //    EditorUtility.SetDirty(video);
        //    EditorSceneManager.MarkSceneDirty(gameObject.scene);
        //}
    }
}

#endif