using SLywnow;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SLM_OW_Main : MonoBehaviour
{
    [Header("Scripts")]
    SLM_AddonManager AM;
    public SLM_Commands C;
    SLM_NovelImages I;
    public string layerName;

    [Header("SetUps")]
    public List<Sprite> sprites;
    public SLM_OW_Move moves;
    public SLM_OW_Interactions interactions;
    public bool useMinimap;
    public SLM_OW_Minimap minimap;

    [HideInInspector] public SLM_OW_RoomsList rooms;

    [Header("Files")]
    public bool useroomFile;
    public int defFile = 0;
    public List<Object> roomsFile;

    Vector2Int curpos;
    int curroomsetupid;
    bool hasiteration;

    void Awake()
    {
        if (C != null)
        {
            AM = C.addonManager;
            I = C.imagesc;
            curpos = new Vector2Int(-1, -1);
            curroomsetupid = -1;

            if (useroomFile)
                rooms = JsonUtility.FromJson<SLM_OW_RoomsList>(((roomsFile[defFile] as TextAsset).text));
            moves.mainObj.SetActive(false);
            interactions.mainObj.SetActive(false);

            if (useMinimap)
            {
                minimap.startpos = minimap.controll.GetComponent<RectTransform>().anchoredPosition;
                if (minimap.aiblock != null && minimap.aiparrent != null)
                {
                    minimap.aiblock.gameObject.SetActive(false);
                    for (int i = 0; i < rooms.ais.Count; i++)
                    {
                        if (rooms.ais[i].minimapObject != -1)
                        {
                            rooms.ais[i].mapobj = Instantiate(minimap.aiblock.rectTransform, minimap.aiparrent);
                            rooms.ais[i].mapobj.GetComponent<Image>().sprite = sprites[rooms.ais[i].minimapObject];
                            rooms.ais[i].mapobj.gameObject.SetActive(false);
                        }
                    }
                }
            }

            for (int i = 0; i < rooms.ais.Count; i++)
            {
                if (!string.IsNullOrEmpty(rooms.ais[i].layerName))
                {
                    List<string> lines = rooms.ais[i].imageIds.Split('\n').ToList();
                    if (lines.Count > 0)
                    {
                        rooms.ais[i].imgs = new List<int>();
                        rooms.ais[i].ids = new List<int>();
                        if (!string.IsNullOrEmpty(lines[0]))
                            rooms.ais[i].def = int.Parse(lines[0]);
                        for (int l = 1; l < lines.Count; l++)
                        {
                            if (!string.IsNullOrEmpty(lines[l]) && lines[l].Split(' ').Length > 1)
                            {
                                rooms.ais[i].ids.Add(int.Parse(lines[l].Split(' ')[0]));
                                rooms.ais[i].imgs.Add(int.Parse(lines[l].Split(' ')[1]));
                            }
                        }
                    }
                }
            }
            GenerateMinimap();

            CommandAdd("setroompreset", true);
            CommandAdd("spawnonroom", false);
            CommandAdd("endroomevent", false);
            CommandAdd("disableroomui", true);
            CommandAdd("enableroomui", true);
            CommandAdd("regenerateminimap", true);
            CommandAdd("showminimap", true);
            CommandAdd("getplayerroom", true);
            CommandAdd("getairoom", true);
            CommandAdd("setai", true);
            CommandAdd("setroomimage", true);
            CommandAdd("getplayerroomid", true);
            CommandAdd("isplayerroom", false);
            CommandAdd("isplayerroomid", false);


            AM.AddAddon("Open world", Command, cmds, cmdbools);
        }
    }

    List<string> cmds = new List<string>();
    List<bool> cmdbools = new List<bool>();
    void CommandAdd(string comm, bool next)
    {
        cmds.Add(comm);
        cmdbools.Add(next);
    }

    public bool Command(string comm, string[] args)
    {
        switch (comm)
        {
            case "setroompreset":
                {
                    int id = C.ValueWorkInt(args[0]);
                    if (roomsFile.Count > id && roomsFile[id] != null)
                    {
                        rooms = JsonUtility.FromJson<SLM_OW_RoomsList>(((roomsFile[id] as TextAsset).text));
                        GenerateMinimap();
                        LoadRoom(GetPos(C.ValueWorkInt(args[1])));
                        return true;
                    }
                    else
                    {
                        C.Error("Preset not exist");
                        return false;
                    }
                }
            case "spawnonroom":
                {
                    //spawns in room
                    LoadRoom(GetPos(C.ValueWorkInt(args[0])));
                    return true;
                }
            case "endroomevent":
                {
                    //just pause for commands to continue code
                    return true;
                }
            case "exitroomnoevent":
                {
                    curpos = new Vector2Int(-1, -1);
                    curroomsetupid = -1;
                    return true;
                }
            case "disableroomui":
                {
                    moves.mainObj.SetActive(false);
                    if (hasiteration)
                        interactions.mainObj.SetActive(false);
                    return true;
                }
            case "enableroomui":
                {
                    moves.mainObj.SetActive(true);
                    if (hasiteration)
                        interactions.mainObj.SetActive(true);
                    return true;
                }
            case "setroomimage":
                {
                    //setroomimage::id::imageId
                    int id = C.ValueWorkInt(args[0]);
                    int img = C.ValueWorkInt(args[1]);

                    if (id > 0 && id < rooms.setups.Count)
                    {
                        SLM_NovelImages_Block l = I.GetLayer(layerName);
                        if (img > 0 && img < l.sprites.Count)
                        {
                            rooms.setups[id].imageId = img;
                            if (curroomsetupid == id)
                                I.ShowLayer(layerName, img);
                        }
                    }

                    return true;
                }
            case "regenerateminimap":
                {
                    GenerateMinimap();
                    return true;
                }
            case "showminimap":
                {
                    if (useMinimap)
                    {
                        minimap.mainObj.SetActive(C.ValueWorkBool(args[0]));
                    }
                    return true;
                }
            case "getairoom":
                {
                    //getairoom::name::id
                    int id = C.ValueWorkInt(args[1]);
                    if (id >= 0 && id < rooms.ais.Count)
                    {
                        if (rooms.ais[id].active)
                            C.stats.SetValue(C.ValueWorkString(args[0]), GetID(rooms.ais[id].pos));
                    }
                    return true;
                }
            case "getplayerroom":
                {
                    if (curpos != new Vector2Int(-1, -1))
                    {
                        C.stats.SetValue(C.ValueWorkString(args[0]), GetID(curpos));
                    }
                    else
                        C.Error("Player not in room, use spawnonroom first!");
                    return true;
                }
            case "getplayerroomid":
                {
                    if (curpos != new Vector2Int(-1, -1))
                    {
                        string room = GetRoom(curpos);
                        int roomid = -1;
                        if (!string.IsNullOrEmpty(room) && int.TryParse(room.Split('\n')[0], out roomid))
                            C.stats.SetValue(C.ValueWorkString(args[0]), roomid);
                    }
                    else
                        C.Error("Player not in room, use spawnonroom first!");
                    return true;
                }
            case "isplayerroom":
                {
                    //isplayerroom::id::true::false
                    if (curpos != new Vector2Int(-1, -1))
                    {
                        if (GetID(curpos) == C.ValueWorkInt(args[0]))
                            C.RunCommand(C.ValueWorkCommand(args[1]));
                        else
                            C.RunCommand(C.ValueWorkCommand(args[2]));
                    }
                    else
                        C.Error("Player not in room, use spawnonroom first!");
                    return true;
                }
            case "isplayerroomid":
                {
                    //isplayerroomid::id::true::false
                    if (curpos != new Vector2Int(-1, -1))
                    {
                        string room = GetRoom(curpos);
                        int roomid = -1;
                        if (!string.IsNullOrEmpty(room) && int.TryParse(room.Split('\n')[0], out roomid))
                        {
                            if (roomid == C.ValueWorkInt(args[0]))
                                C.RunCommand(C.ValueWorkCommand(args[1]));
                            else
                                C.RunCommand(C.ValueWorkCommand(args[2]));
                        }
                        else if (!string.IsNullOrEmpty(room))
                        {
                            if (rooms.setups.Find(r => r.name == C.ValueWorkString(args[0])) != null)
                                C.RunCommand(C.ValueWorkCommand(args[1]));
                            else
                                C.RunCommand(C.ValueWorkCommand(args[2]));
                        }
                    }
                    else
                        C.Error("Player not in room, use spawnonroom first!");
                    return true;
                }
            case "isplayerwithai":
                {
                    //isplayerwithai::aiId::true::false
                    int id = C.ValueWorkInt(args[0]);
                    if (id >= 0 && id < rooms.ais.Count)
                    {
                        if (rooms.ais[id].active)
                        {
                            if (curpos == rooms.ais[id].pos)
                                C.RunCommand(C.ValueWorkCommand(args[1]));
                            else
                                C.RunCommand(C.ValueWorkCommand(args[2]));
                        }
                        else
                            C.RunCommand(C.ValueWorkCommand(args[2]));
                    }
                    else
                        C.RunCommand(C.ValueWorkCommand(args[2]));

                    return true;
                }
            case "setai":
                {
                    if (args[0] == "spawn")
                    {
                        //setai::spawn::id::room;;
                        AISpawn(C.ValueWorkInt(args[1]), C.ValueWorkInt(args[2]));
                    }
                    else if (args[0] == "disable")
                    {
                        //setai::disable::id;;
                        AIDespawn(C.ValueWorkInt(args[1]));
                    }
                    else if (args[0] == "setpath")
                    {
                        //setai::disable::id::path;;
                        int id = C.ValueWorkInt(args[1]);
                        if (id >= 0 && id < rooms.ais.Count)
                        {
                            string path = C.ValueWorkString(args[2]);
                            rooms.ais[id].path = path;
                            if (rooms.ais[id].active)
                                AISpawn(id, GetID(rooms.ais[id].pos));
                        }
                    }
                    else if (args[0] == "setmovetype")
                    {
                        //setai::setmovetype::id::type
                        int id = C.ValueWorkInt(args[1]);
                        if (id >= 0 && id < rooms.ais.Count)
                        {
                            int tpe = C.ValueWorkInt(args[2]);
                            if (tpe >= 0 && tpe <= 3)
                            {
                                rooms.ais[id].type = tpe;
                                if (rooms.ais[id].active)
                                    AISpawn(id, GetID(rooms.ais[id].pos));
                            }
                        }
                    }
                    else if (args[0] == "setspeedperstep")
                    {
                        //setai::setspeedperstep::id::count
                        int id = C.ValueWorkInt(args[1]);
                        if (id >= 0 && id < rooms.ais.Count)
                        {
                            rooms.ais[id].roomsPerStep = C.ValueWorkInt(args[2]);
                        }
                    }
                    else if (args[0] == "setdelaybeforestep")
                    {
                        //setai::setdelaybeforestep::id::count
                        int id = C.ValueWorkInt(args[1]);
                        if (id >= 0 && id < rooms.ais.Count)
                        {
                            rooms.ais[id].waitBeforeStep = C.ValueWorkInt(args[2]);
                        }
                    }
                    else if (args[0] == "showinminimap")
                    {
                        //setai::showinminimap::id::bool
                        int id = C.ValueWorkInt(args[1]);
                        if (id >= 0 && id < rooms.ais.Count)
                        {
                            if (rooms.ais[id].mapobj != null)
                            {
                                if (C.ValueWorkBool(args[2]))
                                {
                                    rooms.ais[id].mapobj.gameObject.SetActive(true);
                                }
                                else
                                {
                                    rooms.ais[id].mapobj.gameObject.SetActive(false);
                                }
                            }
                        }
                    }
                    else if (args[0] == "forcemoveall")
                    {
                        AIProcessing();
                    }
                    return true;
                }
        }
        return false;
    }

    public int GetRoomIdFromString(string input)
    {
        string line = input.Split('\n')[0];
        int ret = -1;
        if (!string.IsNullOrEmpty(line))
        {
            try
            {
                ret = int.Parse(line);
            }
            catch
            {
                SLM_OW_RoomSetUp r = rooms.setups.Find(f => f.name == line);
                if (r != null)
                    ret = rooms.setups.IndexOf(r);
            }
        }
        return ret;
    }

    void GenerateMinimap()
    {
        if (useMinimap)
        {
            for (int i = minimap.parrent.childCount - 1; i >= 0; i--)
                Destroy(minimap.parrent.GetChild(i).gameObject);

            minimap.controll.GetComponent<RectTransform>().anchoredPosition = minimap.startpos;
            minimap.controll.constraintCount = rooms.roomsH[0].rooms.Count;

            for (int h = 0; h < rooms.roomsH.Count; h++)
            {
                rooms.roomsH[h].minimaproom = new List<RectTransform>();
                for (int v = 0; v < rooms.roomsH[h].rooms.Count; v++)
                {
                    Image i = Instantiate(minimap.block, minimap.parrent);
                    rooms.roomsH[h].minimaproom.Add(i.GetComponent<RectTransform>());
                    i.gameObject.SetActive(true);
                    if (!string.IsNullOrEmpty(rooms.roomsH[h].rooms[v]))
                    {
                        List<string> lines = rooms.roomsH[h].rooms[v].Split('\n').ToList();

                        for (int s = 0; s < lines.Count; s++)
                        {
                            if (lines[s].IndexOf(";;") >= 0)
                                lines[s] = lines[s].Remove(lines[s].IndexOf(";;"));
                        }

                        if (!string.IsNullOrEmpty(lines[0]))
                        {
                            i.color = minimap.roomOn;
                            if (rooms.setups[GetRoomIdFromString(rooms.roomsH[h].rooms[v])].spriteId < sprites.Count && rooms.setups[GetRoomIdFromString(rooms.roomsH[h].rooms[v])].spriteId>=0)
                            {
                                GameObject g = FastFind.FindChild(i.transform, minimap.iconName).gameObject;
                                g.SetActive(true);
                                g.GetComponent<Image>().sprite = sprites[rooms.setups[GetRoomIdFromString(rooms.roomsH[h].rooms[v])].spriteId];
                            }
                            if (minimap.hideClosedWays)
                            {
                                if (lines.Count >= 5)
                                {
                                    if (string.IsNullOrEmpty(lines[4]) || !C.ValueWorkBool(lines[4]))
                                    {
                                        i.color = minimap.roomOff;
                                        FastFind.FindChild(i.transform, minimap.iconName).GetComponent<Image>().color = new Color(0, 0, 0, 0);
                                    }
                                }
                            }
                        }
                        else
                            i.color = minimap.roomOff;
                    }
                    else
                        i.color = minimap.roomOff;
                }
            }

            minimap.controll.GetComponent<RectTransform>().anchoredPosition = new Vector2(minimap.startpos.x + minimap.shift.x * curpos.y, minimap.startpos.y + minimap.shift.y * curpos.x);
            if (minimap.aiparrent != null)
                minimap.aiparrent.anchoredPosition = minimap.controll.GetComponent<RectTransform>().anchoredPosition;

            if (minimap.aiblock != null && minimap.aiparrent != null)
            {
                for (int i = 0; i < rooms.ais.Count; i++)
                {
                    if (rooms.ais[i].mapobj != null)
                    {
                        rooms.ais[i].mapobj.anchoredPosition = new Vector2(-minimap.shift.x / 2 - minimap.shift.x * rooms.ais[i].pos.y, -minimap.shift.y / 2 - minimap.shift.y * rooms.ais[i].pos.x);
                    }
                }
            }
        }
    }

    public void LoadRoom(Vector2Int pos)
    {
        string load = GetRoom(pos);

        if (curpos != new Vector2Int(-1, -1))
        {
            foreach (string e in rooms.setups[GetRoomIdFromString(GetRoom(curpos))].eventsOnExit.Split('\n').ToArray())
            {
                if (!string.IsNullOrEmpty(e))
                    C.RunPoint(e);
            }
        }

        curpos = pos;
        if (!string.IsNullOrEmpty(load))
        {
            List<string> lines = load.Split('\n').ToList();

            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].IndexOf(";;") >= 0)
                    lines[i] = lines[i].Remove(lines[i].IndexOf(";;"));
            }

            //show
            curroomsetupid = GetRoomIdFromString(load);
            SLM_OW_RoomSetUp room = rooms.setups[GetRoomIdFromString(load)];
            {
                try
                {
                    I.SetImage(layerName, room.imageId);
                }
                catch
                {
                    I.SetImage(layerName, room.imageId);
                }
            }

            //moves
            List<int> roomsid = new List<int>();
            {
                roomsid.Add(CheckRoom(new Vector2Int(pos.x - 1, pos.y)));
                roomsid.Add(CheckRoom(new Vector2Int(pos.x, pos.y + 1)));
                roomsid.Add(CheckRoom(new Vector2Int(pos.x + 1, pos.y)));
                roomsid.Add(CheckRoom(new Vector2Int(pos.x, pos.y - 1)));

                if (lines.Count >= 2 && !string.IsNullOrEmpty(lines[1]))
                    foreach (string s in lines[1].Split(' ').ToList())
                    {
                        roomsid.Add(CheckRoom(GetPos(int.Parse(s))));
                    }
            }
            List<int> roomspawn = new List<int>();
            if (lines.Count >= 3 && !string.IsNullOrEmpty(lines[2]))
            {
                List<string> poses = lines[2].Split(' ').ToList();
                for (int i = 0; i < poses.Count; i++)
                {
                    int p = int.Parse(poses[i]);
                    if (p < roomsid.Count)
                        roomspawn.Add(roomsid[p]);
                    else
                        roomspawn.Add(-1);
                }
            }
            else
                roomspawn = roomsid;

            moves.mainObj.SetActive(true);
            moves.up.gameObject.SetActive(false);
            moves.right.gameObject.SetActive(false);
            moves.down.gameObject.SetActive(false);
            moves.left.gameObject.SetActive(false);
            foreach (Button b in moves.additional)
                b.gameObject.SetActive(false);

            for (int i = 0; i < roomspawn.Count; i++)
            {
                int id = roomspawn[i];
                if (id >= 0)
                {
                    if (i == 0)
                    {
                        moves.up.gameObject.SetActive(true);
                        moves.up.onClick.RemoveAllListeners();
                        moves.up.onClick.AddListener(() => Move(id));
                    }
                    else if (i == 1)
                    {
                        moves.right.gameObject.SetActive(true);
                        moves.right.onClick.RemoveAllListeners();
                        moves.right.onClick.AddListener(() => Move(id));
                    }
                    else if (i == 2)
                    {
                        moves.down.gameObject.SetActive(true);
                        moves.down.onClick.RemoveAllListeners();
                        moves.down.onClick.AddListener(() => Move(id));
                    }
                    else if (i == 3)
                    {
                        moves.left.gameObject.SetActive(true);
                        moves.left.onClick.RemoveAllListeners();
                        moves.left.onClick.AddListener(() => Move(id));
                    }
                    else if (i - 4 < moves.additional.Count)
                    {
                        moves.additional[i - 4].gameObject.SetActive(true);
                        moves.additional[i - 4].onClick.RemoveAllListeners();
                        moves.additional[i - 4].onClick.AddListener(() => Move(id));
                    }
                }
            }

            //map
            if (useMinimap)
            {
                minimap.controll.GetComponent<RectTransform>().anchoredPosition = new Vector2(minimap.startpos.x + minimap.shift.x * curpos.y, minimap.startpos.y + minimap.shift.y * curpos.x);
                if (minimap.aiparrent != null)
                    minimap.aiparrent.anchoredPosition = minimap.controll.GetComponent<RectTransform>().anchoredPosition;
            }

            //events
            foreach (string e in room.eventsOnEnter.Split('\n').ToArray())
            {
                if (!string.IsNullOrEmpty(e))
                    C.RunPoint(e);
            }
            SetUpIteration(room);

            //ai 
            AIProcessing();
        }
        else
            C.Error("Room " + (pos.x * rooms.roomsH[0].rooms.Count + pos.y) + " not exist");


    }

    public void SetUpIteration(SLM_OW_RoomSetUp room)
    {
        if (!string.IsNullOrEmpty(room.roomIterationPoints))
        {
            bool checkbools = true;
            if (string.IsNullOrEmpty(room.roomIterationBools))
                checkbools = false;
            List<string> points = room.roomIterationPoints.Split('\n').ToList();
            List<string> texts = room.roomIterationTexts.Split('\n').ToList();
            List<string> bools = room.roomIterationBools.Split('\n').ToList();

            for (int i = 0; i < points.Count; i++)
            {
                if (points[i].IndexOf(";;") >= 0)
                    points[i] = points[i].Remove(points[i].IndexOf(";;"));
            }
            for (int i = 0; i < texts.Count; i++)
            {
                if (texts[i].IndexOf(";;") >= 0)
                    texts[i] = texts[i].Remove(texts[i].IndexOf(";;"));
            }
            for (int i = 0; i < bools.Count; i++)
            {
                if (bools[i].IndexOf(";;") >= 0)
                    bools[i] = bools[i].Remove(bools[i].IndexOf(";;"));
            }

            if (points.Count != texts.Count)
            {
                C.Error("Points and texts not mach in room " + room.name);
                return;
            }
            if (checkbools && points.Count != bools.Count)
            {
                C.Error("Points and bools not mach in room " + room.name + ", clear bools Field or make it mach");
                return;
            }


            interactions.mainObj.SetActive(true);
            hasiteration = true;

            interactions.text.text = C.blocks[C.currentid].texts[C.ValueWorkInt(room.IterationLogo)];

            int interount = 0;

            if (interactions.autogen)
            {
                for (int i = interactions.parent.childCount - 1; i >= 0; i--)
                    Destroy(interactions.parent.GetChild(i).gameObject);

                for (int i = 0; i < points.Count; i++)
                {
                    if (i < interactions.maxchoice)
                    {
                        bool can = true;
                        if (checkbools)
                            if (!C.ValueWorkBool(bools[i]))
                                can = false;
                        if (can)
                        {
                            interount++;
                            string point = points[i];
                            Button b = Instantiate(interactions.spawnobj.gameObject, interactions.parent).GetComponent<Button>();
                            b.gameObject.SetActive(true);
                            b.onClick.RemoveAllListeners();
                            b.onClick.AddListener(() => C.RunPoint(point));
                            FastFind.FindChild(b.transform, interactions.textName).GetComponent<Text>().text = C.blocks[C.currentid].texts[C.ValueWorkInt(texts[i])];
                        }
                    }
                }
            }
            else
            {
                foreach (Button b in interactions.blocks)
                    b.gameObject.SetActive(false);

                for (int i = 0; i < points.Count; i++)
                {
                    if (i < interactions.blocks.Count)
                    {
                        bool can = true;
                        if (checkbools)
                            if (!C.ValueWorkBool(bools[i]))
                                can = false;
                        if (can)
                        {
                            interount++;
                            string point = points[i];
                            interactions.blocks[i].gameObject.SetActive(true);
                            interactions.blocks[i].onClick.RemoveAllListeners();
                            interactions.blocks[i].onClick.AddListener(() => C.RunPoint(point));
                            FastFind.FindChild(interactions.blocks[i].transform, interactions.textName).GetComponent<Text>().text = C.blocks[C.currentid].texts[C.ValueWorkInt(texts[i])];
                        }
                    }
                }
            }

            if (interount==0)
			{
                interactions.mainObj.SetActive(false);
                hasiteration = false;
            }
        }
        else
        {
            interactions.mainObj.SetActive(false);
            hasiteration = false;
        }
    }

    public void Move(int id)
    {
        LoadRoom(GetPos(id));
    }

    int CheckRoom(Vector2Int pos, bool ai = false, int aiid = -1, bool boolscheck = false)
    {
        int ret = -1;

        if (pos.x >= 0 && pos.x < rooms.roomsH.Count)
        {
            if (pos.y >= 0 && pos.y < rooms.roomsH[pos.x].rooms.Count)
            {
                string s = GetRoom(pos);
                if (!string.IsNullOrEmpty(s))
                {
                    List<string> lines = s.Split('\n').ToList();

                    for (int i = 0; i < lines.Count; i++)
                    {
                        if (lines[i].IndexOf(";;") >= 0)
                            lines[i] = lines[i].Remove(lines[i].IndexOf(";;"));
                    }

                    if (GetRoomIdFromString(s) != -1)
                    {
                        if (!ai)
                        {
                            if (aiid == -1)
                            {
                                if (lines.Count >= 5)
                                {
                                    if (C.ValueWorkBool(lines[4]))
                                        ret = GetID(pos);
                                }
                                else
                                    ret = GetID(pos);
                            }
                            else
                            {
                                if (boolscheck && lines.Count >= 5)
                                {
                                    if (C.ValueWorkBool(lines[4]))
                                        ret = GetID(pos);
                                }
                                else
                                    ret = GetID(pos);
                            }
                        }
                        else
                        {
                            bool can = false;
                            if (lines.Count >= 4)
                            {
                                if (lines[3].IndexOf(aiid + "") >= 0)
                                {
                                    if (boolscheck && lines.Count >= 5)
                                    {
                                        if (C.ValueWorkBool(lines[4]))
                                            can = true;
                                    }
                                    else
                                        can = true;
                                }
                                else
                                {
                                    if (boolscheck && lines.Count >= 5)
                                    {
                                        if (C.ValueWorkBool(lines[4]))
                                            can = true;
                                    }
                                    else
                                        can = true;
                                }
                            }

                            if (can)
                                ret = GetID(pos);
                        }
                    }

                }
            }
        }

        return ret;
    }

    Vector2Int GetPos(int id)
    {
        return new Vector2Int((int)(id / rooms.roomsH[0].rooms.Count), id - ((int)(id / rooms.roomsH[0].rooms.Count) * rooms.roomsH[0].rooms.Count));
    }

    int GetID(Vector2Int pos)
    {
        return pos.x * rooms.roomsH[0].rooms.Count + pos.y;
    }

    string GetRoom(Vector2Int pos)
    {
        string ret = "";
        if (pos.x < rooms.roomsH.Count)
            if (pos.y < rooms.roomsH[pos.x].rooms.Count)
                ret = rooms.roomsH[pos.x].rooms[pos.y];
            else
                C.Error("Room  " + pos.x + "/" + pos.y + " out of grid!");
        else
            C.Error("Room  " + pos.x + "/" + pos.y + " out of grid!");
        return ret;
    }

    void AIProcessing()
	{
        for (int i=0;i<rooms.ais.Count;i++)
		{
            if (rooms.ais[i].active)
            {
                if (rooms.ais[i].curentWait <= 0)
                {
                    for (int s = 0; s < rooms.ais[i].roomsPerStep; s++)
					{
                        if (s == rooms.ais[i].roomsPerStep - 1)
                            rooms.ais[i].finalstep = true;
                        else
                            rooms.ais[i].finalstep = false;
                        
                        if (rooms.ais[i].stopIfTouchPlayer)
                        {
                            if (curpos == rooms.ais[i].pos)
							{
                                if (s == 0 && rooms.ais[i].ignoreFirstStep)
                                {
                                    MoveAI(rooms.ais[i], i);
                                }
                                else
                                {
                                    rooms.ais[i].finalstep = true;
                                    SpawnAiOnRoom(rooms.ais[i], false);
                                    break;
                                }
							}
                            else
                                MoveAI(rooms.ais[i], i);
                        }
                        else
                            MoveAI(rooms.ais[i], i);
                    }
                        
                    rooms.ais[i].curentWait = rooms.ais[i].waitBeforeStep;
                }
                else
                {
                    SpawnAiOnRoom(rooms.ais[i], true);

                    if (rooms.ais[i].runEventsWhenStay)
                        CheckAiEvents(rooms.ais[i]);

                    rooms.ais[i].curentWait--;
                }
            }
		}
	}


    public void AISpawn(int id, int spawnId)
    {
        if (!string.IsNullOrEmpty(GetRoom(GetPos(spawnId))) && id>=0 && id<rooms.ais.Count)
		{
            if (rooms.ais[id].type == 1 || rooms.ais[id].type == 2)
            {
                rooms.ais[id].rooms = new List<Vector2Int>();
                foreach (string s in rooms.ais[id].path.Split(' ').ToList())
				{
                    int i = int.Parse(s);
                    rooms.ais[id].rooms.Add(GetPos(i));
                }

                if (rooms.ais[id].rooms.Count > 0)
                {
                    rooms.ais[id].active = true;
                    rooms.ais[id].finalstep = true;
                    if (rooms.ais[id].mapobj != null)
                        rooms.ais[id].mapobj.gameObject.SetActive(true);

                    if (rooms.ais[id].type == 1)
                    {
                        if (!rooms.ais[id].rooms.Contains(GetPos(spawnId)))
                            rooms.ais[id].pos = rooms.ais[id].rooms[Random.Range(0, rooms.ais[id].rooms.Count)];
                        else
                            rooms.ais[id].pos = GetPos(spawnId);
                    }
                    else if (rooms.ais[id].type == 2)
                    {
                        if (!rooms.ais[id].rooms.Contains(GetPos(spawnId)))
                            rooms.ais[id].pos = rooms.ais[id].rooms[0];
                        else
                            rooms.ais[id].pos = GetPos(spawnId);
                    }

                    if (rooms.ais[id].waitBeforeStep == 0)
                        SpawnAiOnRoom(rooms.ais[id]);
                    else
                        rooms.ais[id].curentWait = rooms.ais[id].waitBeforeStep;
                }
                else
                    C.Error("Ai path not setted!");
            }
            else if (rooms.ais[id].type == 0)
			{
                if (rooms.ais[id].mapobj != null)
                    rooms.ais[id].mapobj.gameObject.SetActive(true);
                rooms.ais[id].active = true;
                rooms.ais[id].finalstep = true;
                rooms.ais[id].pos = GetPos(spawnId);
                if (rooms.ais[id].waitBeforeStep == 0)
                    SpawnAiOnRoom(rooms.ais[id]);
                else
                    rooms.ais[id].curentWait = rooms.ais[id].waitBeforeStep;
            }
            else if (rooms.ais[id].type == 3)
			{
                if (rooms.ais[id].mapobj != null)
                    rooms.ais[id].mapobj.gameObject.SetActive(true);
                rooms.ais[id].active = true;
                rooms.ais[id].finalstep = true;
                rooms.ais[id].pos = GetPos(spawnId);
                SpawnAiOnRoom(rooms.ais[id]);
            }
        }
    }

    public void AIDespawn(int id)
    {
        rooms.ais[id].active = false;

        if (rooms.ais[id].mapobj != null)
            rooms.ais[id].mapobj.gameObject.SetActive(false);

        if (!string.IsNullOrEmpty(rooms.ais[id].layerName))
            I.DisableLayer(rooms.ais[id].layerName);
    }

    public void MoveAI(SLM_OW_AI ai, int index)
	{
        bool checkAi = ai.checkAiLine;
        bool checkBool = ai.checkBools;
        if (ai.type==0)
		{
            List<int> roomsid = new List<int>();
            List<int> roomsidd = new List<int>();
            List<string> lines = GetRoom(ai.pos).Split('\n').ToList();

            {
                roomsidd.Add(CheckRoom(new Vector2Int(ai.pos.x - 1, ai.pos.y), checkAi, index, checkBool));
                roomsidd.Add(CheckRoom(new Vector2Int(ai.pos.x, ai.pos.y + 1), checkAi, index, checkBool));
                roomsidd.Add(CheckRoom(new Vector2Int(ai.pos.x + 1, ai.pos.y), checkAi, index, checkBool));
                roomsidd.Add(CheckRoom(new Vector2Int(ai.pos.x, ai.pos.y - 1), checkAi, index, checkBool));

                if (lines.Count >= 2 && !string.IsNullOrEmpty(lines[1]))
                    foreach (string s in lines[1].Split(' ').ToList())
                    {
                        roomsidd.Add(CheckRoom(GetPos(int.Parse(s)), checkAi, index, checkBool));
                    }
            }
			{
                if (lines.Count >= 3 && !string.IsNullOrEmpty(lines[2]))
                {
                    List<string> rm = lines[2].Split(' ').ToList();

                    foreach (string r in rm)
					{
                        int ir = int.Parse(r);
                        if (ir >= 0 && ir < roomsidd.Count)
                            roomsid.Add(roomsidd[ir]);
                    }
                }
                else
                    roomsid = roomsidd;

            }

            if (ai.canDontMove)
                roomsid.Add(CheckRoom(ai.pos, checkAi, index, checkBool));

            for (int i = roomsid.Count - 1; i >= 0; i--)
                if (roomsid[i] == -1)
                    roomsid.RemoveAt(i);

            if (roomsid.Count>0)
			{
                ai.pos = GetPos(roomsid[Random.Range(0, roomsid.Count)]);
                rooms.ais[index].pos = ai.pos;
                SpawnAiOnRoom(ai);
            }
        }
        else if (ai.type == 1)
		{
            bool ok=false;
            bool found = false;
            int max = ai.rooms.Count * 2;
            if (ai.rooms.Count > 0)
            {
                while (!ok)
                {
                    if (max > 0)
                    {
                        Vector2Int rnd = ai.rooms[Random.Range(0, ai.rooms.Count)];
                        string data = GetRoom(rnd);
                        if (!string.IsNullOrEmpty(data))
						{
                            if (ai.checkBools)
                            {
                                if (CheckRoom(rnd, checkAi, index, checkBool) != -1)
                                {
                                    ai.pos = rnd;
                                    rooms.ais[index].pos = ai.pos;
                                    ok = true;
                                    found = true;
                                }
                                else
                                    continue;
                            }
                            else
							{
                                ai.pos = rnd;
                                rooms.ais[index].pos = ai.pos;
                                ok = true;
                                found = true;
                            }
						}
                        max--;
                    }
                    else
                        return;
                }

                if (found)
                    SpawnAiOnRoom(ai);
                else
                    C.Error("Ai can't fount path!");
            }
		}
        else if (ai.type==2)
		{
            int id = ai.rooms.IndexOf(ai.pos);
            if (id < 0 || id >= ai.rooms.Count)
            {
                id = 0;
            }
            ai.pos = ai.rooms[id];
            rooms.ais[index].pos = ai.pos;
            SpawnAiOnRoom(ai);
            id++;
            if (id < 0 || id >= ai.rooms.Count)
            {
                id = 0;
            }
            ai.pos = ai.rooms[id];
            rooms.ais[index].pos = ai.pos;
        }
        else if (ai.type == 3)
		{
            SpawnAiOnRoom(ai);
        }

    }

    void SpawnAiOnRoom(SLM_OW_AI ai, bool noevents=false)
	{
        //all visual effects, rooms 100% exist
        //pos getted from ai.pos

        if (ai.finalstep)
        {
            if (!string.IsNullOrEmpty(ai.layerName))
            {
                if (ai.pos == curpos)
                {
                    int id = -1;

                    if (ai.ids.Contains(GetRoomIdFromString(GetRoom(ai.pos))))
                    {
                        id = ai.imgs[ai.ids.IndexOf(GetRoomIdFromString(GetRoom(ai.pos)))];
                    }
                    else
                        id = ai.def;


                    if (id == -1)
                        I.DisableLayer(ai.layerName);
                    else
                        I.ShowLayer(ai.layerName, id);
                }
                else
                {
                    I.DisableLayer(ai.layerName);
                }
            }
        }

        if (useMinimap && ai.mapobj != null)
        {
            if (minimap.block.rectTransform.anchoredPosition == rooms.roomsH[ai.pos.x].minimaproom[ai.pos.y].anchoredPosition)
                ai.mapobj.anchoredPosition = new Vector2(-minimap.shift.x / 2 - minimap.shift.x * ai.pos.y, -minimap.shift.y / 2 - minimap.shift.y * ai.pos.x);
            else
                ai.mapobj.anchoredPosition = rooms.roomsH[ai.pos.x].minimaproom[ai.pos.y].anchoredPosition;
        }

        if (!noevents)
            CheckAiEvents(ai);
    }

    void CheckAiEvents(SLM_OW_AI ai)
	{
        if (ai.pos == curpos)
		{
            List<string> lines = ai.eventsInRoomsWithPlayer.Split('\n').ToList();
            foreach (string l in lines)
                if (!string.IsNullOrEmpty(l))
                    C.RunPoint(l);
        }
        else
		{
            List<string> lines = ai.eventsInRooms.Split('\n').ToList();
            foreach (string l in lines)
                if (!string.IsNullOrEmpty(l))
                    C.RunPoint(l);
        }
	}
}

[System.Serializable]
public class SLM_OW_RoomsList
{
    public List<SLM_OW_Rooms> roomsH;
    public List<SLM_OW_RoomSetUp> setups;
    public List<SLM_OW_AI> ais;
}

[System.Serializable]
public class SLM_OW_Rooms
{
    public List<string> rooms;
    public List<RectTransform> minimaproom;
    /*
    id
    additional rooms - 22 44 (4 5)
    pos of buttons - 0 1 2 3 4 5
    AI ids
    bool
     */
}

[System.Serializable]
public class SLM_OW_RoomSetUp
{
    public string name;
    public int imageId;
    public int spriteId=-1;
    public string IterationLogo;
    public string roomIterationPoints;
    public string roomIterationTexts;
    public string roomIterationBools;
    public string eventsOnEnter;
    public string eventsOnExit;
    public bool show;
}

[System.Serializable]
public class SLM_OW_Interactions
{
    public GameObject mainObj;
    public Text text;
    public int maxchoice = 10;
    public string textName;
    public List<Button> blocks;
    public bool autogen;
    public Transform parent;
    public Button spawnobj;
}

[System.Serializable]
public class SLM_OW_Move
{
    public GameObject mainObj;
    public Button up;
    public Button right;
    public Button down;
    public Button left;
    public List<Button> additional;
}

[System.Serializable]
public class SLM_OW_Minimap
{
    public GameObject mainObj;
    public GridLayoutGroup controll;
    public Color roomOn = Color.white;
    public Color roomOff;
    public Image block;
    public string iconName;
    public Transform parrent;
    public Vector2 shift;
    public Image aiblock;
    public RectTransform aiparrent;
    public bool hideClosedWays;
    [HideInInspector] public Vector2 startpos;
}

[System.Serializable]
public class SLM_OW_AI
{
    public string name; //name for editor
    public bool active; //active now?
    public string layerName; //layer in NovelImages
    public string imageIds; // id image form: "id image"
    public int minimapObject = -1; //id of sprite -1 off
    public bool runEventsWhenStay;
    public string eventsInRooms;
    public string eventsInRoomsWithPlayer;
    public int type; // 0 - nearest 1 - nearest 2 - path 3 - dont move but check
    public bool canDontMove = true;
    public bool checkBools;
    public bool checkAiLine;
    public string path;
    public List<Vector2Int> rooms;
    public int roomsPerStep = 1;
    public bool stopIfTouchPlayer = true;
    public bool ignoreFirstStep = false;
    public int waitBeforeStep = 0;
    public bool show;

    [HideInInspector] public int curentWait; //waiting before step
    [HideInInspector] public Vector2Int pos; //position
    [HideInInspector] public RectTransform mapobj; //object in map
    [HideInInspector] public List<int> ids; //ids of rooms for layer
    [HideInInspector] public List<int> imgs; //ids imgs in layer
    [HideInInspector] public int def; //def image in layer
    [HideInInspector] public bool finalstep; //last step to update ui
}