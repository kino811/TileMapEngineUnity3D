using UnityEngine;
using System.Collections;

namespace Kino.TileMap
{
    public class TileMapObject : MonoBehaviour, ITileMapObject
    {
        public Direction dir = Direction.Bottom;
        public TilePos tilePos;
        public TileMapSize tileMapSize = new TileMapSize(1, 1);
        public Vector2 tileSize = Vector2.one;

        private Transform rotateAnchor = null;

        #region Property

        public TileMapSize TileMapSize {
            get {return tileMapSize;}
        }

        public TilePos TilePos {
            get {return tilePos;}
            set {
                this.tilePos = value;
            }
        }

        #endregion

		public MapObjType Type {
			get { return MapObjType.Construction; }
		}

        public void Init(Direction dir, TilePos tilePos)
        {
            this.dir = dir;
            this.tilePos = tilePos;

            UpdateRotateAnchorPos();
            UpdateDirection();
        }

        public void UpdateRotateAnchorPos() {
            const string rotateAnchorName = "RotateAnchor";

            if (rotateAnchor == null) {
                rotateAnchor = transform.FindChild(rotateAnchorName);

                if (rotateAnchor == null) {
                    GameObject rotateAnchorObj = new GameObject(rotateAnchorName);
                    rotateAnchorObj.transform.rotation = Quaternion.identity;
                    rotateAnchorObj.transform.parent = transform;

                    rotateAnchor = rotateAnchorObj.transform;
                }
            }

            Debug.Assert(rotateAnchor);
            Debug.Assert(tileMapSize.width > 0 && tileMapSize.height > 0);
            Debug.Assert(tileSize.x > 0 && tileSize.y > 0);

            rotateAnchor.localPosition = new Vector3(
                tileSize.x * 0.5f * (tileMapSize.width - 1), 
                0.0f, 
                tileSize.y * 0.5f * (tileMapSize.height - 1));
        }

        public void UpdateDirection()
        {
            if (rotateAnchor == null) {
                rotateAnchor = transform.FindChild("RotateAnchor");
            }

            if (rotateAnchor == null) {
                Debug.LogError("null RotateAnchor");
            }

            switch (dir)
            {
            case Direction.Left:
                {
                    rotateAnchor.transform.rotation = Quaternion.AngleAxis(90.0f, Vector3.up);
                }
                break;
            case Direction.Top:
                {
                    rotateAnchor.transform.rotation = Quaternion.AngleAxis(180.0f, Vector3.up);
                }
                break;
            case Direction.Right:
                {
                    rotateAnchor.transform.rotation = Quaternion.AngleAxis(270.0f, Vector3.up);
                }
                break;
            case Direction.Bottom:
                {
                    rotateAnchor.transform.rotation = Quaternion.AngleAxis(0.0f, Vector3.up);
                }
                break;
            }
        }

        void Awake()
        {
        }
    }
}