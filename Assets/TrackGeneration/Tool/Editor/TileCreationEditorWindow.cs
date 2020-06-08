using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TileCreationEditorWindow : EditorWindow
{
	[MenuItem("Window/Generation Tiles/Tile creator window")]
	public static void ShowWindow()
	{
		EditorWindow.GetWindow(typeof(TileCreationEditorWindow), false, "Generation tile dashboard");
	}

	private class TileCreationValues
	{
		public string tileName = "New tile";

		// Tile components
		public GenerationTileController tileController = null;
		public GenerationTile generationTile = null;
		public GenerationTilePropController propController = null;
		// Tile child components
		public BezierTool.BezierAccesser spline = null;
		public RoadSegment road = null;

		public bool isResetting = false;
		public bool displayTileId = false;
		public bool displaySplinePlacementSettings = false;
		public bool displayPropSettings = false;

		public bool isAssigningSubmesh = false;
		public bool isAssigningNewProp = false;
		public bool isOpenedResetMenu = false;

		public MeshCrossection newShapeToAdd = null;
		public Material newMaterialToAdd = null;
		public GenerationProp propToAdd = null;
		public GenerationPropTypes propType = GenerationPropTypes.Decoration;
		public int propAmountToAdd = 30;
		public bool spacePropsEqually = false;

		public string layerMaskToAssign = "Terrain";
	}

	private TileCreationValues values = new TileCreationValues();

	Vector2 scrollPos;
	private float bigButtonHeight = 40;
	private float smallButtonHeight = 26;
	private float smallButtonWidth = 250;

	public void OnGUI()
	{
		GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
		style.fontSize = 24;
		EditorGUILayout.BeginVertical();
		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Label("Generation tile dashboard", style);
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.EndVertical();

		EditorGUILayout.BeginHorizontal();
		GUILayout.Label("Tile to edit: ");
		GenerationTileController gtc = values.tileController;
		gtc = EditorGUILayout.ObjectField(gtc, typeof(GenerationTileController), true) as GenerationTileController;
		if(gtc != values.tileController)
		{
			values.tileController = gtc;
			values.generationTile = values.tileController.GetComponent<GenerationTile>();
			values.propController = values.tileController.GetComponent<GenerationTilePropController>();
			values.spline = values.tileController.GetComponentInChildren<BezierTool.BezierAccesser>();
			values.road = values.tileController.GetComponentInChildren<RoadSegment>();
		}
		EditorGUILayout.EndHorizontal();
		#region Reset button stack
		EditorGUILayout.BeginVertical();
		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if(values.tileController == null)
		{
			if(GUILayout.Button("Spawn new tile", GUILayout.Height(bigButtonHeight), GUILayout.Width(smallButtonWidth)))
			{
				CreateNewTileController();
			}
		}
		else
		{
			if(values.isResetting)
			{
				EditorGUILayout.BeginHorizontal();
				GUILayout.Label("Are you sure you want to reset?", "tooltip", GUILayout.Height(bigButtonHeight));
				if(GUILayout.Button("Reset", GUILayout.Height(bigButtonHeight))) ResetWindow();
				if(GUILayout.Button("Cancel", GUILayout.Height(bigButtonHeight))) values.isResetting = false;
				EditorGUILayout.EndHorizontal();
			}
			else if(GUILayout.Button("Reset tool", GUILayout.Height(bigButtonHeight), GUILayout.Width(smallButtonWidth)))
			{
				values.isResetting = true;
			}
		}
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.EndVertical();
		#endregion

		scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

		if(values.tileController != null)
		{
			DisplayTileBaseSettings();

			EditorGUILayout.BeginHorizontal();
			if(GUILayout.Button("Toggle display tile connection details " + ((values.displayTileId) ? "off" : "on"), GUILayout.Height(bigButtonHeight)))
				values.displayTileId = !values.displayTileId;
			if(values.displayTileId)
				DisplayTileID();
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			if(GUILayout.Button("Toggle tile generation settings " + ((values.displaySplinePlacementSettings) ? "off" : "on"), GUILayout.Height(bigButtonHeight)))
				values.displaySplinePlacementSettings = !values.displaySplinePlacementSettings;
			if(values.displaySplinePlacementSettings)
				DisplayTilePlacementSettings();
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			if(GUILayout.Button("Toggle tile prop settings " + ((values.displayPropSettings) ? "off" : "on"), GUILayout.Height(bigButtonHeight)))
				values.displayPropSettings = !values.displayPropSettings;
			if(values.displayPropSettings)
				DisplayPropSettings();
			EditorGUILayout.EndHorizontal();
		}

		EditorGUILayout.EndScrollView();
	}

	private void ResetWindow()
	{
		values = new TileCreationValues();
	}

	private void CreateNewTileController()
	{
		GameObject go = new GameObject();
		go.transform.position = Vector3.zero;
		go.name = values.tileName;

		GameObject goChild = new GameObject();
		goChild.transform.parent = go.transform;
		goChild.transform.localPosition = Vector3.zero;
		goChild.name = "Spline";

		GameObject entry = new GameObject();
		entry.transform.parent = go.transform;
		entry.name = "Entry";
		GameObject exit = new GameObject();
		exit.transform.parent = go.transform;
		exit.name = "Exit";

		values.road = goChild.AddComponent<RoadSegment>();
		goChild.AddComponent<BezierTool.SplineBezierCurve>();
		values.spline = goChild.GetComponent<BezierTool.BezierAccesser>();

		values.generationTile = go.AddComponent<GenerationTile>();
		values.tileController = go.AddComponent<GenerationTileController>();
		values.propController = go.AddComponent<GenerationTilePropController>();

		values.tileController.entryPoint = entry.transform;
		values.tileController.exitPoint = exit.transform;
		values.tileController.progressCurve = values.spline;
		values.tileController.road = values.road;
		values.tileController.propController = values.propController;

		values.propController.spline = values.spline;

		values.road.values = new RoadSegmentValues();
		values.road.values.spline = goChild.GetComponent<BezierTool.SplineBezierCurve>();

		values.road.values.layerToAssign = LayerMask.NameToLayer(values.layerMaskToAssign);

		values.road.values.spline.Reset();
	}

	public void DisplayTileID()
	{
		EditorGUILayout.BeginVertical();
		GenerationConnectionID connection = values.generationTile.connectionID;

		if(!values.generationTile.connectionID.IsValid())
		{
			EditorGUILayout.HelpBox("The connection ID is invalid. Make sure the direction values don't match or are 'none'", MessageType.Error);
		}

		EditorGUI.indentLevel++;

		EditorGUILayout.LabelField("Entry ID");
		EditorGUI.indentLevel++;
		EditorGUILayout.LabelField("Direction");
		connection.entry.dir = (CardinalDirections)EditorGUILayout.EnumPopup(connection.entry.dir);
		EditorGUI.indentLevel++;
		EditorGUILayout.LabelField("Identifier");
		for(int i = 0; i < ConnectionID.CONNECTION_SIZE; i++)
		{
			connection.entry.id.connectionID[i] = (ConnectionVariations)EditorGUILayout.EnumPopup(connection.entry.id.connectionID[i]);
		}
		EditorGUI.indentLevel -= 2;
		GUILayout.Label("");
		EditorGUILayout.LabelField("Exit ID");
		EditorGUI.indentLevel++;
		EditorGUILayout.LabelField("Direction");
		connection.exit.dir = (CardinalDirections)EditorGUILayout.EnumPopup(connection.exit.dir);
		EditorGUI.indentLevel++;
		EditorGUILayout.LabelField("Identifier");
		for(int i = 0; i < ConnectionID.CONNECTION_SIZE; i++)
		{
			connection.exit.id.connectionID[i] = (ConnectionVariations)EditorGUILayout.EnumPopup(connection.exit.id.connectionID[i]);
		}
		GUILayout.Label("");
		EditorGUI.indentLevel = 0;

		values.generationTile.OnValidate();
		EditorGUILayout.EndVertical();
	}

	public void DisplayTileBaseSettings()
	{
		EditorGUILayout.BeginHorizontal();
		GUILayout.Label("Tile name: ");
		values.tileName = values.tileController.transform.name;
		values.tileName = EditorGUILayout.TextField(values.tileName);
		if(values.tileName.Length >= 3 && values.tileName != "")
			values.tileController.transform.name = values.tileName;
		EditorGUILayout.EndHorizontal();

		Vector2Int v2int = new Vector2Int(Mathf.FloorToInt(values.tileController.tileDimensions.x), Mathf.FloorToInt(values.tileController.tileDimensions.y));
		values.tileController.tileDimensions = EditorGUILayout.Vector2IntField("Tile dimensions: ", v2int);

		float val = values.tileController.rampAngle;
		val = EditorGUILayout.FloatField("Slope angle: ",values.tileController.rampAngle);
		val = Mathf.Clamp(val, 0, 85);
		values.tileController.rampAngle = val;

		int amt = values.road.values.edgeRingCount;
		amt = EditorGUILayout.IntField("Edge ring count: ", values.road.values.edgeRingCount);
		amt = Mathf.Clamp(amt, 5, 128);
		values.road.values.edgeRingCount = amt;

		DisplayMeshMenu();
	}

	public void DisplayMeshMenu()
	{
		EditorGUILayout.BeginHorizontal();
		GUILayout.Label("Base road mesh: ");
		MeshCrossection shape = values.road.shape2D;
		shape = EditorGUILayout.ObjectField(values.road.shape2D, typeof(MeshCrossection), true) as MeshCrossection;
		if(shape != values.road.shape2D)
		{
			values.road.shape2D = shape;
			if(values.road.materialToAssign != null)
				values.road.GenerateMeshAndInit();
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();
		GUILayout.Label("Base road material: ");
		Material m = values.road.materialToAssign;
		m = EditorGUILayout.ObjectField(values.road.materialToAssign, typeof(Material), false) as Material;
		if(m != values.road.materialToAssign)
		{
			values.road.materialToAssign = m;
			if(values.road.shape2D != null)
				values.road.GenerateMeshAndInit();
		}
		EditorGUILayout.EndHorizontal();
		// add in submeshes when buttons are pressed

		if(values.isAssigningSubmesh)
		{
			GUILayout.Label("");
			EditorGUI.indentLevel++;
			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("New submesh: ");
			MeshCrossection s = values.newShapeToAdd;
			s = EditorGUILayout.ObjectField(values.newShapeToAdd, typeof(MeshCrossection), false) as MeshCrossection;
			if(s != values.newShapeToAdd)
			{
				values.newShapeToAdd = s;
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("Submesh material: ");
			Material mN = values.newMaterialToAdd;
			mN = EditorGUILayout.ObjectField(values.newMaterialToAdd, typeof(Material), false) as Material;
			if(mN != values.newMaterialToAdd)
			{
				values.newMaterialToAdd = mN;
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			if(values.newShapeToAdd == null || values.newMaterialToAdd == null)
			{
				EditorGUILayout.HelpBox("Make sure you assign a new MeshCrossection AND a new material.", MessageType.Error);
			}
			else
			{
				if(GUILayout.Button("Add submesh", GUILayout.Height(bigButtonHeight)))
				{
					values.road.AddNewSubmesh(values.newShapeToAdd, values.newMaterialToAdd);
					values.isAssigningSubmesh = false;
				}
			}
			if(GUILayout.Button("Cancel", GUILayout.Height(bigButtonHeight))) values.isAssigningSubmesh = false;
			EditorGUILayout.EndHorizontal();
			EditorGUI.indentLevel--;
		}
		else
		{
			EditorGUILayout.BeginVertical();
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if(GUILayout.Button("Add new submesh", GUILayout.Height(smallButtonHeight), GUILayout.Width(smallButtonWidth)))
							values.isAssigningSubmesh = true;
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();
		}
		GUILayout.Label("");
	}

	public void DisplayTilePlacementSettings()
	{
		if(!values.generationTile.connectionID.IsValid())
		{
			EditorGUILayout.HelpBox("The connection ID is invalid. Make sure the direction values don't match or are 'none'", MessageType.Error);
		}
		else if(values.road.shape2D == null || values.road.materialToAssign == null)
		{
			EditorGUILayout.HelpBox("Make sure the tile is assigned at least a base mesh and material before proceeding.", MessageType.Error);
		}
		else
		{
			// A lot of editor clutter to make sure the buttons are centered.
			EditorGUILayout.BeginVertical();
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			{
				if(GUILayout.Button("Set entry and exit points", GUILayout.Height(smallButtonHeight), GUILayout.Width(smallButtonWidth)))
					values.tileController.SetTilePointsAndAnchors();
			}
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			{
				if(GUILayout.Button("Smooth out curve", GUILayout.Height(smallButtonHeight), GUILayout.Width(smallButtonWidth)))
					values.tileController.AlignAnchors();
			}
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			{
				if(GUILayout.Button("Generate mesh along spline", GUILayout.Height(smallButtonHeight), GUILayout.Width(smallButtonWidth)))
					values.tileController.SetMesh();
			}
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			{
				if(GUILayout.Button("Generate props", GUILayout.Height(smallButtonHeight), GUILayout.Width(smallButtonWidth)))
					values.tileController.SetProps();
			}
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			{
				if(GUILayout.Button("Remove prop instances", GUILayout.Height(smallButtonHeight), GUILayout.Width(smallButtonWidth)))
					values.propController.RemoveProps();
			}
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			{
				if(GUILayout.Button("Completely regenerate tile", GUILayout.Height(smallButtonHeight), GUILayout.Width(smallButtonWidth)))
				{
					values.road.values.layerToAssign = LayerMask.NameToLayer(values.layerMaskToAssign);
					values.tileController.CompletelyRegenerateTile();
				}
			}
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();
		}
	}

	public void DisplayPropSettings()
	{
		EditorGUILayout.BeginVertical();

		EditorGUILayout.BeginHorizontal();
		GUILayout.Label("Add a new prop: ");
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		GUILayout.Label("New prop: ");
		GenerationProp prop = values.propToAdd;
		prop = EditorGUILayout.ObjectField(values.propToAdd, typeof(GenerationProp), false) as GenerationProp;
		if(prop != values.propToAdd)
		{
			values.propToAdd = prop;
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		GUILayout.Label("Prop type: ");
		values.propType = (GenerationPropTypes)EditorGUILayout.EnumPopup(values.propType);
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();
		GUILayout.Label("Space props equally along spline: ");
		values.spacePropsEqually = EditorGUILayout.Toggle(values.spacePropsEqually);
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();
		GUILayout.Label("Amount to add: ");
		values.propAmountToAdd = EditorGUILayout.IntField(values.propAmountToAdd);
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		if(values.propToAdd == null || values.propAmountToAdd <= 0)
		{
			EditorGUILayout.HelpBox("Make sure you assign a new prop and have a valid amount above 0.", MessageType.Error);
		}
		else
		{
			if(GUILayout.Button("Add prop", GUILayout.Height(bigButtonHeight)))
			{
				AddProps();
				values.propToAdd = null;
				values.propType = GenerationPropTypes.Decoration;
				values.propAmountToAdd = 30;
				values.spacePropsEqually = false;
				values.isAssigningNewProp = false;
			}
		}
		if(GUILayout.Button("Cancel", GUILayout.Height(bigButtonHeight)))
		{
			values.propToAdd = null;
			values.propType = GenerationPropTypes.Decoration;
			values.propAmountToAdd = 30;
			values.spacePropsEqually = false;
			values.isAssigningNewProp = false;
		}
		EditorGUILayout.EndHorizontal();
		EditorGUI.indentLevel--;


		GUILayout.Label("");

		EditorGUILayout.BeginVertical();
		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		{
			EditorGUILayout.BeginVertical();
			EditorGUILayout.BeginHorizontal();

			{
				if(GUILayout.Button("Toggle reset menu " + ((values.isOpenedResetMenu) ? "off" : "on"), GUILayout.Height(smallButtonHeight), GUILayout.Width(smallButtonWidth)))
					values.isOpenedResetMenu = !values.isOpenedResetMenu;
			}

			EditorGUILayout.EndHorizontal();

			if(values.isOpenedResetMenu)
			{
				//EditorGUILayout.BeginVertical();
				EditorGUILayout.BeginHorizontal();
				if(GUILayout.Button("Reset decor props", GUILayout.Height(smallButtonHeight), GUILayout.Width(smallButtonWidth * 0.5f)))
				{
					RemoveDeco();
				}
				if(GUILayout.Button("Reset reward props", GUILayout.Height(smallButtonHeight), GUILayout.Width(smallButtonWidth * 0.5f)))
				{
					RemoveReward();
				}
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.BeginHorizontal();
				if(GUILayout.Button("Reset hazard props", GUILayout.Height(smallButtonHeight), GUILayout.Width(smallButtonWidth * 0.5f)))
				{
					RemoveHazard();
				}
				if(GUILayout.Button("Reset all props", GUILayout.Height(smallButtonHeight), GUILayout.Width(smallButtonWidth * 0.5f)))
				{
					RemoveAllProps();
				}
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndVertical();
		}
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.EndVertical();
		GUILayout.Label("");
		EditorGUILayout.EndVertical();
	}

	private void AddProps()
	{
		if(values.propAmountToAdd == 0)
			return;

		float segmentSize = 1f / (float)values.propAmountToAdd;
		switch(values.propType)
		{
			case GenerationPropTypes.Decoration:
				for(int i = 0; i < values.propAmountToAdd; i++)
				{
					GenerationProp pr = Instantiate(values.propToAdd.gameObject).GetComponent<GenerationProp>();
					if(values.spacePropsEqually)
					{
						if(values.propAmountToAdd == 1)
							pr.localPropPos.positionOnSpline = 0.5f;

						if(segmentSize * i == 0)
							pr.localPropPos.positionOnSpline = 0.005f;
						else
							pr.localPropPos.positionOnSpline = segmentSize * i;
					}
					pr.transform.parent = values.propController.transform;
					if(values.propController.DecorativeGenerationProps == null)
						values.propController.DecorativeGenerationProps = new List<GenerationProp>();
					values.propController.DecorativeGenerationProps.Add(pr);
				}
				break;
			case GenerationPropTypes.Hazard:
				for(int i = 0; i < values.propAmountToAdd; i++)
				{
					GenerationProp pr = Instantiate(values.propToAdd.gameObject).GetComponent<GenerationProp>();
					if(values.spacePropsEqually)
					{
						if(values.propAmountToAdd == 1)
							pr.localPropPos.positionOnSpline = 0.5f;

						if(segmentSize * i == 0)
							pr.localPropPos.positionOnSpline = 0.005f;
						else
							pr.localPropPos.positionOnSpline = segmentSize * i;
					}
					pr.transform.parent = values.propController.transform;
					if(values.propController.HazardGenerationProps == null)
						values.propController.HazardGenerationProps = new List<GenerationProp>();
					values.propController.HazardGenerationProps.Add(pr);
				}
				break;
			case GenerationPropTypes.Reward:
				for(int i = 0; i < values.propAmountToAdd; i++)
				{
					GenerationProp pr = Instantiate(values.propToAdd.gameObject).GetComponent<GenerationProp>();
					if(values.spacePropsEqually)
					{
						if(values.propAmountToAdd == 1)
							pr.localPropPos.positionOnSpline = 0.5f;

						if(segmentSize * i == 0)
							pr.localPropPos.positionOnSpline = 0.005f;
						else
							pr.localPropPos.positionOnSpline = segmentSize * i;
					}
					pr.transform.parent = values.propController.transform;
					if(values.propController.RewardGenerationProps == null)
						values.propController.RewardGenerationProps = new List<GenerationProp>();
					values.propController.RewardGenerationProps.Add(pr);
				}
				break;
		}
	}

	private void RemoveDeco()
	{
		GenerationTilePropController p = values.propController;

		for(int i = p.DecorativeGenerationProps.Count - 1; i >= 0; i--)
		{
			if(p.DecorativeGenerationProps[i] != null)
			{
				p.DecorativeGenerationProps[i].RemoveProp();
				DestroyImmediate(p.DecorativeGenerationProps[i].gameObject);
			}
		}
		p.DecorativeGenerationProps.Clear();
	}

	private void RemoveReward()
	{
		GenerationTilePropController p = values.propController;

		for(int i = p.RewardGenerationProps.Count - 1; i >= 0; i--)
		{
			if(p.RewardGenerationProps[i] != null)
			{
				p.RewardGenerationProps[i].RemoveProp();
				DestroyImmediate(p.RewardGenerationProps[i].gameObject);
			}
		}
		p.RewardGenerationProps.Clear();
	}

	private void RemoveHazard()
	{
		GenerationTilePropController p = values.propController;

		for(int i = p.HazardGenerationProps.Count - 1; i >= 0; i--)
		{
			if(p.HazardGenerationProps[i] != null)
			{
				p.HazardGenerationProps[i].RemoveProp();
				DestroyImmediate(p.HazardGenerationProps[i].gameObject);
			}
		}
		p.HazardGenerationProps.Clear();
	}

	private void RemoveAllProps()
	{
		RemoveDeco();
		RemoveHazard();
		RemoveReward();
	}

	public void Phase2()
	{
		// default mesh setup is snowroad + snowroad sides
		// default material is snow all around
		// pick hazards. -> selection via headers and ints so you can enter decoratives that way
	}

	public void Phase3()
	{
		// somehow save this as a new prefab -> spawn in scene or save as prefab
	}
}