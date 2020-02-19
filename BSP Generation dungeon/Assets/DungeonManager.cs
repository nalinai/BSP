using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonManager : MonoBehaviour {
    //Created by : Samuel 14 April 2019
    //Purpose : divide the space into smaller room and connect it to become a dungeon
    public int dungeonRows, dungeonColumns, minRoomSize, maxRoomSize;

    public GameObject floorTile;
    public GameObject corridorTile;

    private GameObject[,] boardPositionFloor;

    public void DrawRooms(SubDungeon subDungeon)
    {
        if (subDungeon == null) return;

        else if(subDungeon.IsLeaf())
        {
            for(int i = (int)subDungeon.room.x;  i < subDungeon.room.xMax;i++)
            {
                for (int j = (int)subDungeon.room.y; j < subDungeon.room.yMax; j++)
                {
                    GameObject instance = Instantiate(floorTile, new Vector3(i, j, 0f), Quaternion.identity);
                }
            }
        }
        else
        {
            DrawRooms(subDungeon.left);
            DrawRooms(subDungeon.right);
        }
    }

    void DrawCorridor(SubDungeon subDungeon)
    {
        if (subDungeon == null)
        {
            return;
        }

        DrawCorridor(subDungeon.left);
        DrawCorridor(subDungeon.right);

        foreach (Rect corridor in subDungeon.corridors)
        {
            for (int i = (int)corridor.x; i < corridor.xMax; i++)
            {
                for (int j = (int)corridor.y; j < corridor.yMax; j++)
                {
                    if (boardPositionFloor[i, j] == null)
                    {
                        GameObject instance = Instantiate(corridorTile, new Vector3(i, j, 0f), Quaternion.identity) as GameObject;
                        instance.transform.SetParent(transform);
                        boardPositionFloor[i, j] = instance;
                    }
                }
            }
        }
    }

    public class SubDungeon
    {
        public SubDungeon left, right;

        public Rect rect;
        public Rect room = new Rect(-1, -1, 0, 0);
        public List<Rect> corridors = new List<Rect>();

        public int debugId;

        private static int  debugCounter = 0;

        public Rect GetRoom()
        {
            if(IsLeaf())
            {
                return room;
            }

            if (left != null)
            {
                Rect lRoom = left.GetRoom();
                if(lRoom.x != -1)
                {
                    return lRoom;
                }
            }

            if (right != null)
            {
                Rect rRoom = right.GetRoom();
                if (rRoom.x != -1)
                {
                    return rRoom;
                }
            }

            //this mean null
            return new Rect(-1, -1, 0, 0);
        }

        public void CreateRoom()
        {
            if(left != null)
            {
                left.CreateRoom();
            }

            if (right != null)
            {
                right.CreateRoom();
            }

            if(left != null && right != null)
            {
                CreateCorridor(left, right);
            }

            if(IsLeaf())
            {
                int roomWidth = (int)Random.Range(rect.width / 2, rect.width - 2);
                int roomHeight = (int)Random.Range(rect.height / 2, rect.height - 2);
                int roomX = (int)Random.Range(1, rect.width - roomWidth - 1);
                int roomY = (int)Random.Range(1, rect.height - roomHeight - 1);

                room = new Rect(rect.x + room.x, rect.y + room.y, roomWidth, roomHeight);
                Debug.Log("Created room " + room + " in sub-dungeon " + debugId + " " + rect);
            }

        }

        public SubDungeon(Rect r)
        {
            rect = r;
            debugId = debugCounter;
            debugCounter++;
        }

        public bool IsLeaf()
        {
            return left == null && right == null;
        }

        public bool Split(int minRoomSize , int maxRoomSize)
        {
            if(!IsLeaf())
            {
                return false;
            }

            bool checker;

            // choose a vertical or horizontal split depending on the proportions
            // if it's too wide split vertically, or too long horizontally 
            // or choose vertical or horizontal at random

            if (rect.width / rect.height >=1.25)
            {
                checker = false;
            }
            else if(rect.height / rect.width >=1.25 )
            {
                checker = true;
            }
            else
            {
                checker = Random.Range(0.0f, 1.0f) > 0.5f;
            }

            if(Mathf.Min(rect.height , rect.width) / 2 < minRoomSize )
            {
                Debug.Log("Sub-dungeon " + debugId + " will be a leaf");
                return false;
            }

            if(checker)
            {
                int split = Random.Range(minRoomSize, (int)(rect.width - minRoomSize));

                left = new SubDungeon(new Rect(rect.x, rect.y, rect.width, split));

                right = new SubDungeon(new Rect(rect.x, rect.y + split, rect.width, rect.height - split));
            }

            else
            {
                int split = Random.Range(minRoomSize, (int)(rect.height - minRoomSize));

                left = new SubDungeon(new Rect(rect.x, rect.y, split, rect.height));

                right = new SubDungeon(new Rect(rect.x + split, rect.y, rect.width - split, rect.height));
            }

            return true;
        }

        public void CreateCorridor(SubDungeon left, SubDungeon right)
        {
            Rect lRoom = left.GetRoom();
            Rect rRoom = right.GetRoom();

            Debug.Log("Creating corridor(s) between " + left.debugId + "(" + lRoom + ") and " + right.debugId + " (" + rRoom + ")");

            Vector2 lPoint = new Vector2((int)Random.Range(lRoom.x + 1, lRoom.xMax - 1), (int)Random.Range(lRoom.y + 1, lRoom.yMax - 1));
            Vector2 rPoint = new Vector2((int)Random.Range(rRoom.x + 1, rRoom.xMax - 1), (int)Random.Range(rRoom.y + 1, rRoom.yMax - 1));

            //to swap the left point if the left point is't the left one
            if (lPoint.x > rPoint.x)
            {
                Vector2 temp = lPoint;
                lPoint = rPoint;
                rPoint = lPoint;
            }

            int w = (int)(lPoint.x - rPoint.x);
            int h = (int)(lPoint.y - rPoint.y);

            Debug.Log("lpoint: " + lPoint + ", rpoint: " + rPoint + ", w: " + w + ", h: " + h);

            // if the points are not aligned horizontally
            if (w != 0)
            {
                // choose at random to go horizontal then vertical or the opposite
                if (Random.Range(0, 1) > 2)
                {
                    // add a corridor to the right
                    corridors.Add(new Rect(lPoint.x, lPoint.y, Mathf.Abs(w) + 1, 1));

                    // if left point is below right point go up
                    // otherwise go down
                    if (h < 0)
                    {
                        corridors.Add(new Rect(rPoint.x, lPoint.y, 1, Mathf.Abs(h)));
                    }
                    else
                    {
                        corridors.Add(new Rect(rPoint.x, lPoint.y, 1, -Mathf.Abs(h)));
                    }
                }
                else
                {
                    // go up or down
                    if (h < 0)
                    {
                        corridors.Add(new Rect(lPoint.x, lPoint.y, 1, Mathf.Abs(h)));
                    }
                    else
                    {
                        corridors.Add(new Rect(lPoint.x, rPoint.y, 1, Mathf.Abs(h)));
                    }

                    // then go right
                    corridors.Add(new Rect(lPoint.x, rPoint.y, Mathf.Abs(w) + 1, 1));
                }
            }
            else
            {
                // if the points are aligned horizontally
                // go up or down depending on the positions
                if (h < 0)
                {
                    corridors.Add(new Rect((int)lPoint.x, (int)lPoint.y, 1, Mathf.Abs(h)));
                }
                else
                {
                    corridors.Add(new Rect((int)rPoint.x, (int)rPoint.y, 1, Mathf.Abs(h)));
                }
            }

            Debug.Log("Corridors: ");
            foreach (Rect corridor in corridors)
            {
                Debug.Log("corridor: " + corridor);
            }

        }
    }


    public void CreateBSP(SubDungeon subDungeon)
    {
        Debug.Log("Splitting sub-dungeon " + subDungeon.debugId + ": " + subDungeon.rect);
        if (subDungeon.IsLeaf())
        {
            if(subDungeon.rect.width > maxRoomSize || subDungeon.rect.height > maxRoomSize)
            {
                if(subDungeon.Split(minRoomSize , maxRoomSize))
                {
                    Debug.Log("Splitted sub-dungeon " + subDungeon.debugId + " in "
                        + subDungeon.left.debugId + ": " + subDungeon.left.rect + ", "
                        + subDungeon.right.debugId + ": " + subDungeon.right.rect);

                    CreateBSP(subDungeon.left);
                    CreateBSP(subDungeon.right);
                }
            }
        }
    }


    // Use this for initialization
    void Start ()
    {
        SubDungeon root = new SubDungeon(new Rect(0, 0, dungeonRows, dungeonColumns));
        CreateBSP(root);
        root.CreateRoom();

        boardPositionFloor = new GameObject[dungeonRows, dungeonColumns];
        DrawCorridor(root);
        DrawRooms(root);
    }
	
}
