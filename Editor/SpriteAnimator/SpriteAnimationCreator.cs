using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

// Fork of: https://answers.unity.com/questions/1165627/editor-build-save-animations-in-scripts.html

namespace NK.MyEditor
{
    public class SpriteAnimationCreator : EditorWindow
    {
        public string animName;
        public bool notUseSpriteSheet;
        public Texture2D spriteSheet;
        public List<Sprite> spriteList = new List<Sprite>();
        public int animWidth;
        public int animHeight;
        public int numOfanimations;
        public int numOfFrames;
        public int samplesFrameRate;
        public bool loop;

        public int pixelPerUnit;
        public bool alphaIsTransparency;
        public TextureImporterCompression compression;
        public TextureWrapMode wrapMode;
        public FilterMode filterMode;

        public bool hasDirections;
        public string[] directions;

        private Sprite[] _sprites;
        private Vector2 scrollPos = Vector2.zero;

        private readonly string prefsKey = "NK.SpriteAnimationReferences";
        private SpriteAnimationReferences currentReferences;

        private void OnEnable()
        {
            LoadPreferences();
        }

        private void OnDisable()
        {
            SavePreferences();
        }

        [MenuItem("Tools/NK/Sprite Animator")]
        private static void InitSpriteAnimator()
        {
            SpriteAnimationCreator window = GetWindow(typeof(SpriteAnimationCreator), false, "Sprite Animator", true) as SpriteAnimationCreator;
            window.minSize = new Vector2(380, 430);
            window.Show();
        }

        private void OnGUI()
        {
            GUI.enabled = spriteSheet != null || (spriteList.Count > 0 && !CheckIfNull(spriteList));
            if (GUILayout.Button("Generate Animation"))
            {
                if (spriteSheet != null || spriteList.Count > 0)
                {
                    if (!notUseSpriteSheet)
                    {
                        CutSprites();
                    }
                    else
                    {
                        _sprites = spriteList.ToArray();
                    }
                    MakeAnimation();
                }
                else
                {
                    Debug.LogWarning("Forgot to assign Images!");
                }
            }
            GUI.enabled = true;

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandHeight(true));

            EditorGUILayout.LabelField("Animation Settings:", EditorStyles.boldLabel);

            animName = EditorGUILayout.TextField("Animation Name:", animName);
            notUseSpriteSheet = EditorGUILayout.Toggle("Use Single Sprites:", notUseSpriteSheet);
            if (!notUseSpriteSheet)
            {
                spriteSheet = (Texture2D)EditorGUILayout.ObjectField("Sprite Sheet:", spriteSheet, typeof(Texture2D), true);
            }
            else
            {
                if (spriteSheet != null) spriteSheet = null;
                SetArrayProperty("spriteList");
            }

            hasDirections = EditorGUILayout.Toggle("Has directions:", hasDirections);
            if (hasDirections) SetArrayProperty("directions");
            else if (directions.Length > 0) ArrayUtility.Clear(ref directions);

            animWidth = EditorGUILayout.IntField("Single Sprite Width:", animWidth);
            animHeight = EditorGUILayout.IntField("Single Sprite Height:", animHeight);
            numOfanimations = EditorGUILayout.IntField("Number of Animations:", numOfanimations);
            numOfFrames = EditorGUILayout.IntField("Frames per Animation:", numOfFrames);
            samplesFrameRate = EditorGUILayout.IntField("Samples Frame Rate", samplesFrameRate);
            loop = EditorGUILayout.Toggle("Loop:", loop);

            GUI.enabled = spriteSheet != null;
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Texture Settings:", EditorStyles.boldLabel);
            pixelPerUnit = EditorGUILayout.IntField("Pixel Per Unit:", pixelPerUnit);
            alphaIsTransparency = EditorGUILayout.Toggle("Alpha is Transparency:", alphaIsTransparency);
            compression = (TextureImporterCompression)EditorGUILayout.EnumPopup("Texture Compression:", compression);
            wrapMode = (TextureWrapMode)EditorGUILayout.EnumPopup("Wrap Mode:", wrapMode);
            filterMode = (FilterMode)EditorGUILayout.EnumPopup("Filter Mode:", filterMode);
            GUI.enabled = true;

            EditorGUILayout.Space();

            EditorGUILayout.EndScrollView();

            Repaint();
        }

        private bool CheckIfNull(List<Sprite> sprites)
        {
            foreach (Sprite sprite in sprites)
            {
                if (sprite == null)
                    return true;
            }

            return false;
        }

        private void SetArrayProperty(string name)
        {
            ScriptableObject target = this;
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(name);
            EditorGUILayout.PropertyField(property, true);
            serializedObject.ApplyModifiedProperties();
        }

        private void CutSprites()
        {
            SpriteUtils.SliceSprites(animWidth, animHeight, new Texture2D[] { spriteSheet }, pixelPerUnit, compression, alphaIsTransparency, wrapMode, filterMode);

            Object[] _objects = AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetDatabase.GetAssetPath(spriteSheet));

            if (_objects != null && _objects.Length > 0)
                _sprites = new Sprite[_objects.Length];

            for (int i = 0; i < _objects.Length; i++)
            {
                _sprites[i] = _objects[i] as Sprite;
            }
        }

        private void MakeAnimation()
        {
            if (numOfanimations * numOfFrames != _sprites.Length)
            {
                Debug.LogError("Number of animations or frames per animations are not set correctly!");
                return;
            }

            string newName = string.IsNullOrEmpty(animName) ? (spriteSheet != null ? spriteSheet.name : "default") : animName;
            EditorCurveBinding curveBinding = EditorCurveBinding.PPtrCurve(string.Empty, typeof(SpriteRenderer), "m_Sprite");

            int i = 0;
            for (int j = 0; j < numOfanimations; j++)
            {
                ObjectReferenceKeyframe[] keyFrames = new ObjectReferenceKeyframe[numOfFrames];
                for (int k = 0; k < numOfFrames; k++)
                {
                    keyFrames[k] = new ObjectReferenceKeyframe
                    {
                        value = _sprites[i],
                        time = (float)k / samplesFrameRate
                    };
                    i++;
                }

                AnimationClip animClip = new AnimationClip { frameRate = samplesFrameRate };
                var settings = AnimationUtility.GetAnimationClipSettings(animClip);
                settings.loopTime = loop;
                AnimationUtility.SetAnimationClipSettings(animClip, settings);

                AnimationUtility.SetObjectReferenceCurve(animClip, curveBinding, keyFrames);
                string clipName = hasDirections ? newName + "_" + directions[j] : newName + "_" + j;
                // Modify this line of code to change where the animation will be saved
                AssetDatabase.CreateAsset(animClip, string.Format("Assets/Animations/ScriptCreatedAnimations/{0}.anim", clipName));
                AssetDatabase.SaveAssets();
            }

            Debug.Log(string.Format("Animations of {0} Created!", newName));
        }

        private void LoadPreferences()
        {
            var serializedPrefs = EditorPrefs.GetString(prefsKey);
            if (!string.IsNullOrEmpty(serializedPrefs))
            {
                currentReferences = JsonUtility.FromJson<SpriteAnimationReferences>(serializedPrefs);
            }

            notUseSpriteSheet = currentReferences.notUseSpriteSheet;
            animWidth = currentReferences.animWidth;
            animHeight = currentReferences.animHeight;
            numOfanimations = currentReferences.numOfanimations;
            numOfFrames = currentReferences.numOfFrames;
            samplesFrameRate = currentReferences.samplesFrameRate;
            loop = currentReferences.loop;

            pixelPerUnit = currentReferences.pixelPerUnit;
            alphaIsTransparency = currentReferences.alphaIsTransparency;
            compression = currentReferences.compression;
            wrapMode = currentReferences.wrapMode;
            filterMode = currentReferences.filterMode;

            hasDirections = currentReferences.hasDirections;
            directions = currentReferences.directions;
        }

        private void SavePreferences()
        {
            currentReferences.notUseSpriteSheet = notUseSpriteSheet;
            currentReferences.animWidth = animWidth;
            currentReferences.animHeight = animHeight;
            currentReferences.numOfanimations = numOfanimations;
            currentReferences.numOfFrames = numOfFrames;
            currentReferences.samplesFrameRate = samplesFrameRate;
            currentReferences.loop = loop;

            currentReferences.pixelPerUnit = pixelPerUnit;
            currentReferences.alphaIsTransparency = alphaIsTransparency;
            currentReferences.compression = compression;
            currentReferences.wrapMode = wrapMode;
            currentReferences.filterMode = filterMode;

            currentReferences.hasDirections = hasDirections;
            currentReferences.directions = directions;

            EditorPrefs.SetString(prefsKey, JsonUtility.ToJson(currentReferences));
        }
    }
}