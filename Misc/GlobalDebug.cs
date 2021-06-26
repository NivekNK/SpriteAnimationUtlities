#if UNITY_EDITOR
using UnityEngine;

namespace NK
{
	[CreateAssetMenu(menuName = "Helpers/GlobalDebug", fileName = "GlobalDebug")]
	public class GlobalDebug : ScriptableObject
	{
		public static bool activeGlobalDebug;

		public void Activate()
        {
			activeGlobalDebug = true;
		}

		public void Deactivate()
        {
			activeGlobalDebug = false;
        }

		public static void Log(object message)
		{
			if (activeGlobalDebug)
			{
				Debug.Log(message);
			}
		}

		public static void LogWarning(string message)
		{
			if (activeGlobalDebug)
			{
				Debug.LogWarning(message);
			}
		}

		public static void LogError(string message)
		{
			if (activeGlobalDebug)
			{
				Debug.LogError(message);
			}
		}

		public static void DrawRay(Vector3 start, Vector3 direction, Color color, float duration, bool depthTest)
        {
			if (activeGlobalDebug)
			{
				Debug.DrawRay(start, direction, color, duration, depthTest);
			}
		}
	}
}
#endif