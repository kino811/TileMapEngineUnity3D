using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Kino.TileMap 
{
    public class SquareTileMapNode : MonoBehaviour, ITileMapNode 
    {    
        #region Fields

        public int tileID;
        public Vector2 squareSize;
        public int nodeID;
        public int tilePosX;
        public int tilePosY;
        public bool impossibleObjectOn = false;

        private List<ITileMapNode> connections = new List<ITileMapNode>();
        private HashSet<ITileMapObject> objectsOnNodeSet = new HashSet<ITileMapObject>();
        private TileMapRoot tileMap;

        #endregion

        #region Properties

        public HashSet<ITileMapObject> ObjectsOnNodeSet {
            get {return objectsOnNodeSet;}
        }

        public int TileID {
            get {return tileID;}
        }

        public TilePos TilePos {
            get {
                return new TilePos(tilePosX, tilePosY);
            }
        }

        public Vector2 SquareSize {
            get {return squareSize;}
        }

        public bool ImpossibleObjectOn {
            get {return impossibleObjectOn;}
        }

        public List<ITileMapNode> Connections {
            get {return connections;}
        }

        public Vector3 WorldPosition {
            get {return transform.position;}
        }

        public bool Invalid {
            get {
                if (this == null)
                    return true;

                if (!gameObject.activeInHierarchy)
                    return true;

                if (impossibleObjectOn)
                    return true;

//                if (objectsOnNodeSet.Count > 0)
//                    return true;

                return false;
            }
        }

        public int NodeID {
            get {return nodeID;}
        }

        public int TilePosX {
            get {return tilePosX;}
            set {
                tilePosX = value < 0 ? 0 : value;
            }
        }

        public int TilePosY {
            get {return tilePosY;}
            set {
                tilePosY = value < 0 ? 0 : value;
            }
        }

        public void AddOnObject(ITileMapObject obj) {
            objectsOnNodeSet.Add(obj);
        }

        public void SubOnObject(ITileMapObject obj) {
            objectsOnNodeSet.Remove(obj);
        }

        #endregion

        public void Init(int nodeID, int tilePosX, int tilePosY) {
            this.nodeID = nodeID;
            this.TilePosX = tilePosX;
            this.TilePosY = tilePosY;
        }

        public bool HasTileObj() {
            return objectsOnNodeSet.Count > 0;
        }

        public void UpdatePos() {
            transform.position = new Vector3(TilePosX * squareSize.x, 0f, TilePosY * squareSize.y);
        }

        public void ClearConnectionNode()
        {
            connections.Clear();
        }

        public void AddConnectionNode(SquareTileMapNode node)
        {
            connections.Add(node);
        }

        void Awake() {
            gameObject.layer = LayerMask.NameToLayer("MapTile");

            BoxCollider[] colliders = transform.GetComponentsInChildren<BoxCollider>();
            foreach (BoxCollider coll in colliders) {
                coll.gameObject.layer = LayerMask.NameToLayer("MapTile");
            }

            objectsOnNodeSet.Clear();
        }

        void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying)
            {
                if (tileMap == null)
                    SetTileMap();

                bool screen2D = false;
                if (tileMap)
                    screen2D = tileMap.GetMapInfo().screen2D;

                Gizmos.color = Color.blue;

                Vector3 size;
                if (screen2D)
                    size = new Vector3(squareSize.x * 0.9f, squareSize.y * 0.9f, 0.0f);
                else
                    size = new Vector3(squareSize.x * 0.9f, 0.0f, squareSize.y * 0.9f);

                Gizmos.DrawWireCube(WorldPosition, size);

                foreach (ITileMapNode connectedNode in connections)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireCube(
                        WorldPosition + (connectedNode.WorldPosition - WorldPosition) * 0.5f,
                        size * 0.5f
                    );
                }
            }
        }

        void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                if (tileMap == null)
                    SetTileMap();

                bool screen2D = false;
                if (tileMap)
                    screen2D = tileMap.GetMapInfo().screen2D;

                Gizmos.color = Color.red;

                Vector3 size;
                if (screen2D)
                    size = new Vector3(squareSize.x, squareSize.y, 0.0f);
                else
                    size = new Vector3(squareSize.x, 0.0f, squareSize.y);

                Gizmos.DrawWireCube(WorldPosition, size);
            }
        }

        void SetTileMap() {
            if (transform.parent && transform.parent.transform.parent)
                tileMap = transform.parent.transform.parent.GetComponent<TileMapRoot>();
        }
    }
}