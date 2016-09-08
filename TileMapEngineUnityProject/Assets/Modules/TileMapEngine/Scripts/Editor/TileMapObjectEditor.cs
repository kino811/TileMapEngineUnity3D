#if UNITY_EDITOR

using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Kino.TileMap
{
    [CustomEditor(typeof(TileMapObject))]
    public class TileMapObjectEditor : Editor 
    {
        private TileMapObject tileMapObj;

    	public override void OnInspectorGUI()
        {    
            DrawDefaultInspector();

            if (GUI.changed)
            {                
                TileMapObject tileMapObj = target as TileMapObject;

                if (!IsPrefabTarget()) {
                    tileMapObj.UpdateRotateAnchorPos();
                    tileMapObj.UpdateDirection();
                }
            }
        }

        void OnEnable() {
            this.tileMapObj = target as TileMapObject;

            if (!IsPrefabTarget() && tileMapObj != null) {
                tileMapObj.UpdateRotateAnchorPos();
            }
        }

        bool IsPrefabTarget() {
            return PrefabUtility.GetPrefabParent(tileMapObj) == null && PrefabUtility.GetPrefabObject(tileMapObj) != null;
        }
    }
}

#endif