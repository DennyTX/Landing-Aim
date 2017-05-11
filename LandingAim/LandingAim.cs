using UnityEngine;

namespace LandingAim
{
	public class LandingAim : PartModule
    {
        [KSPField(guiActive = true, guiActiveEditor = false, guiName = "Landing Aim", isPersistant = true)]
        [UI_Toggle(controlEnabled = true, enabledText = "ON", disabledText = "OFF", scene = UI_Scene.Flight)]
        public bool IsLandingAim;

        private int _pointsCount = 128;
        private float _lineWidth = 0.1f;

        private Texture2D _crossTexture;

        private Transform _crossTransform;
        private Transform CrossTransform
        {
            get
            {
                if (_crossTransform == null)
                {
                    var obj = GameObject.CreatePrimitive(PrimitiveType.Plane);
                    var r = obj.GetComponent<Renderer>();
                    var mat = new Material(Shader.Find("Particles/Additive"));
                    mat.SetTexture("_MainTex", _crossTexture);
                    r.sharedMaterial = mat;
                    var col = obj.GetComponent<Collider>();
                    col.enabled = false;
                    _crossTransform = obj.transform;
                }
                return _crossTransform;
            }
        }

        private readonly LayerMask _targetLayer = LayerMask.GetMask("TerrainColliders", "PhysicalObjects", "EVA", "Local Scenery", "Water");
        private LineRenderer Line { get; set; }

        public override void OnStart(StartState state)

        {
            _crossTexture = GameDatabase.Instance.GetTexture("OLDD/LandingAim/AimCross", false);
			Line = gameObject.AddComponent<LineRenderer>();
			Line.useWorldSpace = true;
		    Line.enabled = IsLandingAim;
			Line.SetVertexCount(_pointsCount);
            Line.SetWidth(_lineWidth, _lineWidth);
			Line.sharedMaterial = Resources.Load("DefaultLine3D") as Material;
			Line.material.SetColor("_TintColor", new Color(0.1f, 1f, 0.1f));
		}

        private void FixedUpdate()
		{
			if (IsLandingAim)
			{
				if(!Line.enabled) Line.enabled = true;

                var pos = FlightGlobals.ActiveVessel.transform.position;
                var dragVector = FlightGlobals.activeTarget.dragVector;
				var grav = FlightGlobals.ActiveVessel.graviticAcceleration;
				var drawTime = 0;

                RaycastHit hit;

                for (var i = 1; i < 10; i++)
                {
                    drawTime++;
					var lastPos = pos;
					dragVector += grav*i;
					pos += dragVector*i;
					if(Physics.Linecast(lastPos, pos, out hit, _targetLayer)) break;
				}

				dragVector = FlightGlobals.activeTarget.dragVector;
				pos = FlightGlobals.ActiveVessel.transform.position;

				Vector3[] poses = new Vector3[_pointsCount];

				var interval = (float)drawTime/poses.Length;

				var checkContact = true;
				poses[0] = pos;
				for(var i = 1; i < poses.Length; i++)
				{
					var lastPos = pos;
					dragVector += grav*(i*interval);
					pos += dragVector*(i*interval);
					poses[i] = pos;
					if(checkContact && Physics.Linecast(lastPos, pos, out hit, _targetLayer))
					{
						checkContact = false;
					    CrossTransform.position = hit.point + hit.normal*0.16f;
						CrossTransform.localEulerAngles = Quaternion.FromToRotation(Vector3.up, hit.normal).eulerAngles;
					}
				}
				Line.SetPositions(poses);
			}
			else
			{
				if(Line.enabled) Line.enabled = false;
			    if (_crossTransform != null) _crossTransform.gameObject.DestroyGameObject();
			}
		}
	}
}