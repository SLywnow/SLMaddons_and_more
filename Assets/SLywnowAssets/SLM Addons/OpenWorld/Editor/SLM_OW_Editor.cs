using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SLywnow;
using UnityEditor.SceneManagement;

public class SLM_OW_Editor : EditorWindow
{

	SLM_OW_Main main;
	SLM_OW_RoomsList rooms;
	bool opened;
	Vector2 pos;
	Vector2 pos2;
	Vector2 pos3;
	Vector2Int size;
	Vector2Int newsize;
	enum curs { grid, rooms, ais };
	curs tab;

	int fileid = 0;
	bool canopen = false;
	bool showupdate;
	bool imagegrid;
	bool imagegridicons;
	float scale = 1;

	public bool mapgenerator;
	Vector2 posgrid;
	Texture2D gridmap;
	Texture2D checknewgridmap;
	List<Color> mapcolors;
	List<string> mapstrings;

	Vector2Int curroomselect = new Vector2Int(-1, -1);
	string stringtocopy;

	GUIStyle styleF = new GUIStyle();
	GUIStyle styleB = new GUIStyle();
	GUIStyle style = new GUIStyle();

	bool first;

	void OnGUI()
	{
		style.richText = true;

		styleB = GUI.skin.GetStyle("Button");
		styleB.richText = true;

		styleF = GUI.skin.GetStyle("Button");
		styleF.richText = true;
		if (!Application.isPlaying)
		{

			if (!first)
			{
				main = FindObjectOfType<SLM_OW_Main>();
				first = true;
				if (main != null)
					canopen = true;
			}

			GUILayout.BeginHorizontal();
			main = EditorGUILayout.ObjectField("Main script", main, typeof(SLM_OW_Main), true) as SLM_OW_Main;
			if (canopen)
			{
				if (GUILayout.Button("Open"))
				{
					if (main.useroomFile)
					{
						rooms = JsonUtility.FromJson<SLM_OW_RoomsList>((main.roomsFile[fileid] as TextAsset).text);
						if (rooms != null)
							size = new Vector2Int(rooms.roomsH.Count, rooms.roomsH[0].rooms.Count);
						else
						{
							rooms = new SLM_OW_RoomsList();
							size = new Vector2Int(1, 1);
							rooms.ais = new List<SLM_OW_AI>();
							rooms.setups = new List<SLM_OW_RoomSetUp>();
							rooms.roomsH = new List<SLM_OW_Rooms>();
							for (int h = 0; h < 1; h++)
							{
								rooms.roomsH.Add(new SLM_OW_Rooms());
								rooms.roomsH[h].rooms = new List<string>();
								for (int v = 0; v < 1; v++)
								{
									rooms.roomsH[h].rooms.Add("");
								}
							}
						}
					}
					else
					{
						if (main.rooms.roomsH != null && main.rooms.roomsH.Count > 0)
						{
							rooms = main.rooms;
							size = new Vector2Int(rooms.roomsH.Count, rooms.roomsH[0].rooms.Count);
						}
						else
						{
							rooms = new SLM_OW_RoomsList();
							size = new Vector2Int(1, 1);
							rooms.roomsH = new List<SLM_OW_Rooms>();
							rooms.ais = new List<SLM_OW_AI>();
							rooms.setups = new List<SLM_OW_RoomSetUp>();
							for (int h = 0; h < 1; h++)
							{
								rooms.roomsH.Add(new SLM_OW_Rooms());
								rooms.roomsH[h].rooms = new List<string>();
								for (int v = 0; v < 1; v++)
								{
									rooms.roomsH[h].rooms.Add("");
								}
							}
						}
					}
					newsize = new Vector2Int(size.y, size.x);
					tab = curs.grid;
					opened = true;
				}
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			if (main != null && main.useroomFile)
			{
				if (main.roomsFile.Count > 0)
				{
					GUILayout.Label("Select file");
					fileid = EditorGUILayout.IntSlider(fileid, 0, main.roomsFile.Count - 1);
					canopen = true;
				}
				else
				{
					GUILayout.Label("<color=red>Add some file!</color>", style);
					canopen = false;
				}
			}
			else if (main != null)
			{
				canopen = true;
			}
			GUILayout.EndHorizontal();

			EditorGUILayout.Space();

			if (main != null && opened)
			{
				if (tab == curs.grid)
				{
					GUILayout.BeginHorizontal();
					if (GUILayout.Button("<color=cyan>Grid</color>", styleB))
					{
						tab = curs.grid;
						showupdate = false;
						imagegrid = false;
					}
					if (GUILayout.Button("Room SetUp", styleB))
					{
						tab = curs.rooms;
					}
					if (GUILayout.Button("Ai", styleB))
					{
						tab = curs.ais;
					}
					GUILayout.EndHorizontal();
					EditorGUILayout.Space();

					pos = GUILayout.BeginScrollView(pos, true, true);
					if (rooms != null)
					{
						for (int h = 0; h < rooms.roomsH.Count; h++)
						{
							GUILayout.BeginHorizontal();
							for (int v = 0; v < rooms.roomsH[h].rooms.Count; v++)
							{
								GUILayout.BeginVertical();
								if (imagegrid)
								{
									int id = -1;
									Vector2Int curpos = new Vector2Int(h, v);
									try
									{
										id = int.Parse(rooms.roomsH[h].rooms[v].Split('\n')[0]);
									}
									catch 
									{
										SLM_OW_RoomSetUp r = main.rooms.setups.Find(f => f.name == rooms.roomsH[h].rooms[v].Split('\n')[0]);
										if (r != null)
											id = main.rooms.setups.IndexOf(r);
									}
									if ((id >= 0 && id < rooms.setups.Count) && rooms.setups[id].imageId >= 0 && rooms.setups[id].imageId < main.C.imagesc.GetLayer(main.layerName).sprites.Count)
									{
										if (imagegridicons)
										{
											if ((rooms.setups[id].spriteId >= 0 && rooms.setups[id].spriteId < main.sprites.Count))
											{
												GUILayout.Label(new GUIContent((h * size.y + v) + " " + rooms.setups[id].name, main.sprites[rooms.setups[id].spriteId].texture), GUILayout.Width(80 * scale), GUILayout.Height(20 * scale));
												if (GUILayout.Button(new GUIContent(main.C.imagesc.GetLayer(main.layerName).sprites[rooms.setups[id].imageId].texture), GUILayout.Width(96 * scale), GUILayout.Height(50 * scale)))
												{
													if (curroomselect == curpos)
														curroomselect = new Vector2Int(-1, -1);
													else
														curroomselect = curpos;
												}
											}
											else
											{
												GUILayout.Label(new GUIContent((h * size.y + v) + " " + rooms.setups[id].name), GUILayout.Width(80 * scale), GUILayout.Height(20 * scale));
												if (GUILayout.Button(new GUIContent(main.C.imagesc.GetLayer(main.layerName).sprites[rooms.setups[id].imageId].texture), GUILayout.Width(96 * scale), GUILayout.Height(50 * scale)))
												{
													if (curroomselect == curpos)
														curroomselect = new Vector2Int(-1, -1);
													else
														curroomselect = curpos;
												}
											}
										}
										else
										{
											if (GUILayout.Button(new GUIContent(main.C.imagesc.GetLayer(main.layerName).sprites[rooms.setups[id].imageId].texture), GUILayout.Width(96 * scale), GUILayout.Height(60 * scale)))
											{
												if (curroomselect == curpos)
													curroomselect = new Vector2Int(-1, -1);
												else
													curroomselect = curpos;
											}
										}
									}
									else
									{
										if (GUILayout.Button(new GUIContent(""), GUILayout.Width(96 * scale), GUILayout.Height(60 * scale)))
										{
											if (curroomselect == curpos)
												curroomselect = new Vector2Int(-1, -1);
											else
												curroomselect = curpos;
										}
									}

									if (curroomselect == curpos)
										GUILayout.Label("●", GUILayout.Width(96 * scale));
								}
								else
								{
									GUILayout.Label((h * size.y + v) + "", GUILayout.Width(96 * scale));
									rooms.roomsH[h].rooms[v] = GUILayout.TextArea(rooms.roomsH[h].rooms[v], GUILayout.Width(96 * scale));
								}
								GUILayout.EndVertical();
							}
							GUILayout.EndHorizontal();

						}
					}
					GUILayout.EndScrollView();

					//scale
					EditorGUILayout.Space();
					GUILayout.BeginHorizontal();
					scale = EditorGUILayout.Slider("Grid scale", scale, 0.5f, 5);
					GUILayout.EndHorizontal();

					EditorGUILayout.Space();
					GUILayout.BeginHorizontal();
					if (!imagegrid)
						showupdate = EditorGUILayout.BeginFoldoutHeaderGroup(showupdate, "Update Settings");
					if (main.C.imagesc != null & main.C.imagesc.GetLayer(main.layerName) != null)
						imagegrid = GUILayout.Toggle(imagegrid, "Visualize grid");
					if (imagegrid)
						imagegridicons = GUILayout.Toggle(imagegridicons, "Show icons and ids");
					GUILayout.EndHorizontal();

					if (showupdate && !imagegrid)
					{
						GUILayout.BeginHorizontal();
						mapgenerator = GUILayout.Toggle(mapgenerator, "Map generator mode");
						GUILayout.EndHorizontal();

						if (!mapgenerator)
						{
							GUILayout.BeginHorizontal();
							newsize = EditorGUILayout.Vector2IntField("Size", newsize);
							GUILayout.BeginVertical();
							GUILayout.Label("<color=red>Resizing deletes old data</color>", style);
							if (GUILayout.Button("Update"))
							{
								size = new Vector2Int(newsize.y, newsize.x);

								rooms.roomsH = new List<SLM_OW_Rooms>();
								for (int h = 0; h < size.x; h++)
								{
									rooms.roomsH.Add(new SLM_OW_Rooms());
									rooms.roomsH[h].rooms = new List<string>();
									for (int v = 0; v < size.y; v++)
									{
										rooms.roomsH[h].rooms.Add("");
									}
								}
							}
							GUILayout.EndVertical();
							GUILayout.EndHorizontal();
						}
						else
						{
							posgrid = GUILayout.BeginScrollView(posgrid);
							GUILayout.BeginHorizontal();
							GUILayout.Label("Texture");
							gridmap = EditorGUILayout.ObjectField(gridmap, typeof(Texture2D), false) as Texture2D;
							if (gridmap != null)
								GUILayout.Label(new GUIContent(gridmap), GUILayout.Width(50), GUILayout.Height(30));
							GUILayout.EndHorizontal();

							if (gridmap != null)
							{
								if (gridmap != checknewgridmap)
								{
									mapcolors = new List<Color>();
									mapstrings = new List<string>();

									for (int x = 0; x < gridmap.width; x++)
										for (int y = 0; y < gridmap.height; y++)
										{
											if (!mapcolors.Contains(gridmap.GetPixel(x, y)))
											{
												mapcolors.Add(gridmap.GetPixel(x, y));
												mapstrings.Add("");
											}
										}

									checknewgridmap = gridmap;
								}
								else
								{
									GUILayout.BeginVertical();
									GUILayout.BeginHorizontal();
									GUILayout.Label("Color", GUILayout.Width(50));
									GUILayout.Label("String");
									GUILayout.EndHorizontal();
									for (int i = 0; i < mapcolors.Count; i++)
									{
										int id = i;
										GUILayout.BeginHorizontal();
										EditorGUILayout.ColorField(mapcolors[id], GUILayout.Width(50));
										mapstrings[id] = EditorGUILayout.TextArea(mapstrings[id]);
										if (!string.IsNullOrEmpty(mapstrings[id]))
											if (int.TryParse(mapstrings[id].Split('\n')[0], out int p))
											{
												if (p >= 0 && p < rooms.setups.Count)
												{
													GUILayout.Label(rooms.setups[p].name);
													if (main.C.imagesc != null && main.C.imagesc.GetLayer(main.layerName) != null && rooms.setups[p].imageId > 0 && rooms.setups[p].imageId < main.C.imagesc.GetLayer(main.layerName).sprites.Count)
													{
														GUILayout.Label(new GUIContent(main.C.imagesc.GetLayer(main.layerName).sprites[rooms.setups[p].imageId].texture), GUILayout.Width(96), GUILayout.Height(50));
													}
												}
											}
										GUILayout.EndHorizontal();
									}
								}
								GUILayout.EndVertical();

								GUILayout.Label("<color=red>That rewrite old grid data!</color>", style);
								if (GUILayout.Button("Run"))
								{
									size = new Vector2Int(gridmap.width, gridmap.height);
									newsize = size;

									rooms.roomsH = new List<SLM_OW_Rooms>();

									for (int y = 0; y < gridmap.height; y++)
									{
										rooms.roomsH.Add(new SLM_OW_Rooms());
										rooms.roomsH[y].rooms = new List<string>();
									}

									for (int y = 0; y < gridmap.height; y++)
									{
										for (int x = 0; x < gridmap.width; x++)
										{
											rooms.roomsH[rooms.roomsH.Count - 1 - y].rooms.Add("");
											if (mapcolors.Contains(gridmap.GetPixel(x, y)))
											{
												rooms.roomsH[rooms.roomsH.Count - 1 - y].rooms[x] = mapstrings[mapcolors.IndexOf(gridmap.GetPixel(x, y))];
											}
										}
									}
								}
							}
							GUILayout.EndScrollView();
						}
					}

					if (imagegrid)
					{
						if (curroomselect != new Vector2Int(-1, -1))
						{
							GUILayout.Label("Selected: " + (curroomselect.x * size.y + curroomselect.y));

							GUILayout.BeginHorizontal();
							GUILayout.Label("RoomId" + '\n' + "Additional rooms warp" + '\n' + "Buttons pos" + '\n' + "Ai border" + '\n' + "bool (vw)", GUILayout.Width(150));
							rooms.roomsH[curroomselect.x].rooms[curroomselect.y] = GUILayout.TextArea(rooms.roomsH[curroomselect.x].rooms[curroomselect.y]);
							GUILayout.EndHorizontal();

							GUILayout.BeginHorizontal();
							if (GUILayout.Button("Copy"))
							{
								stringtocopy = rooms.roomsH[curroomselect.x].rooms[curroomselect.y];
							}
							if (GUILayout.Button("Paste"))
							{
								rooms.roomsH[curroomselect.x].rooms[curroomselect.y] = stringtocopy;
							}
							GUILayout.EndHorizontal();
						}
					}
				}
				else if (tab == curs.rooms)
				{
					GUILayout.BeginHorizontal();
					if (GUILayout.Button("Grid", styleB))
					{
						tab = curs.grid;
						showupdate = false;
						imagegrid = false;
					}
					if (GUILayout.Button("<color=cyan>Room SetUp</color>"))
					{
						tab = curs.rooms;
					}
					if (GUILayout.Button("Ai", styleB))
					{
						tab = curs.ais;
					}
					GUILayout.EndHorizontal();
					EditorGUILayout.Space();

					pos2 = GUILayout.BeginScrollView(pos2, false, true);
					for (int i = 0; i < rooms.setups.Count; i++)
					{
						int id = i;
						GUILayout.BeginHorizontal();
						rooms.setups[id].show = EditorGUILayout.BeginFoldoutHeaderGroup(rooms.setups[id].show, id + " " + rooms.setups[id].name);
						if (rooms.setups[id].spriteId >= 0 && rooms.setups[id].spriteId < main.sprites.Count)
							GUILayout.Label(new GUIContent(main.sprites[rooms.setups[id].spriteId].texture), GUILayout.Width(60), GUILayout.Height(60));
						if (main.C.imagesc != null && main.C.imagesc.GetLayer(main.layerName) != null && rooms.setups[id].imageId >= 0 && rooms.setups[id].imageId < main.C.imagesc.GetLayer(main.layerName).sprites.Count)
							GUILayout.Label(new GUIContent(main.C.imagesc.GetLayer(main.layerName).sprites[rooms.setups[id].imageId].texture), GUILayout.Width(96), GUILayout.Height(60));

						GUILayout.EndHorizontal();

						if (rooms.setups[id].show)
						{
							GUILayout.BeginHorizontal();
							GUILayout.Label("Name: ");
							rooms.setups[id].name = GUILayout.TextField(rooms.setups[id].name);
							GUILayout.EndHorizontal();
							GUILayout.BeginHorizontal();
							GUILayout.Label("Image ID: ");
							rooms.setups[id].imageId = EditorGUILayout.IntField(rooms.setups[id].imageId);
							GUILayout.EndHorizontal();

							GUILayout.BeginHorizontal();
							GUILayout.Label("Icon: sprite id in sprites (-1 off)");
							rooms.setups[id].spriteId = EditorGUILayout.IntSlider(rooms.setups[id].spriteId, -1, main.sprites.Count - 1);
							GUILayout.EndHorizontal();

							GUILayout.BeginHorizontal();
							GUILayout.BeginVertical();
							GUILayout.Label("Events when enter: ");
							rooms.setups[id].eventsOnEnter = GUILayout.TextArea(rooms.setups[id].eventsOnEnter);
							GUILayout.EndVertical();
							GUILayout.BeginVertical();
							GUILayout.Label("Events when exit: ");
							rooms.setups[id].eventsOnExit = GUILayout.TextArea(rooms.setups[id].eventsOnExit);
							GUILayout.EndVertical();
							GUILayout.EndHorizontal();

							GUILayout.BeginHorizontal();
							GUILayout.Label("Interactions title: ");
							rooms.setups[id].IterationLogo = GUILayout.TextArea(rooms.setups[id].IterationLogo);
							GUILayout.EndHorizontal();

							GUILayout.Label("Interactions: ");
							GUILayout.BeginHorizontal();
							GUILayout.BeginVertical();
							GUILayout.Label("Points: ");
							rooms.setups[id].roomIterationPoints = GUILayout.TextArea(rooms.setups[id].roomIterationPoints);
							GUILayout.EndVertical();
							GUILayout.BeginVertical();
							GUILayout.Label("Texts: ");
							rooms.setups[id].roomIterationTexts = GUILayout.TextArea(rooms.setups[id].roomIterationTexts);
							GUILayout.EndVertical();
							GUILayout.BeginVertical();
							GUILayout.Label("Bools (optional): ");
							rooms.setups[id].roomIterationBools = GUILayout.TextArea(rooms.setups[id].roomIterationBools);
							GUILayout.EndVertical();
							GUILayout.EndHorizontal();

							GUILayout.BeginHorizontal();
							if (GUILayout.Button("Delete"))
							{
								rooms.setups.RemoveAt(id);
							}
							if (GUILayout.Button("Copy"))
							{
								rooms.setups.Add(new SLM_OW_RoomSetUp());
								rooms.setups[rooms.setups.Count - 1].name = rooms.setups[id].name;
								rooms.setups[rooms.setups.Count - 1].imageId = rooms.setups[id].imageId;
								rooms.setups[rooms.setups.Count - 1].eventsOnEnter = rooms.setups[id].eventsOnEnter;
								rooms.setups[rooms.setups.Count - 1].eventsOnExit = rooms.setups[id].eventsOnExit;
								rooms.setups[rooms.setups.Count - 1].IterationLogo = rooms.setups[id].IterationLogo;
								rooms.setups[rooms.setups.Count - 1].roomIterationPoints = rooms.setups[id].roomIterationPoints;
								rooms.setups[rooms.setups.Count - 1].roomIterationTexts = rooms.setups[id].roomIterationTexts;
								rooms.setups[rooms.setups.Count - 1].roomIterationBools = rooms.setups[id].roomIterationBools;
								rooms.setups[rooms.setups.Count - 1].show = false;
							}
							GUILayout.EndHorizontal();
						}
						EditorGUILayout.EndFoldoutHeaderGroup();
						EditorGUILayout.Space();
					}
					GUILayout.EndScrollView();

					EditorGUILayout.Space();
					GUILayout.BeginHorizontal();
					if (GUILayout.Button("Add new room"))
					{
						rooms.setups.Add(new SLM_OW_RoomSetUp());
					}
					GUILayout.EndHorizontal();

				}
				else if (tab == curs.ais)
				{
					GUILayout.BeginHorizontal();
					if (GUILayout.Button("Grid", styleB))
					{
						tab = curs.grid;
						showupdate = false;
						imagegrid = false;
					}
					if (GUILayout.Button("Room SetUp"))
					{
						tab = curs.rooms;
					}
					if (GUILayout.Button("<color=cyan>Ai</color>", styleB))
					{
						tab = curs.ais;
					}
					GUILayout.EndHorizontal();
					EditorGUILayout.Space();

					pos3 = GUILayout.BeginScrollView(pos3, false, true);
					for (int i = 0; i < rooms.ais.Count; i++)
					{
						int id = i;
						rooms.ais[id].show = EditorGUILayout.BeginFoldoutHeaderGroup(rooms.ais[id].show, id + " " + rooms.ais[id].name);

						if (rooms.ais[id].show)
						{

							GUILayout.BeginHorizontal();
							GUILayout.Label("Name: ");
							rooms.ais[id].name = GUILayout.TextField(rooms.ais[id].name);
							GUILayout.EndHorizontal();

							GUILayout.BeginHorizontal();
							GUILayout.Label("Image layer name: ");
							rooms.ais[id].layerName = GUILayout.TextField(rooms.ais[id].layerName);
							GUILayout.EndHorizontal();

							//images
							EditorGUILayout.Space();
							GUILayout.BeginVertical();
							GUILayout.BeginHorizontal();
							GUILayout.Label("Images: ");
							rooms.ais[id].imageIds = GUILayout.TextArea(rooms.ais[id].imageIds);
							GUILayout.EndHorizontal();
							GUILayout.Label("first line - default, next 'roomId imageId' -1 is off");
							GUILayout.EndVertical();

							//minimap
							GUILayout.BeginHorizontal();
							GUILayout.Label("Sprite id for minimap (-1 off)");
							rooms.ais[id].minimapObject = EditorGUILayout.IntSlider(rooms.ais[id].minimapObject, -1, main.sprites.Count - 1);
							if (rooms.ais[id].minimapObject >= 0 && rooms.ais[id].minimapObject < main.sprites.Count)
								EditorGUILayout.ObjectField("Preview", main.sprites[rooms.ais[id].minimapObject], typeof(Sprite), false);
							GUILayout.EndHorizontal();

							//events
							EditorGUILayout.Space();
							GUILayout.BeginVertical();
							GUILayout.Label("Events: ");
							rooms.ais[id].runEventsWhenStay = GUILayout.Toggle(rooms.ais[id].runEventsWhenStay, "Run events when delay");
							GUILayout.BeginHorizontal();
							GUILayout.BeginVertical();
							GUILayout.Label("Without player: ");
							rooms.ais[id].eventsInRooms = GUILayout.TextArea(rooms.ais[id].eventsInRooms);
							GUILayout.EndVertical();
							GUILayout.BeginVertical();
							GUILayout.Label("With player: ");
							rooms.ais[id].eventsInRoomsWithPlayer = GUILayout.TextArea(rooms.ais[id].eventsInRoomsWithPlayer);
							GUILayout.EndVertical();
							GUILayout.EndHorizontal();
							GUILayout.Label("format: 'roomId point'");
							GUILayout.EndVertical();

							//ai
							EditorGUILayout.Space();
							GUILayout.BeginVertical();
							GUILayout.Label("AI");
							GUILayout.BeginHorizontal();
							GUILayout.Label("Type");
							rooms.ais[id].type = EditorGUILayout.IntSlider(rooms.ais[id].type, 0, 2);
							GUILayout.EndHorizontal();
							GUILayout.Label("0 - nearest; 1 - random from list; 2 - path; 3 - dont move");
							GUILayout.EndVertical();

							if (rooms.ais[id].type == 2 || rooms.ais[id].type == 1)
							{
								GUILayout.BeginHorizontal();
								GUILayout.Label("AI path");
								rooms.ais[id].path = GUILayout.TextField(rooms.ais[id].path);
								GUILayout.EndHorizontal();
							}

							if (rooms.ais[id].type == 0)
							{
								GUILayout.BeginHorizontal();
								rooms.ais[id].canDontMove = GUILayout.Toggle(rooms.ais[id].canDontMove, "Ai can don't move");
								GUILayout.EndHorizontal();
							}

							if (rooms.ais[id].type != 2)
							{
								GUILayout.BeginHorizontal();
								rooms.ais[id].checkBools = GUILayout.Toggle(rooms.ais[id].checkBools, "Check bool line in grid");
								rooms.ais[id].checkAiLine = GUILayout.Toggle(rooms.ais[id].checkAiLine, "Check AI line in grid");
								GUILayout.EndHorizontal();
							}

							//AI speed
							EditorGUILayout.Space();
							GUILayout.Label("AI speed");
							try
							{
								GUILayout.BeginHorizontal();
								GUILayout.BeginVertical();
								GUILayout.Label("Rooms per step");
								rooms.ais[id].roomsPerStep = EditorGUILayout.IntField(rooms.ais[id].roomsPerStep);
								rooms.ais[id].stopIfTouchPlayer = GUILayout.Toggle(rooms.ais[id].stopIfTouchPlayer, "Stop moving if touch player");
								if (rooms.ais[id].stopIfTouchPlayer && rooms.ais[id].roomsPerStep > 1)
									rooms.ais[id].ignoreFirstStep = GUILayout.Toggle(rooms.ais[id].ignoreFirstStep, "Ignore first step (ai can go out of room if player entered to it)");
								GUILayout.EndVertical();
								GUILayout.Label("Delay before next step");
								rooms.ais[id].waitBeforeStep = EditorGUILayout.IntField(rooms.ais[id].waitBeforeStep);
								GUILayout.EndHorizontal();
							}
							catch { }

							EditorGUILayout.Space();
							GUILayout.BeginHorizontal();
							if (GUILayout.Button("Delete"))
							{
								rooms.ais.RemoveAt(id);
							}
							if (GUILayout.Button("Copy"))
							{
								rooms.ais.Add(new SLM_OW_AI());
								rooms.ais[rooms.ais.Count - 1].name = rooms.ais[id].name;
								rooms.ais[rooms.ais.Count - 1].active = rooms.ais[id].active;
								rooms.ais[rooms.ais.Count - 1].layerName = rooms.ais[id].layerName;
								rooms.ais[rooms.ais.Count - 1].imageIds = rooms.ais[id].imageIds;
								rooms.ais[rooms.ais.Count - 1].minimapObject = rooms.ais[id].minimapObject;
								rooms.ais[rooms.ais.Count - 1].eventsInRooms = rooms.ais[id].eventsInRooms;
								rooms.ais[rooms.ais.Count - 1].eventsInRoomsWithPlayer = rooms.ais[id].eventsInRoomsWithPlayer;
								rooms.ais[rooms.ais.Count - 1].type = rooms.ais[id].type;
								rooms.ais[rooms.ais.Count - 1].checkBools = rooms.ais[id].checkBools;
								rooms.ais[rooms.ais.Count - 1].checkAiLine = rooms.ais[id].checkAiLine;
								rooms.ais[rooms.ais.Count - 1].path = rooms.ais[id].path;
								rooms.ais[rooms.ais.Count - 1].roomsPerStep = rooms.ais[id].roomsPerStep;
								rooms.ais[rooms.ais.Count - 1].waitBeforeStep = rooms.ais[id].waitBeforeStep;
								rooms.ais[rooms.ais.Count - 1].show = false;
							}
							GUILayout.EndHorizontal();
						}
						EditorGUILayout.EndFoldoutHeaderGroup();
						EditorGUILayout.Space();
					}
					GUILayout.EndScrollView();

					EditorGUILayout.Space();
					GUILayout.BeginHorizontal();
					if (GUILayout.Button("Add new AI"))
					{
						rooms.ais.Add(new SLM_OW_AI());
					}
					GUILayout.EndHorizontal();
				}
				EditorGUILayout.Space();
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Save"))
				{
					main.rooms = rooms;
					if (main.useroomFile && main.roomsFile != null)
					{
						FilesSet.SaveStream(AssetDatabase.GetAssetPath(main.roomsFile[fileid]), JsonUtility.ToJson(rooms, true), false, false);
					}
					EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
				}
				GUILayout.EndHorizontal();
			}
			else if (main == null)
			{
				opened = false;
			}
		}
	}
}

public class SLM_OW_EditorManager :Editor
{
	[MenuItem("SLywnow/SLM/OpenWorld Editor")]
	static void SetDirection()
	{
		EditorWindow.GetWindow(typeof(SLM_OW_Editor), false, "SLM OpenWorld Editor", true);
	}
}