﻿#if UNITY_EDITOR

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using Kino.TileMap;
using Kino.TileMap.Test;

namespace Kino.TileMap
{
    public class TileMapEngineEditor : EditorWindow 
    {
        public enum SelectMode {
            None,
            TileNode,
            TileObject,
        }

        private const int maxTileWidthCount = 1000;
        private const int maxTileHeightCount = 1000;
        private bool screen2D = false;
        private int tileWidthCount = 0;
        private int tileHeightCount = 0;
        private SquareTileMapNode tilePrefab;
        private SquareTileMapNode tilePrefabForChange;
        private List<SquareTileMapNode> selectedTileMapNodes = new List<SquareTileMapNode>();
        private SquareTileMapNode pathFindTestStartNode;
        private SquareTileMapNode pathFindTestGoalNode;
        private TileMapObject tileMapObjPrefabForCreation;
        private TileMapRoot curTileMap;
        private TileMapObjectGroup curTileMapObjectGroup;
        private Vector2 scrollPos;
        private GameObject positionedObjGroupGameObj;
        private SquareTileMapNode groundTileForChangeAtSelectedObjectBlockPos;
        private SelectMode selectMode = SelectMode.None;
        private Component lastSelectionWhenSelectMode;

        [MenuItem("Window/TileMapEngine/TileMapEngineEditor")]
        private static void OpenTileMapEngineEditor()
        {
            TileMapEngineEditor window = EditorWindow.GetWindow(typeof(TileMapEngineEditor), false, "TileMapEngine") as TileMapEngineEditor;
            window.Init();
        }

        [MenuItem("Window/TileMapEngine/SelectMode/None &q")]
        private static void SetSelectModeNone() {
            TileMapEngineEditor window = EditorWindow.GetWindow(typeof(TileMapEngineEditor), false, "TileMapEngine") as TileMapEngineEditor;
            window.selectMode = SelectMode.None;
        }

        [MenuItem("Window/TileMapEngine/SelectMode/TileNode &w")]
        private static void SetSelectModeTileNode() {
            TileMapEngineEditor window = EditorWindow.GetWindow(typeof(TileMapEngineEditor), false, "TileMapEngine") as TileMapEngineEditor;
            window.selectMode = SelectMode.TileNode;
        }

        [MenuItem("Window/TileMapEngine/SelectMode/TileObject &e")]
        private static void SetSelectModeTileObject() {
            TileMapEngineEditor window = EditorWindow.GetWindow(typeof(TileMapEngineEditor), false, "TileMapEngine") as TileMapEngineEditor;
            window.selectMode = SelectMode.TileObject;
        }

        public void Init()
        {
            TileMapEngine tileMapEngine = TileMapEngine.Instance;

            UpdateTileMapInfo();
        }

        void OnEnable() {
            //Debug.Log("OnEnable");
            SceneView.onSceneGUIDelegate += this.OnSceneGUI;
        }

        void OnDisable()
        {
            //Debug.Log("OnDisable");
            SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
        }

        void OnSelectOnScene<T>(SelectMode selectMode) where T : Component {
            Event e = Event.current;

            if (this.selectMode == selectMode && Tools.current != Tool.View) {
                if (e.type == EventType.MouseDown && e.button == 0) {
                    // prevent unity default control in scene-view
                    GUIUtility.hotControl = GUIUtility.GetControlID(FocusType.Passive);
                    e.Use();

                    T selected = null;

                    Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

                    ArrayList hits;
                    if (this.screen2D) {
                        hits = new ArrayList(Physics2D.RaycastAll(ray.origin, ray.direction));
                    }
                    else {
                        hits = new ArrayList(Physics.RaycastAll(ray));
                    }

                    foreach (var hit in hits) {
                        GameObject hitObj; 

                        if (screen2D) 
                            hitObj = ((RaycastHit2D)hit).collider.gameObject;
                        else
                            hitObj = ((RaycastHit)hit).collider.gameObject;
                        
                        selected = hitObj.GetComponent<T>();

                        if (selected == null) {
                            if (hitObj.transform.parent != null) 
                            {
                                //selected = hitObj.transform.parent.GetComponent<T>();
                                T[] tObjs = hitObj.GetComponentsInParent<T>();
                                if (tObjs != null && tObjs.Length > 0)
                                    selected = tObjs[0];
                            }
                        }

                        if (selected != null) {
                            if (e.control && !e.shift) {
                                // add to selection
                                Object[] selectionObjects = Selection.objects;
                                List<Object> newSelectionList = new List<Object>(selectionObjects);
                                newSelectionList.Add(selected.gameObject);
                                Selection.objects = newSelectionList.ToArray();
                            }
                            else if (e.control && e.shift) {
                                // add to multi selection
                                Object[] selectionObjects = Selection.objects;
                                // add before selections to new selections
                                List<Object> newSelectionList = new List<Object>(selectionObjects);

                                T beginCompo = this.lastSelectionWhenSelectMode as T;
                                if (beginCompo == null) {
                                    if (selectionObjects.Length > 0) {
                                        beginCompo = (selectionObjects[0] as GameObject).GetComponent<T>();
                                    }
                                }

                                if (beginCompo != null) {
                                    List<Object> additionSelectionList = GetObjectListForSelect(beginCompo, selected);
                                    newSelectionList.AddRange(additionSelectionList);

                                    Selection.objects = newSelectionList.ToArray();
                                }
                                else {
                                    Selection.activeGameObject = selected.gameObject;
                                }
                            }
                            else if (!e.control && e.shift) {
                                Object[] selectionObjects = Selection.objects;

                                if (selectionObjects.Length > 0) {
                                    T beginCompo = this.lastSelectionWhenSelectMode as T;

                                    if (beginCompo == null) {                                        
                                        beginCompo = (selectionObjects[0] as GameObject).GetComponent<T>();
                                    }

                                    List<Object> newSelectionList = GetObjectListForSelect(beginCompo, selected);
                                    Selection.objects = newSelectionList.ToArray();
                                }
                            }
                            else {
                                Selection.activeGameObject = selected.gameObject;
                            }

                            this.lastSelectionWhenSelectMode = selected;

                            break;
                        }
                        else {
                            this.lastSelectionWhenSelectMode = null;
                        }
                    }
                }
            }
        }

        void OnSceneGUI(SceneView sceneView)
        {
            switch (this.selectMode) {
                case SelectMode.TileNode: {
                    OnSelectOnScene<SquareTileMapNode>(SelectMode.TileNode);
                } break;
                case SelectMode.TileObject: {
                    OnSelectOnScene<TileMapObjectGroup>(SelectMode.TileObject);
                } break;
            }
        }

        List<Object> GetObjectListForSelect<T>(T beginObj, T destObj) where T : Component {
            if (typeof(T) == typeof(SquareTileMapNode)) {
                return GetNodeObjectList(beginObj as SquareTileMapNode, destObj as SquareTileMapNode);
            }

            return new List<Object>();
        }

        List<Object> GetNodeObjectList(SquareTileMapNode beginNode, SquareTileMapNode destNode)
        {            
            List<Object> newSelectionList = new List<Object>();
            SquareTileMapNode lastSelectedNodeCompo = beginNode;

            GameObject nodeGrp = lastSelectedNodeCompo.transform.parent.gameObject;

            const string nodeNameFormat = "tile_{0}-{1}";

            int loopYGrowUpValue = 0;
            if (lastSelectedNodeCompo.TilePosY < destNode.TilePosY) 
                loopYGrowUpValue = 1;
            else if (lastSelectedNodeCompo.TilePosY > destNode.TilePosY)
                loopYGrowUpValue = -1;

            int loopXGrowUpValue = 0;
            if (lastSelectedNodeCompo.TilePosX < destNode.TilePosX) 
                loopXGrowUpValue = 1;
            else if (lastSelectedNodeCompo.TilePosX > destNode.TilePosX)
                loopXGrowUpValue = -1;

            for (int y = lastSelectedNodeCompo.TilePosY; 
                loopYGrowUpValue > 0 ? 
                    y <= destNode.TilePosY : 
                    (loopYGrowUpValue < 0 ? 
                        y >= destNode.TilePosY : 
                        y == destNode.TilePosY);
                )
            {
                for (int x = lastSelectedNodeCompo.TilePosX; 
                    loopXGrowUpValue > 0 ? 
                        x <= destNode.TilePosX : 
                        (loopXGrowUpValue < 0 ? 
                            x >= destNode.TilePosX : 
                            x == destNode.TilePosX); 
                    )
                {
                    string nodeName = string.Format(nodeNameFormat, x, y);
                    Transform nodeTm = nodeGrp.transform.FindChild(nodeName);

                    newSelectionList.Add(nodeTm.gameObject);

                    if (loopXGrowUpValue == 0) 
                        ++ x;
                    else
                        x += loopXGrowUpValue;
                }

                if (loopYGrowUpValue == 0) 
                    ++ y;
                else 
                    y += loopYGrowUpValue;
            }

            return newSelectionList;
        }

        void OnFocus()
        {
            UpdateTileMapInfo();
        }

        void OnLostFocus()
        {
            UpdateTileMapInfo();
        }

        void UpdateTileMapInfo()
        {            
            this.tileWidthCount = 0;
            this.tileHeightCount = 0;

            if (curTileMap == null) {
                this.curTileMap = GameObject.FindObjectOfType<TileMapRoot>();
            }

            if (curTileMap) {
                this.tileWidthCount = curTileMap.TileWidthCount;
                this.tileHeightCount = curTileMap.TileHeightCount;
                this.screen2D = curTileMap.GetMapInfo().screen2D; 

                curTileMap.InitTileMapEngine();
                EditorUtility.SetDirty(TileMapEngine.Instance);
            }

            GUI.FocusControl("");
        }

        void OnGUI()
        {            
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            selectMode = (SelectMode)EditorGUILayout.EnumPopup("Select Mode: ", selectMode);

            // about create tile
            {
                TileMapRoot beforeTileMap = curTileMap;
                curTileMap = EditorGUILayout.ObjectField("curTileMap", curTileMap, typeof(TileMapRoot), true) as TileMapRoot;
                if (beforeTileMap != curTileMap) {
                    UpdateTileMapInfo();
                }

                if (curTileMap) {
                    if (GUILayout.Button("select", GUILayout.Width(100.0f))) {
                        Selection.activeObject = curTileMap.gameObject;
                    }
                }

                tilePrefab = EditorGUILayout.ObjectField("Tile Prefab", tilePrefab, typeof(SquareTileMapNode), false) as SquareTileMapNode;
                screen2D = EditorGUILayout.Toggle("screen2D", screen2D);

                tileWidthCount = EditorGUILayout.IntField("Tile Width Count: ", tileWidthCount);
                if (tileWidthCount < 0) {
                    tileWidthCount = 0;
                }
                if (tileWidthCount > maxTileWidthCount) {
                    tileWidthCount = maxTileWidthCount;
                }

                tileHeightCount = EditorGUILayout.IntField("Tile Height Count: ", tileHeightCount);
                if (tileHeightCount < 0) {
                    tileHeightCount = 0;
                }
                if (tileHeightCount > maxTileHeightCount) {
                    tileHeightCount = maxTileHeightCount;
                }

                if (tilePrefab && tileWidthCount > 0 && tileHeightCount > 0)
                {                 
                    GUILayout.BeginHorizontal();
                   
                    if (GUILayout.Button("Make New TileMap"))
                    {
                        MakeTileMap();
                    }

                    if (curTileMap) {
                        if (GUILayout.Button("Remake TileMap")) {
                            RemakeTileMap();
                        }

                        if (curTileMap.GetMapInfo().tileWidthCount <= tileWidthCount && 
                                curTileMap.GetMapInfo().tileHeightCount <= tileHeightCount) 
                        {
                            if (GUILayout.Button("Extend TileMap")) {
                                ExtendTileMap();
                            }
                        }
                    }

                    GUILayout.EndHorizontal();
                }
            }

            GUIDrawLine();

            // about tile selection
            {
                // collect selected tileMapNodes
                selectedTileMapNodes.Clear();
                GameObject[] selectedObjects = Selection.gameObjects;
                foreach (GameObject selectedGameObj in selectedObjects)
                {
                    SquareTileMapNode tileMapNode = selectedGameObj.GetComponent<SquareTileMapNode>();
                    if (tileMapNode && selectedGameObj.activeInHierarchy)
                    {
                        selectedTileMapNodes.Add(tileMapNode);
                    }
                }

                // show selected tileMapNodes count
                GUILayout.Label(string.Format("selectedTileMapNodes count: {0}", selectedTileMapNodes.Count));

                // about tile remake
                tilePrefabForChange = EditorGUILayout.ObjectField("Tile Prefab for change", tilePrefabForChange, typeof(SquareTileMapNode), true) as SquareTileMapNode;    
                if (tilePrefabForChange && selectedTileMapNodes.Count > 0)
                {
                    if (GUILayout.Button("Change selected tiles"))
                    {
                        foreach (SquareTileMapNode selectedMapTile in selectedTileMapNodes)
                        {
                            GameObject tileMapNodeGroup = selectedMapTile.transform.parent.gameObject;
                            SquareTileMapNode squareTileMapNode = MakeTileMapOneTile(
                                selectedMapTile.NodeID, 
                                selectedMapTile.TilePosX, 
                                selectedMapTile.TilePosY, 
                                tilePrefabForChange,
                                tileMapNodeGroup);
                        }
    
                        foreach (SquareTileMapNode selectedMapTile in selectedTileMapNodes)
                        {
                            DestroyImmediate(selectedMapTile.gameObject);
                        }

                        MakeConnectionEachOtherNodes();
                    }
                }

                GUIDrawLine();

                // about tile map object creation
                {
                    curTileMapObjectGroup = EditorGUILayout.ObjectField("TileMapObjectGroup ", curTileMapObjectGroup, typeof(TileMapObjectGroup), true) as TileMapObjectGroup;
                    if (curTileMapObjectGroup) {
                        if (IsPrefabTarget(curTileMapObjectGroup.gameObject)) {
                            if (selectedTileMapNodes.Count > 0 && curTileMap != null) {
                                if (GUILayout.Button("Create TileMapObjectGroup")) {
                                    if (positionedObjGroupGameObj == null) {
                                        const string positionGroupOfTileMapObjectGroup = "_PositionedObjectGroups";
                                        positionedObjGroupGameObj = GameObject.Find(positionGroupOfTileMapObjectGroup);
                                        if (positionedObjGroupGameObj == null) {
                                            positionedObjGroupGameObj = new GameObject(positionGroupOfTileMapObjectGroup);
                                        }
                                    }

                                    foreach (SquareTileMapNode tileMapNode in selectedTileMapNodes) {
                                        GameObject newTileMapObjGroup = PrefabUtility.InstantiatePrefab(curTileMapObjectGroup.gameObject) as GameObject;
                                        newTileMapObjGroup.transform.parent = positionedObjGroupGameObj.transform;

                                        TileMapObjectGroup newTileMapObjGroupCompo = newTileMapObjGroup.GetComponent<TileMapObjectGroup>();
                                        newTileMapObjGroupCompo.SetTilePos(tileMapNode);
                                    }
                                }
                            }
                        }
                        else {
                            groundTileForChangeAtSelectedObjectBlockPos = 
                                EditorGUILayout.ObjectField("GroundTileForChangeAtObjectBlockPositions", 
                                    groundTileForChangeAtSelectedObjectBlockPos, 
                                    typeof(SquareTileMapNode), 
                                    true) as SquareTileMapNode;

                            if (groundTileForChangeAtSelectedObjectBlockPos && curTileMap) 
                            {
                                if (GUILayout.Button("Change groundTile at object block pos")) {
                                    MakeConnectionEachOtherNodes();

                                    List<SquareTileMapNode> forChangeGroundTileList = new List<SquareTileMapNode>();
                                    int[,] blockTileData = curTileMapObjectGroup.GetBlockTileData();

                                    for (int x = 0; x < curTileMapObjectGroup.TileMapSize.width; ++ x) {
                                        for (int y = 0; y < curTileMapObjectGroup.TileMapSize.height; ++ y) {
                                            int blockFlag = blockTileData[x, y];

                                            if (blockFlag == 1) {
                                                TilePos tilePos = curTileMapObjectGroup.TilePos + new TilePos(x, y);
                                                SquareTileMapNode node = TileMapEngine.Instance.GetTileNode(tilePos);
                                                if (node == null) {
                                                    Debug.LogError(string.Format("null node : {0}_{1}", tilePos.x, tilePos.y));
                                                    continue;
                                                }

                                                forChangeGroundTileList.Add(node);
                                            }
                                        }
                                    }

                                    foreach (SquareTileMapNode node in forChangeGroundTileList) {
                                        GameObject tileMapNodeGroup = node.transform.parent.gameObject;

                                        SquareTileMapNode squareTileMapNode = MakeTileMapOneTile(
                                            node.NodeID,
                                            node.TilePosX,
                                            node.TilePosY,
                                            groundTileForChangeAtSelectedObjectBlockPos,
                                            tileMapNodeGroup);
                                    }

                                    foreach (SquareTileMapNode node in forChangeGroundTileList) {                                        
                                        DestroyImmediate(node.gameObject);
                                    }
                                }
                            }
                        }
                    }

                    tileMapObjPrefabForCreation = EditorGUILayout.ObjectField("TileMapObj Prefab for Creation", tileMapObjPrefabForCreation, typeof(TileMapObject), true) as TileMapObject;
                    if (tileMapObjPrefabForCreation && selectedTileMapNodes.Count > 0 && curTileMap != null)
                    {
                        if (GUILayout.Button("Create TileMapObjs to selected nodes pos"))
                        {                            
                            GameObject objectGroupGameObj;
                            if (curTileMapObjectGroup != null)
                                objectGroupGameObj = curTileMapObjectGroup.gameObject;

                            // get objectGroup gameobject
                            {
                                const string objectGroupGameObjectName = "_ObjectGroup";

                                objectGroupGameObj = GameObject.Find(objectGroupGameObjectName);
                                if (objectGroupGameObj == null) {
                                    objectGroupGameObj = new GameObject(objectGroupGameObjectName);
                                }

                                objectGroupGameObj.transform.position = Vector3.zero;

                                //BattleObject bo = objectGroupGameObj.GetComponent<BattleObject>();
                                //if (bo == null) {
                                    //bo = objectGroupGameObj.AddComponent<BattleObject>();
                                //}

                                TileMapObjectGroup objectGroup = objectGroupGameObj.GetComponent<TileMapObjectGroup>();
                                if (objectGroup == null) {
                                    objectGroup = objectGroupGameObj.AddComponent<TileMapObjectGroup>();
                                }
                                objectGroup.Init(new TileMapSize(curTileMap.TileWidthCount, curTileMap.TileHeightCount), new TilePos(0, 0));

                                curTileMapObjectGroup = objectGroup;
                            }

                            foreach (SquareTileMapNode tileMapNode in selectedTileMapNodes)
                            {
                                // create tile-map-object
                                GameObject newTileMapObj = PrefabUtility.InstantiatePrefab(tileMapObjPrefabForCreation.gameObject) as GameObject;
                                newTileMapObj.transform.position = tileMapNode.transform.position;

                                TileMapObject tileMapObjCompo = newTileMapObj.GetComponent<TileMapObject>();
                                tileMapObjCompo.Init(Kino.TileMap.Direction.Bottom, new TilePos(tileMapNode.TilePosX, tileMapNode.TilePosY));

                                newTileMapObj.transform.parent = objectGroupGameObj.transform;
                            }
                        }
                    }
                }
            }

            GUIDrawLine();

            // about tile map data export
            {
                if (curTileMap) {
                    GUILayout.Label(string.Format("tileMapID: {0}, tileMapName: {1}, width: {2}, height: {3}", 
                            curTileMap.MapID, curTileMap.MapName, curTileMap.TileWidthCount, curTileMap.TileHeightCount));

                    if (GUILayout.Button("select", GUILayout.Width(100.0f))) {
                        Selection.activeObject = curTileMap.gameObject;
                    }

                    if (GUILayout.Button("Export TileMapData")) {
                        ExportCurrentTileMapData();
                    }

                    if (GUILayout.Button("Export TileMapData for client")) {
                        ExportCurrentTileMapDataForClient();
                    }
                }
            }

            // about tile map object data export
            {
                if (curTileMapObjectGroup) {
                    //BattleObject bo = curTileMapObjectGroup.GetComponent<BattleObject>();

                    //if (bo) {
                        //GUILayout.Label(string.Format("objectID: {0}, objectName: {1}, width: {2}, height: {3}, hp: {4}, respawnTime: {5}", 
                            //bo.ObjectID, 
                            //bo.ObjectName, 
                            //curTileMapObjectGroup.TileMapSize.width, 
                            //curTileMapObjectGroup.TileMapSize.height,
                            //bo.HP,
                            //bo.RespawnTimeBySec));
                    //}

                    if (GUILayout.Button("select", GUILayout.Width(100.0f))) {
                        Selection.activeObject = curTileMapObjectGroup.gameObject;
                    }

                    if (GUILayout.Button("Export TileMapObjectGroupData")) {
                        ExportCurrentTileMapObjectGroupData();
                    }
                }
            }

            // about tile map object groups pos data export
            {
                positionedObjGroupGameObj = EditorGUILayout.ObjectField("positionedObjGroupGameObj for export", positionedObjGroupGameObj, typeof(GameObject), true) as GameObject;

                if (curTileMap && positionedObjGroupGameObj) {
                    if (GUILayout.Button("Export PositionInfos about positionedObjectGroups")) {
                        ExportPositionInfosAboutPositionedObjectGroupsDatas();
                    }
                }
            }

            GUIDrawLine();

            // PathFinder Test
            {
                GUILayout.Label("PathFind Test");

                if (GUILayout.Button("Init for Test"))
                {
                    MakeConnectionEachOtherNodes();
                    ClearAboutPathTest();
                }

                if (GUILayout.Button("clear")) {
                    ClearAboutPathTest();
                }

                if (selectedTileMapNodes.Count > 0) {
                    SquareTileMapNode targetNode = selectedTileMapNodes[0];

                    GUILayout.Label(string.Format("target nodeID:{0}", targetNode.nodeID));

                    if (GUILayout.Button("Set StartNode")) {
                        if (!targetNode.Invalid) {
                            if (pathFindTestStartNode) {
                                DestroyImmediate(pathFindTestStartNode.gameObject.GetComponent<StartNodeMarker>());
                                DestroyImmediate(pathFindTestStartNode.gameObject.GetComponent<GoalNodeMarker>());
                            }

                            pathFindTestStartNode = targetNode;
                            pathFindTestStartNode.gameObject.AddComponent<StartNodeMarker>();
                        }
                    }

                    if (GUILayout.Button("Set GoalNode")) {
                        if (!targetNode.Invalid) {
                            if (pathFindTestGoalNode) {
                                DestroyImmediate(pathFindTestGoalNode.gameObject.GetComponent<StartNodeMarker>());
                                DestroyImmediate(pathFindTestGoalNode.gameObject.GetComponent<GoalNodeMarker>());
                            }

                            pathFindTestGoalNode = targetNode;
                            pathFindTestGoalNode.gameObject.AddComponent<GoalNodeMarker>();
                        }
                    }
                }

                if (pathFindTestStartNode != null && pathFindTestGoalNode != null)
                {
                    if (GUILayout.Button("Find Path")) {
                        List<SquareTileMapNode> findedPath = TileMapEngine.Instance.Calculate(pathFindTestStartNode, pathFindTestGoalNode);

                        GameObject nodePathGameObj = GameObject.Find(NodePathMarker.GameObjName);
                        if (nodePathGameObj) {
                            DestroyImmediate(nodePathGameObj);
                        }

                        if (findedPath == null || findedPath.Count == 0) {
                            ShowNotification(new GUIContent("cannot find path"));
                        }
                        else {                            
                            nodePathGameObj = new GameObject(NodePathMarker.GameObjName);

                            NodePathMarker nodePathMarker = nodePathGameObj.AddComponent<NodePathMarker>();
                            nodePathMarker.findedPath = findedPath;
                        }
                    }
                }
            }

            GUIDrawLine();

            GUILayout.Label("tood:");

            EditorGUILayout.EndScrollView();

            if (Event.current.type == EventType.MouseMove)
                Repaint();
        }

        void GUIDrawLine()
        {
            EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);
        }

        void ExportCurrentTileMapObjectGroupData()
        {
            if (curTileMapObjectGroup) {
                const string exportFilePath = "tileMapObjectGroupExportedData.txt";
                TextWriter fd = new StreamWriter(exportFilePath);

                fd.WriteLine(string.Format("width : {0}", curTileMapObjectGroup.TileMapSize.width));
                fd.WriteLine(string.Format("height : {0}", curTileMapObjectGroup.TileMapSize.height));

                //BattleObject bo = curTileMapObjectGroup.GetComponent<BattleObject>();
                //if (bo) {
                    //fd.WriteLine(string.Format("objectID : {0}", bo.ObjectID));
                    //fd.WriteLine(string.Format("name : {0}", bo.ObjectName));
                    //fd.WriteLine(string.Format("hp : {0}", bo.MaxHP));
                    //fd.WriteLine(string.Format("respawnTime : {0}", bo.RespawnTimeBySec));
                    //fd.WriteLine(string.Format("classType : {0}({1})", (int)bo.ClassType, bo.ClassType));
                //}

                // make tile-postion data for server
                fd.WriteLine(string.Format("binaryData : "));
                int[,] tileBlockData = curTileMapObjectGroup.GetBlockTileData();
                for (int y = 0; y < tileBlockData.GetLength(1); ++ y)
                {
                    System.Text.StringBuilder rowBlockData = new System.Text.StringBuilder();

                    for (int x = 0; x < tileBlockData.GetLength(0); ++ x)
                    {
//                        if (x != 0)
//                            rowBlockData.Append(" ");
                            
                        rowBlockData.Append(tileBlockData[x,y]);
                    }

                    fd.WriteLine(rowBlockData.ToString());
                }

                fd.Close();

                Debug.Log(string.Format("Finish TileMapObjectGroupData Exported. fileName={0}", exportFilePath));
                ShowNotification(new GUIContent("Finish TileMapObjectGroupData Exported"));
            }
            else {
                Debug.LogWarning("curTileMapObjectGroup is null");
            }
        }

        void ExportPositionInfosAboutPositionedObjectGroupsDatas()
        {
            if (curTileMap && positionedObjGroupGameObj) {
                const string exportFilePath = "positionInfosAboutPositionedObjectGroupDatas.txt";
                TextWriter fd = new StreamWriter(exportFilePath);

                TileMapObjectGroup[] objGroups = positionedObjGroupGameObj.GetComponentsInChildren<TileMapObjectGroup>();
                for (int i = 0; i < objGroups.Length; ++ i) {
                    TileMapObjectGroup objGroup = objGroups[i];
                    //BattleObject bo = objGroup.GetComponent<BattleObject>();

                    if (i != 0) {
                        fd.WriteLine("");
                    }

                    // one object group pos info
                    fd.WriteLine(string.Format("mapID : {0}", curTileMap.MapID));
                    //fd.WriteLine(string.Format("objectID : {0}", bo.ObjectID));
                    fd.WriteLine(string.Format("pos : {0}, {1}", objGroup.TilePos.x, objGroup.TilePos.y));
                }

                fd.Close();

                Debug.Log(string.Format("Finish positionInfosAboutPositionedObjectGroupDatas Exported. fileName={0}", exportFilePath));
                ShowNotification(new GUIContent("Finish positionInfosAboutPositionedObjectGroupDatas Exported"));
            }
        }

        void ExportCurrentTileMapData()
        {
            if (curTileMap)
            {
                const string exportFilePath = "tileMapExportedData.txt";
                TextWriter fd = new StreamWriter(exportFilePath);

                fd.WriteLine(string.Format("mapID : {0}", curTileMap.MapID));
                fd.WriteLine(string.Format("mapName : {0}", curTileMap.MapName));
                fd.WriteLine(string.Format("Width : {0}", curTileMap.TileWidthCount));
                fd.WriteLine(string.Format("Height : {0}", curTileMap.TileHeightCount));

                // make map block data for server.
                fd.WriteLine(string.Format("mapBlockBinaryData :"));
                int[,] tileMapBlockData = curTileMap.GetBlockMapData();
                for (int y = 0; y < tileMapBlockData.GetLength(1); ++ y)
                {
                    System.Text.StringBuilder rowBlockData = new System.Text.StringBuilder();

                    for (int x = 0; x < tileMapBlockData.GetLength(0); ++ x)
                    {
//                        if (x != 0)
//                            rowBlockData.Append(" ");
                            
                        rowBlockData.Append(tileMapBlockData[x,y]);
                    }

                    fd.WriteLine(rowBlockData.ToString());
                }

                fd.Close();

                Debug.Log(string.Format("Finish TileMapData Exported. fileName={0}", exportFilePath));
                ShowNotification(new GUIContent("Finish TileMapData Exported"));
            }
            else {
                Debug.LogWarning("not exist current tile map");
            }
        }

        void ExportCurrentTileMapDataForClient()
        {
            if (curTileMap)
            {
                const string exportFilePath = "tileMapExportedDataForClient.txt";
                TextWriter fd = new StreamWriter(exportFilePath);

                // make map block data for server.
                fd.WriteLine(JsonUtility.ToJson(curTileMap.GetMapInfo()));

                // mapTileIDs map
                int[,] tileMapIDDatas = curTileMap.GetMapIDDatas();
                for (int y = 0; y < tileMapIDDatas.GetLength(1); ++ y)
                {
                    System.Text.StringBuilder rowData = new System.Text.StringBuilder();

                    for (int x = 0; x < tileMapIDDatas.GetLength(0); ++ x)
                    {
                        if (x != 0)
                            rowData.Append(" ");
                            
                        rowData.Append(tileMapIDDatas[x,y]);
                    }

                    fd.WriteLine(rowData.ToString());
                }

                fd.Close();

                Debug.Log(string.Format("Finish TileMapData Exported. fileName={0}", exportFilePath));
                ShowNotification(new GUIContent("Finish TileMapData for client Exported"));
            }
            else {
                Debug.LogWarning("not exist current tile map");
            }
        }

        void ClearAboutPathTest()
        {
            {
                StartNodeMarker[] markers = GameObject.FindObjectsOfType<StartNodeMarker>();
                foreach (StartNodeMarker marker in markers)
                {
                    DestroyImmediate(marker);
                }
            }

            {
                GoalNodeMarker[] markers = GameObject.FindObjectsOfType<GoalNodeMarker>();
                foreach (GoalNodeMarker marker in markers)
                {
                    DestroyImmediate(marker);
                }
            }

            {
                GameObject pathMarker = GameObject.Find(NodePathMarker.GameObjName);
                if (pathMarker)
                {
                    DestroyImmediate(pathMarker);
                }
            }

            pathFindTestStartNode = null;
            pathFindTestGoalNode = null;
            selectedTileMapNodes.Clear();
        }

        void MakeTileMap()
        {
            if (tilePrefab == null) {
                Debug.LogError("failed to MakeTileMap. null tilePrefab.");
                return;
            }

            // clear
            curTileMap = null;
            ClearAboutPathTest();

            // new root gameobject
            const string tileMapRootGameObjectName = "_TileMapRoot";
            GameObject tileMapRootGameObj = new GameObject(tileMapRootGameObjectName);
            tileMapRootGameObj.transform.position = Vector3.zero;
            TileMapRoot tileMapRootCompo = tileMapRootGameObj.AddComponent<TileMapRoot>() as TileMapRoot;

            Vector2 tileSize = Vector2.one;
            if (tilePrefab) {
                SquareTileMapNode nodeCompo = tilePrefab.GetComponent<SquareTileMapNode>();
                tileSize = nodeCompo.squareSize;
            }

            tileMapRootCompo.InitFromEditor(screen2D, tileWidthCount, tileHeightCount, tileSize);

            curTileMap = tileMapRootCompo;
            Selection.activeObject = curTileMap.gameObject;

            // new tilenode group gameobject
            GameObject tileMapNodeGroup = tileMapRootCompo.NodeGroup;

            // create tile-map-nodes
            Dictionary<int, SquareTileMapNode> nodeMap = new Dictionary<int, SquareTileMapNode>();
            int nodeIndex = 0;

            for (int y = 0; y < tileHeightCount; ++ y)
            {
                for (int x = 0; x < tileWidthCount; ++ x, ++ nodeIndex)
                {
                    SquareTileMapNode squareTileMapNode = MakeTileMapOneTile(nodeIndex, x, y, tilePrefab, tileMapNodeGroup);

                    nodeMap[nodeIndex] = squareTileMapNode;
                }
            }

            // connect eachother nodes
            MakeConnectionEachOtherNodes();
        }

        void ExtendTileMap() {
            if (curTileMap == null)
                return;

            if (tilePrefab == null) {
                Debug.LogError("failed to ExtendTileMap. null tilePrefab.");
                return;
            }

            // clear
            ClearAboutPathTest();

            curTileMap.InitTileMapEngine();

            // new tilenode group gameobject
            GameObject tileMapNodeGroup = curTileMap.NodeGroup;

            // create tile-map-nodes
            int nodeIndex = 0;

            for (int y = 0; y < tileHeightCount; ++ y)
            {
                for (int x = 0; x < tileWidthCount; ++ x, ++ nodeIndex)
                {
                    SquareTileMapNode node = TileMapEngine.Instance.GetTileNode(new TilePos(x, y));
                    if (node == null)
                        MakeTileMapOneTile(nodeIndex, x, y, tilePrefab, tileMapNodeGroup);
                    else {
                        node.Init(nodeIndex, x, y);
                        EditorUtility.SetDirty(node);
                    }
                }
            }

            curTileMap.ExtendFromEditor(tileWidthCount, tileHeightCount);
            EditorUtility.SetDirty(curTileMap);

            // connect eachother nodes
            MakeConnectionEachOtherNodes();
        }

        void RemakeTileMap()
        {
            if (curTileMap == null)
                return;

            if (tilePrefab == null) {
                Debug.LogError("failed to MakeTileMap. null tilePrefab.");
                return;
            }

            // clear
            ClearAboutPathTest();

            Vector2 tileSize = Vector2.one;
            if (tilePrefab) {
                SquareTileMapNode nodeCompo = tilePrefab.GetComponent<SquareTileMapNode>();
                tileSize = nodeCompo.squareSize;
            }

            curTileMap.InitFromEditor(screen2D, tileWidthCount, tileHeightCount, tileSize);

            // new tilenode group gameobject
            GameObject tileMapNodeGroup = curTileMap.NodeGroup;

            // create tile-map-nodes
            Dictionary<int, SquareTileMapNode> nodeMap = new Dictionary<int, SquareTileMapNode>();
            int nodeIndex = 0;

            for (int y = 0; y < tileHeightCount; ++ y)
            {
                for (int x = 0; x < tileWidthCount; ++ x, ++ nodeIndex)
                {
                    SquareTileMapNode squareTileMapNode = MakeTileMapOneTile(nodeIndex, x, y, tilePrefab, tileMapNodeGroup);

                    nodeMap[nodeIndex] = squareTileMapNode;
                }
            }

            // connect eachother nodes
            MakeConnectionEachOtherNodes();
        }

        SquareTileMapNode MakeTileMapOneTile(int nodeIndex, int x, int y, SquareTileMapNode tilePrefab, GameObject tileMapNodeGroup)
        {
            // create tile-map-node-gameobject
            GameObject newTileMapNodeGameObj = PrefabUtility.InstantiatePrefab(tilePrefab.gameObject) as GameObject;
            newTileMapNodeGameObj.transform.parent = tileMapNodeGroup.transform;
            newTileMapNodeGameObj.name = string.Format("tile_{0}-{1}", x, y);

            SquareTileMapNode squareTileMapNode = newTileMapNodeGameObj.GetComponent<SquareTileMapNode>();
            squareTileMapNode.Init(nodeIndex, x, y);

            Vector3 worldPos = Vector3.zero;
            {                    
                worldPos.x = x * squareTileMapNode.squareSize.x;

                if (screen2D)
                    worldPos.y = y * squareTileMapNode.squareSize.y;
                else
                    worldPos.z = y * squareTileMapNode.squareSize.y;
            }
            newTileMapNodeGameObj.transform.position = worldPos;

            if (screen2D && newTileMapNodeGameObj.GetComponentInChildren<MeshRenderer>() != null) {
                newTileMapNodeGameObj.transform.rotation = Quaternion.AngleAxis(-90.0f, Vector3.right);
            }

            return squareTileMapNode;
        }

        void MakeConnectionEachOtherNodes()
        {
            if (curTileMap)
            {
                curTileMap.InitTileMapEngine();
            }
        }

        bool IsPrefabTarget(GameObject obj) {
            return PrefabUtility.GetPrefabParent(obj) == null && PrefabUtility.GetPrefabObject(obj) != null;
        }
    }
}	

#endif
