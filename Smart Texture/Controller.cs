#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace MeshAnimatorSuite
{
    public class Controller
    {
        private Engine engine;
        private EngineVideoEditor videoEditor;

        public Controller()
        {
            engine = new Engine();
            videoEditor = new EngineVideoEditor();
        }

        public void ExecuteCopyPath()
        {
            engine.ExecuteCopyPath();
        }

        public void ExecuteOSCPicture(GameObject shaderGameObject, DefaultAsset folder, AnimatorController animatorController)
        {
            var result = engine.ExecuteOSCPicture(shaderGameObject, folder, animatorController);
        }

        public void ExecuteShaderGenerator(DefaultAsset folder, int _heigh, int _width)
        {
            engine.ExecuteShaderGenerator(folder, _heigh, _width);
        }

        public void ExecuteVideoEditor(List<Texture2D> TextureArray, GameObject FrameParent, string FolderAnimationName, string FolderMaterialsName, string AnimationName, float VideoLength)
        {
            videoEditor.ExecuteVideoEditor(TextureArray, FrameParent, FolderAnimationName, FolderMaterialsName, AnimationName, VideoLength);

        }
    }
}
#endif