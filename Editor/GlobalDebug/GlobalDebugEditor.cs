using UnityEngine;
using UnityEditor;

namespace NK.MyEditor
{
	[CustomEditor(typeof(GlobalDebug), editorForChildClasses: true)]
	public class GlobalDebugEditor : Editor
	{
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUI.enabled = Application.isPlaying;

            GlobalDebug e = target as GlobalDebug;
            if (GUILayout.Button("Activate"))
                e.Activate();
            if (GUILayout.Button("Deactivate"))
                e.Deactivate();
        }
    }
}