using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Kino.TileMap
{
    public class TileMapObjectGroup : MonoBehaviour, ITileMapObject
    {
        public TileMapSize tileMapSize;
        public TilePos tilePos;

        private List<TileMapObject> tileMapObjects = new List<TileMapObject>();

		public MapObjType Type { get { return MapObjType.Construction; } }

        #region Property

        public TileMapSize TileMapSize {
            get {return tileMapSize;}
        }

        public TilePos TilePos {
            get {return tilePos;}
        }

        #endregion

        public void Init(TileMapSize tileMapSize, TilePos tilePos)
        {
            this.tileMapSize = tileMapSize;
            this.tilePos = tilePos;
        }

        void SubOnObjectToTileNodesByCurPos() {
            foreach (TileMapObject obj in tileMapObjects) {
                TilePos objTilePos = obj.TilePos + this.TilePos;
                TileMapSize objUnitTileMapSize = obj.TileMapSize;
                for (int x = 0; x < objUnitTileMapSize.width; ++ x) {
                    for (int y = 0; y < objUnitTileMapSize.height; ++ y) {
                        TilePos tilePos = objTilePos + new TilePos(x, y);

                        SquareTileMapNode node = TileMapEngine.Instance.GetTileNode(tilePos);
                        if (node)
                            node.SubOnObject(this);
                    }
                }
            }
        }

        public void AddOnObjectToTileNodesByCurPos() {
            foreach (TileMapObject obj in tileMapObjects) {
                TilePos objTilePos = obj.TilePos + this.TilePos;
                TileMapSize objUnitTileMapSize = obj.TileMapSize;
                for (int x = 0; x < objUnitTileMapSize.width; ++ x) {
                    for (int y = 0; y < objUnitTileMapSize.height; ++ y) {
                        TilePos tilePos = objTilePos + new TilePos(x, y);

                        SquareTileMapNode node = TileMapEngine.Instance.GetTileNode(tilePos);
                        if (node)
                            node.AddOnObject(this);
                    }
                }
            }
        }

        public void SetTilePos(SquareTileMapNode tileMapNode)
        {
            SubOnObjectToTileNodesByCurPos();

            this.tilePos = new TilePos(tileMapNode.TilePosX, tileMapNode.TilePosY);
            this.transform.position = tileMapNode.WorldPosition;

            AddOnObjectToTileNodesByCurPos();
        }

        public int[,] GetBlockTileData()
        {
            int[,] blockMapData = new int[TileMapSize.width, TileMapSize.height];

            TileMapObject[] objects = gameObject.GetComponentsInChildren<TileMapObject>();
            foreach (TileMapObject obj in objects)
            {
                blockMapData[obj.TilePos.x, obj.TilePos.y] = 1;

                for (int ix = 0; ix < obj.TileMapSize.width; ++ ix) {
                    for (int iy = 0; iy < obj.TileMapSize.height; ++ iy) {
                        TilePos tilePos = obj.TilePos;
                        tilePos.x += ix;
                        tilePos.y += iy;

                        if (tilePos.x < TileMapSize.width && tilePos.y < TileMapSize.height) {
                            blockMapData[tilePos.x, tilePos.y] = 1;
                        }
                    }
                }
            }

            return blockMapData;
        }

        void OnDestroy() {
            SubOnObjectToTileNodesByCurPos();
        }

        void Awake()
        {
            {                
                TileMapObject[] tileMapObjectsFromChild = GetComponentsInChildren<TileMapObject>();
                foreach (TileMapObject tileMapObj in tileMapObjectsFromChild)
                {
                    tileMapObjects.Add(tileMapObj);
                }
            }
        }
    }
}