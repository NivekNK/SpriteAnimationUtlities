using UnityEngine;
using UnityEditor;

// Fork of: https://answers.unity.com/questions/1165627/editor-build-save-animations-in-scripts.html

namespace NK.MyEditor
{
    public class SpriteAnimationCreator : EditorWindow
    {
        public Texture2D spriteSheet;
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

        private Sprite[] _sprites;

        private readonly string prefsKey = "NK.SpriteAnimationReferences";
        private SpriteAnimationReferences currentReferences;

        private void OnEnable()
        {
            LoadSavedPreferences();
        }

        private void OnDisable()
        {
            SavePreferences();
        }

        [MenuItem("Tools/NK/Sprite Animator")]
        private static void InitSpriteAnimator()
        {
            SpriteAnimationCreator window = GetWindow(typeof(SpriteAnimationCreator), false, "Sprite Animator", true) as SpriteAnimationCreator;
            window.minSize = new Vector2(350, 450);
            window.maxSize = new Vector2(350, 450);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Animation Settings:", EditorStyles.boldLabel);
            spriteSheet = (Texture2D)EditorGUILayout.ObjectField("Sprite Sheet:", spriteSheet, typeof(Texture2D), true);
            animWidth = EditorGUILayout.IntField("Single Sprite Width:", animWidth);
            animHeight = EditorGUILayout.IntField("Single Sprite Height:", animHeight);
            numOfanimations = EditorGUILayout.IntField("Number of Animations:", numOfanimations);
            numOfFrames = EditorGUILayout.IntField("Frames per Animation:", numOfFrames);
            samplesFrameRate = EditorGUILayout.IntField("Samples Frame Rate", samplesFrameRate);
            loop = EditorGUILayout.Toggle("Loop:", loop);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Texture Settings:", EditorStyles.boldLabel);
            pixelPerUnit = EditorGUILayout.IntField("Pixel Per Unit:", pixelPerUnit);
            alphaIsTransparency = EditorGUILayout.Toggle("Alpha is Transparency:", alphaIsTransparency);
            compression = (TextureImporterCompression)EditorGUILayout.EnumPopup("Texture Compression:", compression);
            wrapMode = (TextureWrapMode)EditorGUILayout.EnumPopup("Wrap Mode:", wrapMode);
            filterMode = (FilterMode)EditorGUILayout.EnumPopup("Filter Mode:", filterMode);

            EditorGUILayout.Space();

            if (GUILayout.Button("Generate Animation"))
            {
                if (spriteSheet != null)
                {
                    CutSprites();
                    MakeAnimation();
                }
                else
                {
                    Debug.LogWarning("Forgot to assign Image!");
                }
            }

            Repaint();
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
                // Modify this line of code to change where the animation will be saved
                AssetDatabase.CreateAsset(animClip, string.Format("Assets/aAssets/Animations/ScriptCreatedAnimations/{0}.anim", spriteSheet.name + "_" + j));
                AssetDatabase.SaveAssets();
            }

            Debug.Log(string.Format("Animations of {0} Created!", spriteSheet.name));
        }

        private void LoadSavedPreferences()
        {
            var serializedPrefs = EditorPrefs.GetString(prefsKey);
            if (!string.IsNullOrEmpty(serializedPrefs))
            {
                currentReferences = JsonUtility.FromJson<SpriteAnimationReferences>(serializedPrefs);
            }

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
        }

        private void SavePreferences()
        {
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

            EditorPrefs.SetString(prefsKey, JsonUtility.ToJson(currentReferences));
        }
    }
}
