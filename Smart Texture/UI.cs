#if UNITY_EDITOR
using EasyQuestSwitch.Fields;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Thry;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.WSA;

namespace MeshAnimatorSuite
{
    public class MeshAnimatorSuite : EditorWindow
    {
        Developer developer = new Developer();

        // ----------------
        Controller controller;

        [SerializeField] GameObject shaderGameObject;
        [SerializeField] DefaultAsset folder;
        [SerializeField] AnimatorController animator;

        AnimatorController testAnimator;

        int heigh = 32;
        int width = 32;

        const float baseTime = 18f;
        const int baseSize = 50;
        const float exponent = 6.13f;
        // ----------------


        // RCM Menu
        [MenuItem("GameObject/Copy Path", false, 11)]
        public static void CopyPath()
        {
            Controller controller = new Controller();
            controller.ExecuteCopyPath();
        }

        // Window Menu
        [MenuItem("Window/MeshAnimator Suite/Smart Texture")]
        public static void ShowWindow()
        {
            GetWindow<MeshAnimatorSuite>("Smart Texture");
        }

        private void OnEnable()
        {
            controller = new Controller();
        }

        private void OnGUI()
        {
            GUILayout.BeginVertical("box");

            GUILayout.Label("Shader Settings", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Heigh (Y)", GUILayout.Width(56));
            heigh = EditorGUILayout.IntField(heigh, GUILayout.MinWidth(50), GUILayout.ExpandWidth(true));
            heigh = Mathf.Clamp(heigh, 1, 120);
            GUILayout.Space(10);

            EditorGUILayout.LabelField("Width (X)", GUILayout.Width(56));
            width = EditorGUILayout.IntField(width, GUILayout.MinWidth(50), GUILayout.ExpandWidth(true));
            width = Mathf.Clamp(width, 1, 120);
            GUILayout.EndHorizontal();

            GUILayout.Label("Size In Pixels! (min 1, max 120)", EditorStyles.centeredGreyMiniLabel);

            int size = (heigh + width) / 2;
            float estimatedTime = baseTime * Mathf.Pow((float)size / baseSize, exponent);

            string timeFormatted;
            if (estimatedTime >= 60f)
            {
                int minutes = Mathf.FloorToInt(estimatedTime / 60f);
                int seconds = Mathf.FloorToInt(estimatedTime % 60f);
                timeFormatted = $"{minutes} min {seconds} sec";
            }
            else
            {
                timeFormatted = $"{estimatedTime:F2} sec";
            }

            GUILayout.Label($"Estimated Shader Compilation Time: {timeFormatted}", EditorStyles.centeredGreyMiniLabel);

            folder = (DefaultAsset)EditorGUILayout.ObjectField("Folder", folder, typeof(DefaultAsset), true);
            GUILayout.EndVertical();

            bool isShaderInputValid = heigh >= 1 && heigh <= 120 && width >= 1 && width <= 120 && folder != null;
            bool isAnimationInputValid = shaderGameObject != null && folder != null && animator != null;

            GUILayout.Space(2);

            using (new EditorGUI.DisabledGroupScope(!isShaderInputValid))
            {
                if (GUILayout.Button("Create Shader"))
                {
                    controller.ExecuteShaderGenerator(folder, heigh, width);
                }
            }

            GUILayout.BeginVertical("box");

            GUILayout.Label("Animation Settings", EditorStyles.boldLabel);

            folder = (DefaultAsset)EditorGUILayout.ObjectField("Folder", folder, typeof(DefaultAsset), true);
            shaderGameObject = (GameObject)EditorGUILayout.ObjectField("GO w/ Smart Texture", shaderGameObject, typeof(GameObject), true);
            animator = (AnimatorController)EditorGUILayout.ObjectField("FX Animator", animator, typeof(AnimatorController), true);

            GUILayout.EndVertical();

            GUILayout.Space(2);
            using (new EditorGUI.DisabledGroupScope(!isAnimationInputValid))
            {
                if (GUILayout.Button("Create Animation"))
                {
                    controller.ExecuteOSCPicture(shaderGameObject, folder, animator);
                }
            }

            developer.info();
        }

    }

    public class VideoPlayerWindow : EditorWindow
    {
        Developer developer = new Developer();

        Controller controller;

        private List<Texture2D> textureArray = new List<Texture2D>();
        private GameObject frameParent;
        private string folderAnimationName = "Video";
        private string folderMaterialsName = "Materials";
        private string animationName = "VideoAnim";
        private float videoLength = 0;

        private bool showTextures = false;

        private void OnEnable()
        {
            controller = new Controller();
        }

        [MenuItem("Window/MeshAnimator Suite/Video Player")]
        public static void ShowWindow()
        {
            GetWindow<VideoPlayerWindow>("Video Player");
        }

        private void OnGUI()
        {
            GUILayout.BeginVertical("box");

            GUIStyle smallerFontStyle = new GUIStyle(EditorStyles.label);
            smallerFontStyle.alignment = TextAnchor.MiddleCenter;

            GUILayout.Label("Video Player Settings", EditorStyles.boldLabel);

            GUILayout.Label("Frames", EditorStyles.label);

            Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, "Drag & Drop Frames Here", EditorStyles.helpBox);

            HandleDragAndDrop(dropArea);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Clear List", GUILayout.Width(100)))
            {
                textureArray.Clear();
            }

            showTextures = EditorGUILayout.Foldout(showTextures, $"Frames ({textureArray.Count})");

            GUILayout.EndHorizontal();

            if (showTextures)
            {
                for (int i = 0; i < textureArray.Count; i++)
                {
                    GUILayout.BeginHorizontal();
                    textureArray[i] = (Texture2D)EditorGUILayout.ObjectField($"Frame {i + 1}", textureArray[i], typeof(Texture2D), false);

                    if (GUILayout.Button("Remove", GUILayout.Width(60)))
                    {
                        textureArray.RemoveAt(i);
                    }
                    GUILayout.EndHorizontal();
                }
            }

            frameParent = (GameObject)EditorGUILayout.ObjectField("Frame Parent", frameParent, typeof(GameObject), true);
            folderAnimationName = EditorGUILayout.TextField("Folder Animation Name", folderAnimationName);
            folderMaterialsName = EditorGUILayout.TextField("Folder Materials Name", folderMaterialsName);
            animationName = EditorGUILayout.TextField("Animation Name", animationName);
            videoLength = EditorGUILayout.FloatField("Video Length (seconds)", videoLength);
            GUILayout.Label("Number of seconds how long your video is", EditorStyles.centeredGreyMiniLabel);

            GUILayout.EndVertical();

            bool isButtonEnabled = textureArray.Count > 0 &&
                                   frameParent != null &&
                                   !string.IsNullOrWhiteSpace(folderAnimationName) &&
                                   !string.IsNullOrWhiteSpace(folderMaterialsName) &&
                                   videoLength > 0;

            GUI.enabled = isButtonEnabled;

            if (GUILayout.Button("Create A Video"))
            {
                controller.ExecuteVideoEditor(textureArray, frameParent, folderAnimationName, folderMaterialsName, animationName, videoLength);
            }

            GUI.enabled = true;

            developer.info();
        }

        private void HandleDragAndDrop(Rect dropArea)
        {
            Event evt = Event.current;
            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!dropArea.Contains(evt.mousePosition))
                        return;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (UnityEngine.Object draggedObject in DragAndDrop.objectReferences)
                        {
                            Texture2D texture = draggedObject as Texture2D;
                            if (texture != null)
                            {
                                textureArray.Add(texture);
                            }
                        }
                    }
                    break;
            }
        }
    }


    public class Developer
    {
        public void info()
        {
            GUILayout.Space(16);

            GUIStyle whodidit = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
            whodidit.fontSize = 12;

            GUILayout.Label("powered by Dahus", whodidit);

            var linkStyle = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.gray },
                hover = { textColor = new Color(0.7f, 0.7f, 0.7f) },
            };

            var leftLinkStyle = new GUIStyle(linkStyle)
            {
                alignment = TextAnchor.MiddleLeft
            };

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            var leftLinkText = "#dahusvrc";
            var leftLinkRect = GUILayoutUtility.GetRect(new GUIContent(leftLinkText), leftLinkStyle);

            GUI.Label(leftLinkRect, leftLinkText, leftLinkStyle);
            if (Event.current.type == EventType.MouseDown && leftLinkRect.Contains(Event.current.mousePosition))
            {
                UnityEngine.Application.OpenURL("https://discordapp.com/users/290117159806566421/");
                Event.current.Use();
            }

            GUILayout.Space(6);

            var rightLinkText = "github.com/Dahus";
            var rightLinkRect = GUILayoutUtility.GetRect(new GUIContent(rightLinkText), linkStyle);

            GUI.Label(rightLinkRect, rightLinkText, linkStyle);
            if (Event.current.type == EventType.MouseDown && rightLinkRect.Contains(Event.current.mousePosition))
            {
                UnityEngine.Application.OpenURL("https://github.com/Dahus");
                Event.current.Use();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(4);
        }
    }
}
#endif
