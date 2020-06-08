using UnityEngine;

namespace BezierTool
{
	public class BezierAccesser : MonoBehaviour
	{
		private ICombinableCurve curve;

		public ICombinableCurve GetCurve()
		{
			if(curve == null)
				curve = GetComponent<ICombinableCurve>();
			return curve;
		}

		private void Awake()
		{
			curve = GetComponent<ICombinableCurve>();
		}

		public void OnValidate()
		{
			curve = GetComponent<ICombinableCurve>();
		}
	}
}
