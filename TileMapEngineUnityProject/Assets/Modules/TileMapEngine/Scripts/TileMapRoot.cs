using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Kino.TileMap
{
    public class TileMapRoot : MonoBehaviour {
        [System.Serializable]
        public class MapInfo {
            public int mapID;
            public string mapName;
            public int tileWidthCount;
            public int tileHeightCount;
            public bool screen2D;
            public Vector2 tileSize;
        }

        #region Fields

        public static string nodeGroupGameObjName = "_TileMapNodeGroup";

        public MapInfo mapInfo = new MapInfo();

        private GameObject nodeGroup;

        #endregion

        #region Properties

        public Vector2 MapTileSzie {
            get {return mapInfo.tileSize;}
        }

        public int MapID {
            get {return mapInfo.mapID;}
        }

        public string MapName {
            get {return mapInfo.mapName;}
        }

        public int TileWidthCount {
            get {return mapInfo.tileWidthCount;}
        }

        public int TileHeightCount {
            get {return mapInfo.tileHeightCount;}
        }

        public GameObject NodeGroup {
            get {
                if (nodeGroup == null) {
                    Transform tm = transform.Find(nodeGroupGameObjName);
                    if (tm)
                        nodeGroup = tm.gameObject;
                }

                return nodeGroup;
            }
        }

        #endregion

        #region Methods

        public MapInfo GetMapInfo() {
            return mapInfo;
        }

        public void InitFromEditor(bool screen2D, int tileWidthCount, int tileHeightCount, Vector2 tileSize)
        {
            this.mapInfo.screen2D = screen2D;
            this.mapInfo.tileWidthCount = tileWidthCount;
            this.mapInfo.tileHeightCount = tileHeightCount;
            this.mapInfo.tileSize = tileSize;

            Init();
        }

        public void ExtendFromEditor(int tileWidthCount, int tileHeightCount) {
            if (this.mapInfo.tileWidthCount < tileWidthCount)
                this.mapInfo.tileWidthCount = tileWidthCount;

            if (this.mapInfo.tileHeightCount < tileHeightCount)
                this.mapInfo.tileHeightCount = tileHeightCount;
        }

        public void Init() {
            if (NodeGroup) {
                DestroyImmediate(nodeGroup);
            }

            if (nodeGroup == null) {
                nodeGroup = new GameObject(TileMapRoot.nodeGroupGameObjName);
                nodeGroup.transform.parent = transform;
                nodeGroup.transform.position = Vector3.zero;
            }
        }

        public int[,] GetBlockMapData()
        {
            int[,] blockMapData = new int[TileWidthCount, TileHeightCount];

            Transform nodeGroupGameObjTM = transform.FindChild(nodeGroupGameObjName);
            if (nodeGroupGameObjTM)
            {
                SquareTileMapNode[] nodes = nodeGroupGameObjTM.gameObject.GetComponentsInChildren<SquareTileMapNode>();
                foreach (SquareTileMapNode node in nodes)
                {
                    blockMapData[node.TilePosX, node.TilePosY] = node.ImpossibleObjectOn ? 1 : 0;
                }
            }

            return blockMapData;
        }

        public int[,] GetMapIDDatas()
        {
            int[,] mapIDData = new int[TileWidthCount, TileHeightCount];

            GameObject nodeGroupGameObj = this.NodeGroup;
            if (nodeGroupGameObj)
            {                
                List<SquareTileMapNode> nodeList = new List<SquareTileMapNode>(nodeGroupGameObj.GetComponentsInChildren<SquareTileMapNode>());
                nodeList.Sort(delegate(SquareTileMapNode lhs, SquareTileMapNode rhs) {
                    if (lhs.NodeID < rhs.NodeID) return -1;
                    else if (lhs.NodeID == rhs.NodeID) return 0;

                    return 1;
                });

                foreach (SquareTileMapNode node in nodeList)
                {
                    mapIDData[node.TilePosX, node.TilePosY] = node.TileID;
                }
            }

            return mapIDData;
        }

        public void InitTileMapEngine()
        {
            TileMapEngine.Instance.Init(mapInfo.screen2D, mapInfo.tileWidthCount, mapInfo.tileHeightCount, gameObject.GetComponentsInChildren<SquareTileMapNode>());
        }

        void Awake()
        {
            Transform nodeGroupTM = transform.Find(nodeGroupGameObjName);
            if (nodeGroupTM == null) {
                GameObject nodeGroupGameObject = new GameObject(nodeGroupGameObjName);
                nodeGroupGameObject.transform.parent = transform;
                nodeGroupGameObject.transform.position = Vector3.zero;

                nodeGroupTM = nodeGroupGameObject.transform;
            }

            nodeGroup = nodeGroupTM.gameObject;
        }

        #endregion
    }
}