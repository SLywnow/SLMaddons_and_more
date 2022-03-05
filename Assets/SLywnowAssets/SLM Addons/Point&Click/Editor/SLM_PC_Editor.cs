using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using AutoLangEditorSLywnow;
using SLywnow;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

public class SLM_PC_Editor : EditorWindow
{
	SLM_PC_Main main;
	SLM_PC_Setup setup;

	Vector2 pos1;
	Vector2 pos2;
	bool opened;
	int presopen = -1;
	bool removeobjs;
	bool prewiew;
	bool editobject;

	private void OnEnable()
	{
		main = FindObjectOfType<SLM_PC_Main>();
	}

	void OnGUI()
	{
		if (!Application.isPlaying)
		{
			GUIStyle style = new GUIStyle();
			style.richText = true;

			GUIStyle styleB = new GUIStyle();
			styleB = GUI.skin.GetStyle("Button");
			styleB.richText = true;

			GUILayout.BeginHorizontal();
			main = EditorGUILayout.ObjectField("Main script", main, typeof(SLM_PC_Main), true) as SLM_PC_Main;
			if (GUILayout.Button("Open"))
			{
				setup = main.setup;

				opened = true;
			}
			GUILayout.EndHorizontal();

			if (main != null && opened)
			{
				GUILayout.BeginHorizontal();
				//objects
				GUILayout.BeginVertical();
				pos1 = GUILayout.BeginScrollView(pos1, false, true, GUILayout.Width(400));

				for (int i = 0; i < setup.objs.Count; i++)
				{
					int id = i;
					GUILayout.BeginHorizontal();
					setup.objs[id].show = EditorGUILayout.BeginFoldoutHeaderGroup(setup.objs[id].show, id + " " + setup.objs[id].name);
					if (setup.objs[id].spriteId >= 0 && setup.objs[id].spriteId < main.sprites.Count)
						GUILayout.Label(new GUIContent(main.sprites[setup.objs[id].spriteId].texture), GUILayout.Width(60), GUILayout.Height(60));
					GUILayout.EndHorizontal();

					if (setup.objs[id].show)
					{
						GUILayout.BeginHorizontal();
						GUILayout.Label("Name: ");
						setup.objs[id].name = GUILayout.TextField(setup.objs[id].name);
						GUILayout.EndHorizontal();

						GUILayout.BeginHorizontal();
						GUILayout.Label("ALSL name (can use bridge): ");
						setup.objs[id].alsl_name = GUILayout.TextField(setup.objs[id].alsl_name);
						GUILayout.EndHorizontal();
						if (main.C.ALSLBridge != null)
							GUILayout.Label("Preview (from bridge): " + main.C.ALSLBridge.GetStringEditor(setup.objs[id].alsl_name));
						else
						{
							string s = "Preview: ";
							foreach (string sa in ALSL_Editor_System.GetStringsByKey(setup.objs[id].alsl_name))
								s += sa + " / ";
							GUILayout.Label(s);
						}

						GUILayout.BeginHorizontal();
						GUILayout.Label("Sprite id in sprites:");
						setup.objs[id].spriteId = EditorGUILayout.IntSlider(setup.objs[id].spriteId, 0, main.sprites.Count - 1);
						GUILayout.EndHorizontal();

						GUILayout.BeginHorizontal();
						GUILayout.Label("Max amount:");
						setup.objs[id].maxAmount = EditorGUILayout.IntField(setup.objs[id].maxAmount);
						GUILayout.EndHorizontal();

						GUILayout.BeginHorizontal();
						GUILayout.Label("Point when pick: ");
						setup.objs[id].pointwhenpick = GUILayout.TextField(setup.objs[id].pointwhenpick);
						GUILayout.EndHorizontal();
						GUILayout.BeginHorizontal();
						GUILayout.Label("Point when use (from inventory): ");
						setup.objs[id].pointwhenuse = GUILayout.TextField(setup.objs[id].pointwhenuse);
						GUILayout.EndHorizontal();
						GUILayout.BeginHorizontal();
						setup.objs[id].removecountwhenuse = EditorGUILayout.IntField("Remove count when use:", setup.objs[id].removecountwhenuse);
						GUILayout.EndHorizontal();

						GUILayout.BeginHorizontal();
						if (GUILayout.Button("Copy"))
						{
							setup.objs.Add(JsonUtility.FromJson<SLM_PC_ObjectSet>(JsonUtility.ToJson(setup.objs[id])));
							setup.objs[setup.objs.Count - 1].show = false;
						}
						if (GUILayout.Button("Delete"))
						{
							setup.objs.RemoveAt(id);
						}
						GUILayout.EndHorizontal();
					}
					EditorGUILayout.EndFoldoutHeaderGroup();
					EditorGUILayout.Space();
				}

				EditorGUILayout.Space();
				if (GUILayout.Button("Add"))
				{
					setup.objs.Add(new SLM_PC_ObjectSet());
				}

				GUILayout.EndScrollView();
				GUILayout.EndVertical();


				//presets
				GUILayout.BeginVertical();
				pos2 = GUILayout.BeginScrollView(pos2, false, true);

				if (presopen == -1)
				{
					for (int i = 0; i < setup.presets.Count; i++)
					{
						int id = i;
						GUILayout.BeginHorizontal();
						if (GUILayout.Button("Open #" + id + " " + setup.presets[id].name))
						{
							presopen = i;
							removeobjs = false;
							editobject = false;
						}
						if (GUILayout.Button("Copy", GUILayout.Width(70)))
						{
							setup.presets.Add(new SLM_PC_Preset());
							setup.presets[setup.presets.Count - 1].name = setup.presets[id].name;
							setup.presets[setup.presets.Count - 1].useImageAsParrent = setup.presets[id].useImageAsParrent;
							setup.presets[setup.presets.Count - 1].layerName = setup.presets[id].layerName;
							setup.presets[setup.presets.Count - 1].objs = setup.presets[id].objs.ToArray().ToList();
						}
						if (GUILayout.Button("Delete", GUILayout.Width(70)))
						{
							setup.presets.RemoveAt(id);
						}
						GUILayout.EndHorizontal();
					}

					EditorGUILayout.Space();
					if (GUILayout.Button("Add"))
					{
						setup.presets.Add(new SLM_PC_Preset());
						setup.presets[setup.presets.Count - 1].objs = new List<SLM_PC_PresetObject>();
					}
				}
				else if (presopen < setup.presets.Count)
				{
					GUILayout.Label("Settings");
					GUILayout.BeginHorizontal();
					GUILayout.Label("Name: ");
					setup.presets[presopen].name = GUILayout.TextField(setup.presets[presopen].name);
					GUILayout.EndHorizontal();

					setup.presets[presopen].useImageAsParrent = GUILayout.Toggle(setup.presets[presopen].useImageAsParrent, "Use NovelImage layer for enable/disable and as parent (canvas group)");
					if (setup.presets[presopen].useImageAsParrent)
					{
						GUILayout.BeginHorizontal();
						GUILayout.Label("Layer name: ");
						setup.presets[presopen].layerName = GUILayout.TextField(setup.presets[presopen].layerName);
						GUILayout.EndHorizontal();
					}

					EditorGUILayout.Space();
					GUILayout.BeginHorizontal();
					if (main.item != null)
					{
						if (!prewiew)
						{
							if (GUILayout.Button("Generate preview"))
							{
								Transform par = main.parent;
								if (setup.presets[presopen].useImageAsParrent && main.C.imagesc.GetLayer(setup.presets[presopen].layerName) != null)
								{
									if (main.C.imagesc.GetLayer(setup.presets[presopen].layerName).group != null)
										par = main.C.imagesc.GetLayer(setup.presets[presopen].layerName).group.transform;
								}

								if (par != null)
								{
									if (setup.presets[presopen].objs == null)
										setup.presets[presopen].objs = new List<SLM_PC_PresetObject>();

									for (int i = 0; i < setup.presets[presopen].objs.Count; i++)
									{
										SLM_PC_PresetObject info = setup.presets[presopen].objs[i];

										SLM_PC_Object obj = Instantiate(main.item.gameObject, par).GetComponent<SLM_PC_Object>();
										obj.gameObject.SetActive(true);
										obj.gameObject.name = info.name;
										obj.GetComponent<RectTransform>().sizeDelta = info.size;
										obj.GetComponent<RectTransform>().anchoredPosition = info.pos;
										obj.GetComponent<RectTransform>().rotation = Quaternion.AngleAxis(info.rot, Vector3.forward);
										obj.GetComponent<Button>().interactable = info.interactible;
										obj.GetComponent<Image>().color = info.colorObject;

										obj.count = info.count;
										obj.HoldAsOne = info.pickAtOnce;
										obj.pickable = info.pickable;
										obj.ignoreAmount = info.ignoreAmount;
										obj.resetChanceEveryCall = info.resetChanse;
										obj.chanceToSpawn = info.chanceToSpawn;
										obj.checkBool = info.checkBool;
										obj.pointWhenClick = info.pointwhenclicking;

										obj.id = info.id;
										if (obj.id >= 0 && obj.id < setup.objs.Count)
										{
											if (setup.objs[obj.id].spriteId >= 0 && setup.objs[obj.id].spriteId < main.sprites.Count)
												obj.GetComponent<Image>().sprite = main.sprites[setup.objs[obj.id].spriteId];
										}

										obj.main = main;
									}

									prewiew = true;
								}
								else
									Debug.LogError("Parent not found!");
							}
						}
						else
						{
							if (GUILayout.Button("Close preview"))
							{
								Transform par = main.parent;
								if (setup.presets[presopen].useImageAsParrent && main.C.imagesc.GetLayer(setup.presets[presopen].layerName) != null)
								{
									if (main.C.imagesc.GetLayer(setup.presets[presopen].layerName).group != null)
										par = main.C.imagesc.GetLayer(setup.presets[presopen].layerName).group.transform;
								}

								for (int i = par.childCount - 1; i >= 0; i--)
									DestroyImmediate(par.GetChild(i).gameObject);

								prewiew = false;
							}
							if (GUILayout.Button("Close preview and save changes"))
							{
								Transform par = main.parent;
								if (setup.presets[presopen].useImageAsParrent && main.C.imagesc.GetLayer(setup.presets[presopen].layerName) != null)
								{
									if (main.C.imagesc.GetLayer(setup.presets[presopen].layerName).group != null)
										par = main.C.imagesc.GetLayer(setup.presets[presopen].layerName).group.transform;
								}

								setup.presets[presopen].objs = new List<SLM_PC_PresetObject>();
								for (int i = 0; i < par.childCount; i++)
								{
									if (par.GetChild(i).GetComponent<SLM_PC_Object>() != null)
									{
										setup.presets[presopen].objs.Add(new SLM_PC_PresetObject());
										int id = setup.presets[presopen].objs.Count - 1;
										SLM_PC_Object obj = par.GetChild(i).GetComponent<SLM_PC_Object>();

										setup.presets[presopen].objs[id].name = obj.gameObject.name;
										setup.presets[presopen].objs[id].pos = obj.GetComponent<RectTransform>().anchoredPosition;
										setup.presets[presopen].objs[id].size = new Vector2(obj.GetComponent<RectTransform>().rect.width, obj.GetComponent<RectTransform>().rect.height);
										setup.presets[presopen].objs[id].rot = obj.GetComponent<RectTransform>().rotation.eulerAngles.z;
										setup.presets[presopen].objs[id].interactible = obj.GetComponent<Button>().interactable;
										setup.presets[presopen].objs[id].colorObject = obj.GetComponent<Image>().color;

										setup.presets[presopen].objs[id].id = obj.id;
										setup.presets[presopen].objs[id].count = obj.count;
										setup.presets[presopen].objs[id].pickAtOnce = obj.HoldAsOne;
										setup.presets[presopen].objs[id].pickable = obj.pickable;
										setup.presets[presopen].objs[id].ignoreAmount = obj.ignoreAmount;
										setup.presets[presopen].objs[id].resetChanse = obj.resetChanceEveryCall;
										setup.presets[presopen].objs[id].chanceToSpawn = obj.chanceToSpawn;
										setup.presets[presopen].objs[id].checkBool = obj.checkBool;
										setup.presets[presopen].objs[id].pointwhenclicking = obj.pointWhenClick;

									}
								}

								for (int i = par.childCount - 1; i >= 0; i--)
									DestroyImmediate(par.GetChild(i).gameObject);

								prewiew = false;
							}
						}
					}
					else
					{
						GUILayout.Label("<color=red>Preview not available, set 'item' variable in inspector</color>", style);
					}
					
					GUILayout.EndHorizontal();
					
					GUILayout.BeginHorizontal();
					GUILayout.Label("Objects");
					editobject = GUILayout.Toggle(editobject, "Edit objects");
					GUILayout.EndHorizontal();

					if (!prewiew)
					{
						for (int i = 0; i < setup.presets[presopen].objs.Count; i++)
						{
							int id = i;
							SLM_PC_PresetObject obj = setup.presets[presopen].objs[id];

							GUILayout.BeginHorizontal();
							obj.show = EditorGUILayout.BeginFoldoutHeaderGroup(obj.show, id + " " + obj.name);
							if (obj.id >= 0 && obj.id < setup.objs.Count)
								if (setup.objs[obj.id].spriteId >= 0 && setup.objs[obj.id].spriteId < main.sprites.Count)
									GUILayout.Label(new GUIContent(main.sprites[setup.objs[obj.id].spriteId].texture), GUILayout.Width(60), GUILayout.Height(60));
							GUILayout.EndHorizontal();

							if (obj.show)
							{
								if (!editobject)
								{
									GUILayout.Label("Name: " + obj.name);
									GUILayout.Label("Position: " + obj.pos.x + "/" + obj.pos.x + " Size: " + obj.size.x + "/" + obj.size.x + " Rotation: " + obj.rot);
									GUILayout.Label("Color: #" + ColorUtility.ToHtmlStringRGBA(obj.colorObject));
									if (obj.id >= 0 && obj.id < setup.objs.Count)
										GUILayout.Label("Object: " + setup.objs[obj.id].name);
									else
										GUILayout.Label("Object: unknown");
									GUILayout.Label("Count: " + obj.count + " Hold as one: " + obj.pickAtOnce.ToString());
									GUILayout.Label("Spawn chance: " + obj.chanceToSpawn + "%  Reset every call: " +obj.resetChanse);
									GUILayout.Label("Interactible: " + obj.interactible.ToString());
									GUILayout.Label("Pickable: " + obj.pickable.ToString() + " Ignore Amount "+ obj.ignoreAmount.ToString());
									GUILayout.Label("Bool string: " + obj.checkBool);
									GUILayout.Label("Point when clicking: " + obj.pointwhenclicking);
								}
								else
								{
									GUILayout.BeginHorizontal();
									GUILayout.Label("Name: ");
									setup.presets[presopen].objs[id].name = GUILayout.TextField(obj.name);
									GUILayout.EndHorizontal();

									GUILayout.BeginHorizontal();
									setup.presets[presopen].objs[id].pos = EditorGUILayout.Vector2Field("Position: ", obj.pos);
									setup.presets[presopen].objs[id].size = EditorGUILayout.Vector2Field("Size: ", obj.size);
									setup.presets[presopen].objs[id].rot = EditorGUILayout.FloatField("Rotation: ", obj.rot);
									GUILayout.EndHorizontal();

									GUILayout.BeginHorizontal();
									setup.presets[presopen].objs[id].colorObject = EditorGUILayout.ColorField("Color: ",obj.colorObject);
									GUILayout.EndHorizontal();

									GUILayout.BeginHorizontal();
									if (setup.objs.Count > 0)
									{
										GUILayout.Label("ObjectId: ");
										setup.presets[presopen].objs[id].id = EditorGUILayout.IntSlider(obj.id, 0, setup.objs.Count - 1);
									}
									else
										GUILayout.Label("ObjectId: add some object first");
									GUILayout.EndHorizontal();

									GUILayout.BeginHorizontal();
									GUILayout.Label("Count: ");
									setup.presets[presopen].objs[id].count = EditorGUILayout.IntField(obj.count);
									setup.presets[presopen].objs[id].pickAtOnce = GUILayout.Toggle(obj.pickAtOnce, "Hold as one");
									GUILayout.EndHorizontal();

									GUILayout.BeginHorizontal();
									GUILayout.Label("Spawn chance: ");
									setup.presets[presopen].objs[id].chanceToSpawn = EditorGUILayout.IntSlider(obj.chanceToSpawn, 1, 100);
									setup.presets[presopen].objs[id].resetChanse = GUILayout.Toggle(obj.resetChanse, "Reset every call");
									GUILayout.EndHorizontal();

									GUILayout.BeginHorizontal();
									setup.presets[presopen].objs[id].interactible = GUILayout.Toggle(obj.interactible, "Interactible");
									setup.presets[presopen].objs[id].pickable = GUILayout.Toggle(obj.pickable, "Pickable");
									GUILayout.EndHorizontal();

									GUILayout.BeginHorizontal();
									setup.presets[presopen].objs[id].ignoreAmount = GUILayout.Toggle(obj.ignoreAmount, "Ignore Anount when picking");
									GUILayout.EndHorizontal();

									GUILayout.BeginHorizontal();
									GUILayout.Label("Bool string (vw): ");
									setup.presets[presopen].objs[id].checkBool = GUILayout.TextField(obj.checkBool);
									GUILayout.EndHorizontal();

									GUILayout.BeginHorizontal();
									GUILayout.Label("Point when clicking: ");
									setup.presets[presopen].objs[id].pointwhenclicking = GUILayout.TextField(obj.pointwhenclicking);
									GUILayout.EndHorizontal();

									GUILayout.BeginVertical();
									if (GUILayout.Button("▲", GUILayout.Width(100)))
									{
										int to = id - 1;
										if (to >= 0)
											setup.presets[presopen].objs.Move(id, to);
									}
									if (GUILayout.Button("▼", GUILayout.Width(100)))
									{
										int to = id + 1;
										if (to < setup.presets[presopen].objs.Count)
											setup.presets[presopen].objs.Move(id, to);
									}
									GUILayout.EndVertical();

									EditorGUILayout.Space();
									GUILayout.BeginHorizontal();
									if (GUILayout.Button("Copy"))
									{
										setup.presets[presopen].objs.Add(JsonUtility.FromJson<SLM_PC_PresetObject>(JsonUtility.ToJson(obj)));
										setup.presets[presopen].objs[setup.presets[presopen].objs.Count - 1].show = false;
									}
									if (GUILayout.Button("Delete"))
									{
										setup.presets[presopen].objs.RemoveAt(id);
									}
									GUILayout.EndHorizontal();
								}
							}

							EditorGUILayout.EndFoldoutHeaderGroup();
						}

						if (editobject)
						{
							EditorGUILayout.Space();
							if (GUILayout.Button("Add Object"))
							{
								setup.presets[presopen].objs.Add(new SLM_PC_PresetObject());
							}
						}
					}
					else
						GUILayout.Label("<color=red>Object editor not aviable in preview mode</color>", style);

					EditorGUILayout.Space();
					if (GUILayout.Button("Close"))
					{
						if (prewiew)
						{
							Transform par = main.parent;
							if (setup.presets[presopen].useImageAsParrent && main.C.imagesc.GetLayer(setup.presets[presopen].layerName) != null)
							{
								if (main.C.imagesc.GetLayer(setup.presets[presopen].layerName).group != null)
									par = main.C.imagesc.GetLayer(setup.presets[presopen].layerName).group.transform;
							}

							for (int i = par.childCount - 1; i >= 0; i--)
								DestroyImmediate(par.GetChild(i).gameObject);

							prewiew = false;
						}

						presopen = -1;
					}
				}
				else
					presopen = -1;

				GUILayout.EndScrollView();
				GUILayout.EndVertical();
				GUILayout.EndHorizontal();

				EditorGUILayout.Space();
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Save"))
				{
					main.setup = setup;
					EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
				}
				GUILayout.EndHorizontal();
			}
		}
	}
}

public class SLM_PC_EditorManager : Editor
{
	[MenuItem("SLywnow/SLM/Point&Click Editor")]
	static void SetDirection()
	{
		EditorWindow.GetWindow(typeof(SLM_PC_Editor), false, "SLM Point&Click Editor", true);
	}
}
