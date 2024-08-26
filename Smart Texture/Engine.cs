#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.WSA;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

using System.Text;
using System.Linq;
using static UnityEditor.Experimental.GraphView.GraphView;

//using System.Threading.Tasks;

namespace MeshAnimatorSuite
{

    public class Engine
    {
        public void ExecuteCopyPath()
        {
            CopyPathFromHierarchy copyPathInstance = new CopyPathFromHierarchy();
            copyPathInstance.CopyPath();
        }

        public OSCPicture ExecuteOSCPicture(GameObject shaderGameObcjet, DefaultAsset folder, AnimatorController animatorController)
        {
            return new OSCPicture(shaderGameObcjet, folder, animatorController);
        }

        public ShaderGenerator ExecuteShaderGenerator(DefaultAsset folder, int _heigh, int _width)
        {
            return new ShaderGenerator(folder, _heigh, _width);
        }
    }

    //Zeroing Program


    public class ShaderGenerator
    {
        string pathToSelectedFolder;
        int height;
        int width;

        private string jsonFilePath = "Assets/MeshAnimator Suite/Smart Texture/shaderCode.json";
        ShaderData shaderData;

        public ShaderGenerator(DefaultAsset _folder, int _height, int _width)
        {
            pathToSelectedFolder = PathToFolder(_folder) + "/SmartTexture.shader";

            height = _height;
            width = _width;

            LoadJson();

            if (shaderData != null)
            {
                var shaderCodeGenerator = new ShaderCodeGenerator(shaderData, height, width);
                shaderCodeGenerator.GenerateShader(pathToSelectedFolder);

                MaterialGenerator materialGenerator = new MaterialGenerator(pathToSelectedFolder);
                materialGenerator.GenerateMaterial("SmartTexture");
            }
        }

        string PathToFolder(DefaultAsset folderAsset) => AssetDatabase.GetAssetPath(folderAsset);

        private void LoadJson()
        {
            if (File.Exists(jsonFilePath))
            {
                Debug.Log("JSON file found. Reading...");
                string json = File.ReadAllText(jsonFilePath);
                shaderData = JsonUtility.FromJson<ShaderData>(json);
                if (shaderData != null)
                {
                    Debug.Log("JSON successfully parsed.");
                }
                else
                {
                    Debug.LogError("Failed to parse JSON.");
                }
            }
            else
            {
                Debug.LogError("JSON file not found at " + jsonFilePath);
            }
        }
    }

    [System.Serializable]
    public class ShaderData
    {
        public ShaderCode shaderCode;
    }

    [System.Serializable]
    public class ShaderCode
    {
        public string[] ImmutableCode0;
        public string[] PropertiesHeight;
        public string[] PropertiesWidth;
        public string[] PropertiesValues;
        public string[] ImmutableCode1;
        public string[] SubShader;
        public string[] ImmutableCode2;
        public string[] CycleValues;
        public string[] ImmutableCode3;
    }


    public class ShaderCodeGenerator
    {
        private ShaderData shaderData;
        private int height;
        private int width;

        public ShaderCodeGenerator(ShaderData data, int _height, int _width)
        {
            shaderData = data;
            height = _height;
            width = _width;
        }

        public void GenerateShader(string outputPath)
        {
            if (shaderData == null || shaderData.shaderCode == null)
            {
                Debug.LogError("Shader data is null.");
                return;
            }

            var newPropertiesValues = new StringBuilder();
            var newSubShader = new StringBuilder();
            var newCycleValues = new StringBuilder();

            float value = 1;
            int index = 0;

            for (int j = 0; j < width; j++)
            {
                for (int i = 0; i < height; i++)
                {

                    newPropertiesValues.AppendLine($"        _x{i}y{j}(\"x{i}y{j}\", Range(0, 1)) = {value}");
                    newSubShader.AppendLine($"            float _x{i}y{j};");
                    newCycleValues.AppendLine($"                if (index == {index})");
                    newCycleValues.AppendLine($"                {{");
                    newCycleValues.AppendLine($"                    colorValue = _x{i}y{j};");
                    newCycleValues.AppendLine($"                }}");
                    index++;
                }
            }

            var sb = new StringBuilder();
            AppendLines(sb, shaderData.shaderCode.ImmutableCode0);
            AppendProperties(sb, shaderData.shaderCode.PropertiesHeight, "_Height", height.ToString());
            AppendProperties(sb, shaderData.shaderCode.PropertiesWidth, "_Width", width.ToString());
            sb.Append(newPropertiesValues.ToString());
            AppendLines(sb, shaderData.shaderCode.ImmutableCode1);
            sb.Append(newSubShader.ToString());
            AppendLines(sb, shaderData.shaderCode.ImmutableCode2);
            sb.Append(newCycleValues.ToString());
            AppendLines(sb, shaderData.shaderCode.ImmutableCode3);

            File.WriteAllText(outputPath, sb.ToString());
            Debug.Log($"Shader saved to {outputPath}");

            AssetDatabase.Refresh();
        }

        private void AppendLines(StringBuilder sb, string[] lines)
        {
            if (lines != null)
            {
                foreach (var line in lines)
                {
                    sb.AppendLine(line);
                }
            }
        }

        private void AppendProperties(StringBuilder sb, string[] lines, string propertyName, string newValue)
        {
            if (lines != null)
            {
                foreach (var line in lines)
                {
                    if (line.Contains(propertyName))
                    {
                        sb.AppendLine(line.Replace("42", newValue));
                    }
                    else
                    {
                        sb.AppendLine(line);
                    }
                }
            }
        }
    }


    public class MaterialGenerator
    {
        private Shader shader;
        private string shaderPath;

        public MaterialGenerator(string _shaderPath)
        {
            shaderPath = _shaderPath;
            LoadShader();
        }

        private void LoadShader()
        {
            shader = AssetDatabase.LoadAssetAtPath<Shader>(shaderPath);
            if (shader == null)
            {
                Debug.LogError("Shader not found at " + shaderPath);
            }
        }

        public void GenerateMaterial(string materialName)
        {
            if (shader == null)
            {
                Debug.LogError("Shader is null. Cannot create material.");
                return;
            }

            Material material = new Material(shader);

            string materialPath = Path.Combine(Path.GetDirectoryName(shaderPath), materialName + ".mat");

            AssetDatabase.CreateAsset(material, materialPath);
            Debug.Log($"Material saved to {materialPath}");

            AssetDatabase.Refresh();
        }
    }


    //First Program
    public class OSCPicture
    {
        string pathToSelectedFolder { get; set; }
        string c1C0Folder { get; set; }
        string pathToGOShader { get; set; }
        byte color1or0 { get; set; } = 1;

        ShaderParameterAggregator shaderParameterAggregator { get; set; }
        FactorizationClass factorizationClass { get; set; }

        public static List<string> FilteredPropertyNames = new List<string>();

        int width { get; set; }
        int height { get; set; }
        int sumOfPixels { get; set; }

        public Text resultText { get; set; }

        public OSCPicture(GameObject shaderGameObject, DefaultAsset folder, AnimatorController animatorController)
        {
            factorizationClass = new FactorizationClass();
            pathToSelectedFolder = PathToFolder(folder);
            pathToGOShader = PathToGOShader(shaderGameObject);

            // Find the material
            Material materialShader = FindMaterial(shaderGameObject, "SmartTexture");
            if (materialShader == null)
            {
                Debug.LogError("Material 'SmartTexture' not found on the object.");
                return;
            }

            Shader shader = materialShader.shader;
            ListOfCoordinates(shader);

            shaderParameterAggregator = new ShaderParameterAggregator();
            width = shaderParameterAggregator.GetMaxNumbers(materialShader).maxX;
            height = shaderParameterAggregator.GetMaxNumbers(materialShader).maxY;
            sumOfPixels = height * width;

            ProcessShaderProperties(materialShader, shaderGameObject, width, height, sumOfPixels, factorizationClass, 1);
            ProcessShaderProperties(materialShader, shaderGameObject, width, height, sumOfPixels, factorizationClass, 0);

            LayerCreator layerCreator = new LayerCreator(animatorController, pathToSelectedFolder, width, height, sumOfPixels, factorizationClass);
            layerCreator.CreateLayer("SmartTextureLayer");
        }

        public Material FindMaterial(GameObject gameObject, string materialName)
        {
            Renderer renderer = gameObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                foreach (Material mat in renderer.sharedMaterials)
                {
                    if (mat.name == materialName)
                    {
                        return mat;
                    }
                }
            }
            return null;
        }

        void ProcessShaderProperties(Material materialShader, GameObject shaderGameObject, int width, int height, int sumOfPixels, FactorizationClass factorizationClass, byte color)
        {
            Shader shader = materialShader.shader;

            color1or0 = color;
            c1C0Folder = CheckAndCreateFolder(pathToSelectedFolder, $"c{color1or0}");

            Debug.Log("y - " + factorizationClass.Factorization(sumOfPixels).y + " x - " + factorizationClass.Factorization(sumOfPixels).x);
            Debug.Log("height: " + height + "width: " + width);

            for (int y = 0; y <= height; y++)
            {
                for (int x = 0; x <= width; x++)
                {
                    string stateName = $"x{x}y{y}";
                    Coordinates(materialShader, stateName);
                }
            }
        }

        public static void ListOfCoordinates(Shader shader)
        {
            int propertyCount = ShaderUtil.GetPropertyCount(shader);

            for (int i = 0; i < propertyCount; i++)
            {
                string propertyName = ShaderUtil.GetPropertyName(shader, i);

                if (propertyName.Contains("_x"))
                {
                    FilteredPropertyNames.Add(propertyName);
                }
            }
        }

        void Coordinates(Material material, string propertyName)
        {
            CreateAnimation(c1C0Folder, propertyName, color1or0);
        }

        void CreateAnimation(string pathToFolder, string NameOfPixel, float ColorOfPixel)
        {
            AnimationClip clip = new AnimationClip();
            AnimationCurve curve = AnimationCurve.Linear(0, ColorOfPixel, 0, 0);
            clip.SetCurve(pathToGOShader, typeof(MeshRenderer), $"material._{NameOfPixel}", curve);

            pathToFolder = NameFileAnim(pathToFolder, NameOfPixel, ColorOfPixel);
            AssetDatabase.CreateAsset(clip, pathToFolder);
            AssetDatabase.SaveAssets();
        }

        string PathToFolder(DefaultAsset folderAsset) => AssetDatabase.GetAssetPath(folderAsset);

        string PathToGOShader(GameObject shaderGameObject)
        {
            Transform parentTransform = shaderGameObject.transform.parent;
            string path = shaderGameObject.name;

            while (parentTransform.parent != null)
            {
                shaderGameObject = parentTransform.gameObject;
                parentTransform = shaderGameObject.transform.parent;
                path = shaderGameObject.name + "/" + path;
            }

            return path;
        }

        string NameFileAnim(string pathToFolder, string NameOfPixel, float ColorOfPixel) =>
            $"{pathToFolder}/{NameOfPixel.TrimStart('_') + "c" + ColorOfPixel + ".anim"}";

        string CheckAndCreateFolder(string parentFolder, string folderName)
        {
            string folderPath = $"{parentFolder}/{folderName}";

            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                string guid = AssetDatabase.CreateFolder(parentFolder, folderName);
                folderPath = AssetDatabase.GUIDToAssetPath(guid);
                Debug.Log($"Created folder: {folderPath}");
            }
            else
            {
                Debug.Log($"Folder already exists: {folderPath}");
            }

            return folderPath;
        }
    }

    public class LayerCreator
    {
        private AnimatorController controller;
        private FactorizationClass factorizationClass;
        private string pathToSelectedFolder;
        private int sumOfPixels;
        private int width;
        private int height;
        private const int MaxBlendTreesPerLayer = 15000;

        public LayerCreator(AnimatorController controller, string pathToSelectedFolder, int width, int height, int sumOfPixels, FactorizationClass factorizationClass)
        {
            this.controller = controller;
            this.pathToSelectedFolder = pathToSelectedFolder;
            this.sumOfPixels = sumOfPixels;
            this.factorizationClass = factorizationClass;
            this.width = width;
            this.height = height;
        }

        //public AnimatorController animatorController;

        private void RemoveOldLayers(AnimatorController controller, string baseLayerName)
        {
            var layers = controller.layers;

            for (int i = layers.Length - 1; i >= 0; i--)
            {
                AnimatorControllerLayer layer = layers[i];
                if (layer.name.StartsWith(baseLayerName))
                {
                    var stateMachine = layer.stateMachine;
                    foreach (var state in stateMachine.states)
                    {
                        var stateMotion = state.state.motion;
                        if (stateMotion is BlendTree blendTree)
                        {
                            AssetDatabase.RemoveObjectFromAsset(blendTree);
                        }
                    }

                    controller.RemoveLayer(i);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }



        public void CreateLayer(string baseLayerName)
        {
            RemoveOldLayers(controller, baseLayerName);

            int blendTreeCount = 0;
            int layerIndex = 0;

            AnimatorControllerLayer layer = CreateNewLayer(baseLayerName, layerIndex, controller);

            for (int y = 0; y <= height; y++)
            {
                for (int x = 0; x <= width; x++)
                {
                    if (blendTreeCount >= MaxBlendTreesPerLayer)
                    {
                        layerIndex++;
                        layer = CreateNewLayer(baseLayerName, layerIndex, controller);
                        blendTreeCount = 0;
                    }

                    string stateName = $"x{x}y{y}";
                    SetupAnyStateTransition(layer, stateName, x, y);
                    blendTreeCount++;
                }
            }
        }


        private AnimatorControllerLayer CreateNewLayer(string baseLayerName, int index, AnimatorController animatorController)
        {
            string layerName = $"{baseLayerName}({index})";
            animatorController.AddLayer(layerName);
            var layers = animatorController.layers;

            AnimatorControllerLayer layer = layers[layers.Length - 1];
            layer.defaultWeight = 1.0f;

            animatorController.layers = layers;

            AddParameterIfNotExists(controller, "Items/OSCTablet/PixelParam", AnimatorControllerParameterType.Float, 1f);
            AddParameterIfNotExists(controller, "Items/OSCTablet/PixelX", AnimatorControllerParameterType.Int, -1);
            AddParameterIfNotExists(controller, "Items/OSCTablet/PixelY", AnimatorControllerParameterType.Int, -1);


            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            AnimatorState emptyState = layer.stateMachine.AddState("Why Are You Here?");
            layer.stateMachine.defaultState = emptyState;

            return layer;
        }

        void AddParameterIfNotExists(AnimatorController animatorController, string parameterName, AnimatorControllerParameterType type, float defaultValue = 0f)
        {
            if (!Array.Exists(animatorController.parameters, p => p.name == parameterName))
            {
                AnimatorControllerParameter newParam = new AnimatorControllerParameter
                {
                    name = parameterName,
                    type = type,
                    defaultFloat = defaultValue,
                    defaultInt = (int)defaultValue
                };

                animatorController.AddParameter(newParam);
                Debug.Log($"Параметр '{parameterName}' добавлен в AnimatorController.");
            }
            else
            {
                Debug.Log($"Параметр '{parameterName}' уже существует в AnimatorController.");
            }
        }

        private bool ParameterExists(string parameterName)
        {
            foreach (var param in controller.parameters)
            {
                if (param.name == parameterName)
                {
                    return true;
                }
            }
            return false;
        }

        AnimatorState CreateBlendTreeState(AnimatorControllerLayer layer, string stateName)
        {
            AnimatorState state = layer.stateMachine.AddState(stateName);

            BlendTree blendTree = new BlendTree
            {
                name = $"BlendTree({stateName})",
                //name = $"Blend Tree",
                blendType = BlendTreeType.Simple1D,
                blendParameter = "Items/OSCTablet/PixelParam",
                useAutomaticThresholds = false
            };

            state.motion = blendTree;
            state.writeDefaultValues = false;

            //controller.CreateBlendTreeInController($"BlendTree({stateName})", out blendTree);
            //Debug.Log("Is here? " + AssetDatabase.Contains(blendTree) + " " + AssetDatabase.Contains(state));


            AssetDatabase.AddObjectToAsset(blendTree, state);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            AddAnimationsToBlendTree(blendTree, pathToSelectedFolder, stateName);

            return state;
        }

        void AddAnimationsToBlendTree(BlendTree blendTree, string animationPath, string nameOfAnimation)
        {
            string pathC0 = $"{animationPath}/c0/{nameOfAnimation}c0.anim";
            string pathC1 = $"{animationPath}/c1/{nameOfAnimation}c1.anim";

            AnimationClip animC0 = AssetDatabase.LoadAssetAtPath<AnimationClip>(pathC0);
            AnimationClip animC1 = AssetDatabase.LoadAssetAtPath<AnimationClip>(pathC1);

            if (animC0 == null)
            {
                Debug.LogError($"Failed to load animation for c0: {pathC0}");
            }
            else
            {
                blendTree.AddChild(animC0, 0f);
            }

            if (animC1 == null)
            {
                Debug.LogError($"Failed to load animation for c1: {pathC1}");
            }
            else
            {
                blendTree.AddChild(animC1, 1f);
            }
        }

        void SetupAnyStateTransition(AnimatorControllerLayer layer, string stateName, int pixelX, int pixelY)
        {
            AnimatorState blendTreeState = CreateBlendTreeState(layer, stateName);

            AnimatorStateTransition transition = layer.stateMachine.AddAnyStateTransition(blendTreeState);
            transition.hasExitTime = false;
            transition.hasFixedDuration = true;
            transition.exitTime = 0f;
            transition.duration = 0f;

            transition.AddCondition(AnimatorConditionMode.Equals, pixelX, "Items/OSCTablet/PixelX");
            transition.AddCondition(AnimatorConditionMode.Equals, pixelY, "Items/OSCTablet/PixelY");
        }
    }


    public class ShaderParameterAggregator
    {
        public (int maxX, int maxY) GetMaxNumbers(Material material)
        {
            Shader shader = material.shader;
            int propertyCount = ShaderUtil.GetPropertyCount(shader);

            int maxX = int.MinValue;
            int maxY = int.MinValue;

            Regex xPattern = new Regex(@"x(\d+)");
            Regex yPattern = new Regex(@"y(\d+)");

            for (int i = 0; i < propertyCount; i++)
            {
                string propertyName = ShaderUtil.GetPropertyName(shader, i);

                Match xMatch = xPattern.Match(propertyName);
                if (xMatch.Success)
                {
                    int xValue = int.Parse(xMatch.Groups[1].Value);
                    if (xValue > maxX)
                    {
                        maxX = xValue;
                    }
                }

                Match yMatch = yPattern.Match(propertyName);
                if (yMatch.Success)
                {
                    int yValue = int.Parse(yMatch.Groups[1].Value);
                    if (yValue > maxY)
                    {
                        maxY = yValue;
                    }
                }
            }

            return (maxX == int.MinValue ? 0 : maxX, maxY == int.MinValue ? 0 : maxY);
        }
    }

    public class FactorizationClass
    {
        public (int y, int x) Factorization(int value)
        {
            int divisor1 = 0;
            int divisor2 = 0;
            for (int i = 1; i <= Math.Sqrt(value); i++)
            {
                if (value % i == 0)
                {
                    divisor1 = i;
                    divisor2 = value / i;
                }
            }
            return (divisor2, divisor1);
        }
    }

    // Second Program
    class CopyPathFromHierarchy
    {
        public void CopyPath()
        {
            GameObject currentGameObject = Selection.activeGameObject;

            if (currentGameObject == null)
                return;

            Transform parentTransform = currentGameObject.transform.parent;
            string path = currentGameObject.name;

            while (parentTransform != null && parentTransform.parent != null)
            {
                path = $"{parentTransform.name}/{path}";
                parentTransform = parentTransform.parent;
            }

            EditorGUIUtility.systemCopyBuffer = path;
        }

        /// <summary>
        /// Allow path copying only if 1 object is selected.Allow path copying only if 1 object is selected.
        /// </summary>
        [MenuItem("GameObject/Copy Path", true)]
        static bool CopyPathValidation()
        {
            return Selection.gameObjects.Length == 1;
        }
    }
}
#endif