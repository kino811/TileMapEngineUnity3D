# TileMapEngineUnity3D

## 무슨 프로그램?
유니티에서 타일 맵 기반 게임을 만들때 사용할수 있다. 유니티 에디터 기능으로 만든 에디터도 같이 제공됨으로 맵을 만들고, 오브젝트를 배치 할수 있음.

## 설명

#### 타일맵 엔진
TileMapEngine.cs 에 구현 되있다. 타일들로 만들어진 맵을 로드하는 기능이 있다. 로드된 맵에서 길찾기 기능을 제공한다. 길찾기 기능은 unity wiki 의 A-Start 알고리즘 코드를 가져다가 조금 수정한 정도다. 길찾기시 오브젝트 고려 할지, 목표 지점의 오브젝트 체크 여부 정도 기능 추가. [Unity Wiki AStart](http://wiki.unity3d.com/index.php/AStarHelper)

```cs
List<SquareTileMapNode> pathNodes = TileMapEngine.Instance.Calculate(tileNode, mapTile, checkObjWhenPathFind, goalCheckObjWhenPathFind);

if (pathNodes != null && pathNodes.Count > 0)
    player.AutoMove(ref pathNodes);
```

#### 타일맵 에디터

![TileMap Editor][tileMapEditor]

[tileMapEditor]: Images/TileMapEditor.png "TileMap Editor"
