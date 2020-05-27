using System;
using Bitwise.Game;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Bitwise.Tools
{
    class GameDataEditorWindow : EditorWindow
    {
        [MenuItem ("Bitwise/Game Data Editor")]
        public static void ShowWindow()
        {
            GetWindow(typeof(GameDataEditorWindow));
        }

        private const int kSelectionBorder = 100;
        private const int kTopBarHeight = 95;
        private const int kStatusBarHeight = 25;
        private const int kSideBarWidth = 350;
        private const int kNodeSizeX = 250;
        private const int kNodeSizeY = 80;
        private const int kNodeSpacingX = 270;
        private const int kNodeSpacingY = 120;
        private const int kTangentStrength = 25;
        private const int kAutoSaveInterval = 300;

        private enum EditingTab
        {
            PropertyEditing,
            ResourceUsageEditing,
            ResourceEditing,
            JobEditing,
            ObjectiveEditing
        }

        private GUISkin skin;
        private GUISkin Skin
        {
            get
            {
                if (skin == null)
                {
                    skin = AssetDatabase.LoadAssetAtPath("Assets/Editor/Skin/HamSkin.guiskin", typeof(GUISkin)) as GUISkin;
                }
                return skin;
            }
        }
        private GUIStyle Style(string name)
        {
            return Skin.GetStyle(name);
        }

        protected void OnGUI()
        {
            titleContent = new GUIContent("Game Data Editor");

            GUI.skin = Skin;
            if (activeGameData == null) { GameDataSelection(); }
            else { GameDataEditing(); }
            GUI.skin = null;

            if (activeGameData != null)
            {
                int epoch = GetEpochTime();
                int timeToNextAutosave = kAutoSaveInterval - (epoch - lastAutoSave);
                if (timeToNextAutosave < 0)
                {
                    Debug.Log("Autosaving...");
                    SaveGameData(false, true);
                    lastAutoSave = epoch;
                }
            }

            HandleEvents();
        }

        private GameData activeGameData;
        private EditingTab activeEditingTab = EditingTab.PropertyEditing;
        private int lastAutoSave = 0;

        private string newPropertyName = "";
        private string newResourceUsageTemplateName = "";
        private string newResourceName = "";
        private string newJobName = "";
        private string newObjectiveName = "";

        private void ResetEditorWindow()
        {
            activeGameData = null;
            activeEditingTab = EditingTab.PropertyEditing;
        }

        private void HandleEvents()
        {
            if (Event.current == null) { return; }
            switch (Event.current.type)
            {
            case EventType.MouseDrag:
                //overviewOffset += Event.current.delta;
                Repaint();
                break;
            case EventType.MouseMove:
                Repaint();
                break;
            }
        }

        private void GameDataSelection()
        {
            Rect rect = new Rect(kSelectionBorder, kSelectionBorder, position.width - kSelectionBorder * 2, position.height - kSelectionBorder * 2);
            GUILayout.BeginArea(rect, Style("box"));
            GUILayout.Label("Select Data Blob", Style("Title"));
            GUILayout.Space(4);

            List<string> gameDatum = GameData.GetAllGameDataPaths();

            if (GUILayout.Button("Create New Data Blob"))
            {
                ModalTextWindow.Popup("Name New Data Blob", LoadGameData);
            }
            GUILayout.Space(4);

            GUILayout.Label("Load", Style("SubTitle"));
            GUILayout.BeginVertical();
            for (int i = 0; i < gameDatum.Count; ++i)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(Path.GetFileName(gameDatum[i])))
                {
                    LoadGameData(gameDatum[i]);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void GameDataEditing()
        {
            wantsMouseMove = true;

            Rect topbar = new Rect(0, 0, position.width, kTopBarHeight);
            Rect statusbar = new Rect(0, position.height - kStatusBarHeight, position.width, kStatusBarHeight);
            Rect fullEditPane = new Rect(0, kTopBarHeight, position.width, position.height - kTopBarHeight - kStatusBarHeight);

            GUILayout.BeginArea(fullEditPane, Style("Box"));
            switch (activeEditingTab)
            {
            case EditingTab.PropertyEditing:
                PropertyEditing();
                break;
            case EditingTab.ResourceUsageEditing:
                ResourceUsageEditing();
                break;
            case EditingTab.ResourceEditing:
                ResourceEditing();
                break;
            case EditingTab.JobEditing:
                JobEditing();
                break;
            case EditingTab.ObjectiveEditing:
                ObjectiveEditing();
                break;
            }
            GUILayout.EndArea();

            // Render the controls for saving and loading last, so that doing a save or load in the middle of a render doesn't throw exceptions
            GUILayout.BeginArea(topbar, Style("Box"));
            RenderTopBar();
            GUILayout.EndArea();
        }

        private void PropertyEditing()
        {
            GUILayout.Label("Property Editing", Style("SubTitle"));

            GUILayout.BeginVertical();
            List<int> propertyKeys = activeGameData.PropertyKeys.ToList();
            for (int i = 0; i < propertyKeys.Count; ++i)
            {
                int propertyKey = propertyKeys[i];
                GameDataProperty property = activeGameData.GetProperty(propertyKey);
                GUILayout.BeginHorizontal(Style("box"));

                GUILayout.Label(property.Index.ToString(), GUILayout.Width(50));
                GUILayout.Label(property.PropertyType.ToString(), GUILayout.Width(100));
                property.Name = GUILayout.TextField(property.Name, GUILayout.Width(150));
                if (property.PropertyType == typeof(Single))
                {
                    ((GameDataProperty<float>) property).Value = EditorGUILayout.FloatField(((GameDataProperty<float>) property).Value, GUILayout.Width(50));
                }
                else if (property.PropertyType == typeof(Int32))
                {
                    ((GameDataProperty<int>) property).Value = EditorGUILayout.IntField(((GameDataProperty<int>) property).Value, GUILayout.Width(50));
                }
                else if (property.PropertyType == typeof(Boolean))
                {
                    ((GameDataProperty<bool>) property).Value = EditorGUILayout.Toggle(((GameDataProperty<bool>) property).Value, GUILayout.Width(50));
                }
                if (GUILayout.Button("Delete"))
                {
                    activeGameData.DeleteProperty(property.Index);
                    i--;
                }

                GUILayout.EndHorizontal();
            }
            GUILayout.Label("New Property");
            GUILayout.BeginHorizontal(Style("box"));

            GUILayout.Label("Name", GUILayout.Width(100));
            newPropertyName = GUILayout.TextField(newPropertyName, GUILayout.Width(150));

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Create Integer"))
            {
                activeGameData.AddProperty(newPropertyName, 0);
            }

            if (GUILayout.Button("Create Float"))
            {
                activeGameData.AddProperty(newPropertyName, 0f);
            }

            if (GUILayout.Button("Create Bool"))
            {
                activeGameData.AddProperty(newPropertyName, false);
            }
            GUILayout.EndVertical();
        }

        private void ResourceUsageEditing()
        {
            GUILayout.Label("Resource Usage Template Editing", Style("SubTitle"));

            GUILayout.BeginVertical();

            List<int> rutKeys = activeGameData.ResourceUsageKeys.ToList();
            for (int i = 0; i < rutKeys.Count; ++i)
            {
                int rutKey = rutKeys[i];
                ResourceUsageSpec rut = activeGameData.GetResourceUsageTemplate(rutKey);
                GUILayout.BeginHorizontal(Style("box"));

                GUILayout.Label(rut.Index.ToString(), GUILayout.Width(50));
                rut.Name = GUILayout.TextField(rut.Name, GUILayout.Width(150));
                rut.Cycles = EditorGUILayout.FloatField(rut.Cycles, GUILayout.Width(100));
                rut.MemoryRatio = EditorGUILayout.FloatField(rut.MemoryRatio, GUILayout.Width(100));
                rut.MemoryRequired = EditorGUILayout.IntField(rut.MemoryRequired, GUILayout.Width(100));
                rut.DiskRatio = EditorGUILayout.FloatField(rut.DiskRatio, GUILayout.Width(100));
                rut.DiskRequired = EditorGUILayout.IntField(rut.DiskRequired, GUILayout.Width(100));

                if (GUILayout.Button("Delete"))
                {
                    activeGameData.DeleteResourceUsageTemplate(rut.Index);
                    i--;
                }

                GUILayout.EndHorizontal();
            }
            GUILayout.Label("New Resource Usage Template");
            GUILayout.BeginHorizontal(Style("box"));

            GUILayout.Label("Name", GUILayout.Width(100));
            newResourceUsageTemplateName = GUILayout.TextField(newResourceUsageTemplateName, GUILayout.Width(150));

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Create"))
            {
                ResourceUsageSpec spec = ResourceUsageSpec.DefaultProcessUsage();
                spec.Name = newResourceUsageTemplateName;
                activeGameData.AddResourceUsageTemplate(spec);
            }
            GUILayout.EndVertical();
        }

        private void ResourceEditing()
        {
            GUILayout.Label("Resource Editing", Style("SubTitle"));

            GUILayout.BeginVertical();

            Dictionary<string, int> propertyMapping = new Dictionary<string, int>();
            List<string> propertyNames = new List<string>();
            foreach (int propertyKey in activeGameData.PropertyKeys)
            {
                GameDataProperty prop = activeGameData.GetProperty(propertyKey);
                propertyMapping[prop.Name] = prop.Index;
                propertyNames.Add(prop.Name);
            }

            List<int> resourceKeys = activeGameData.ResourceKeys.ToList();
            for (int i = 0; i < resourceKeys.Count; ++i)
            {
                int resourceKey = resourceKeys[i];
                Resource resource = activeGameData.GetResource(resourceKey);
                GUILayout.BeginHorizontal(Style("box"));

                GUILayout.Label(resource.Index.ToString(), GUILayout.Width(50));
                resource.Name = GUILayout.TextField(resource.Name, GUILayout.Width(150));

                int selectedIndex = propertyNames.IndexOf(activeGameData.GetProperty(resource.StoragePropertyIndex).Name);
                int newSelectedIndex = EditorGUILayout.Popup(selectedIndex, propertyNames.ToArray());
                if (selectedIndex != newSelectedIndex)
                {
                    resource.StoragePropertyIndex = propertyMapping[propertyNames[newSelectedIndex]];
                }

                if (GUILayout.Button("Delete"))
                {
                    activeGameData.DeleteResource(resource.Index);
                    i--;
                }

                GUILayout.EndHorizontal();
            }
            GUILayout.Label("New Resource");
            GUILayout.BeginHorizontal(Style("box"));

            GUILayout.Label("Name", GUILayout.Width(100));
            newResourceName = GUILayout.TextField(newResourceName, GUILayout.Width(150));

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Create"))
            {
                activeGameData.AddResource(GameData.MemoryCapacity, newResourceName, 0);
            }
            GUILayout.EndVertical();
        }

        private void JobEditing()
        {GUILayout.Label("Job Editing", Style("SubTitle"));

            GUILayout.BeginVertical();

            Dictionary<string, int> resourceUsageMapping = new Dictionary<string, int>();
            List<string> rutNames = new List<string>();
            foreach (int rutkey in activeGameData.ResourceUsageKeys)
            {
                ResourceUsageSpec spec = activeGameData.GetResourceUsageTemplate(rutkey);
                resourceUsageMapping[spec.Name] = spec.Index;
                rutNames.Add(spec.Name);
            }

            List<int> jobKeys = activeGameData.JobKeys.ToList();
            for (int i = 0; i < jobKeys.Count; ++i)
            {
                int jobKey = jobKeys[i];
                Job job = activeGameData.GetJob(jobKey);
                GUILayout.BeginHorizontal(Style("box"));

                GUILayout.Label(job.Index.ToString(), GUILayout.Width(50));
                job.Name = GUILayout.TextField(job.Name, GUILayout.Width(150));

                int selectedIndex = rutNames.IndexOf(job.ResourceUsage.Name);
                int newSelectedIndex = EditorGUILayout.Popup(selectedIndex, rutNames.ToArray());
                if (selectedIndex != newSelectedIndex)
                {
                    // TODO - Need to rearchitect this shit
                    //job.ResourceUsage = resourceUsageMapping[rutNames[newSelectedIndex]];
                }

                if (GUILayout.Button("Delete"))
                {
                    activeGameData.DeleteProperty(job.Index);
                    i--;
                }

                GUILayout.EndHorizontal();
            }
            GUILayout.Label("New Resource");
            GUILayout.BeginHorizontal(Style("box"));

            GUILayout.Label("Name", GUILayout.Width(100));
            newResourceName = GUILayout.TextField(newResourceName, GUILayout.Width(150));

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Create"))
            {
                activeGameData.AddResource(GameData.MemoryCapacity, newResourceName, 0);
            }
            GUILayout.EndVertical();
        }

        private void ObjectiveEditing()
        {

        }

        private void RenderTopBar()
        {
            GUILayout.Label("Editing " + activeGameData.Name, Style("Title"));

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save")) { SaveGameData(); }
            if (GUILayout.Button("Save and Close")) { SaveGameData(true); }
            if (GUILayout.Button("Close")) { ResetEditorWindow(); }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUI.enabled = (activeEditingTab != EditingTab.PropertyEditing);
            if (GUILayout.Button("Property")) { activeEditingTab = EditingTab.PropertyEditing; }
            GUI.enabled = (activeEditingTab != EditingTab.ResourceUsageEditing);
            if (GUILayout.Button("Resource Usage")) { activeEditingTab = EditingTab.ResourceUsageEditing; }
            GUI.enabled = (activeEditingTab != EditingTab.ResourceEditing);
            if (GUILayout.Button("Resource")) { activeEditingTab = EditingTab.ResourceEditing; }
            GUI.enabled = (activeEditingTab != EditingTab.JobEditing);
            if (GUILayout.Button("Job")) { activeEditingTab = EditingTab.JobEditing; }
            GUI.enabled = (activeEditingTab != EditingTab.ObjectiveEditing);
            if (GUILayout.Button("Objective")) { activeEditingTab = EditingTab.ObjectiveEditing; }
            GUI.enabled = true;
            GUILayout.EndHorizontal();
        }

        private void SaveGameData(bool close = false, bool makeBackup = false)
        {
            if (activeGameData == null)
            {
                Debug.LogError("No active game data");
                return;
            }

            string path = Path.Combine(GameData.GetGameDataPath(), activeGameData.Name);

            if (makeBackup && File.Exists(path))
            {

                File.Copy(path, Path.Combine(GetBackupPath(), String.Format("{0}_{1}", activeGameData.Name, GetEpochTime())));
            }

            GameData.Save(activeGameData, path);

            if (close)
            {
                ResetEditorWindow();
            }

            lastAutoSave = GetEpochTime();
        }
        private void LoadGameData(string name)
        {
            ResetEditorWindow();
            string path = Path.Combine(GameData.GetGameDataPath(), name);
            if (!File.Exists(path))
            {
                activeGameData = new GameData();
                activeGameData.Name = name;
                activeGameData.DefaultInit();
                SaveGameData();
            }
            else
            {
                try
                {
                    activeGameData = GameData.Load(path);
                }
                catch(Exception e)
                {
                    Debug.LogError("Failed to load game data " + name + ": " + e.Message);
                    Debug.LogError(e.StackTrace);
                }
            }

            lastAutoSave = GetEpochTime();
        }
        private string GetBackupPath()
        {
            string path = Path.Combine(GameData.GetGameDataPath(), "Backups");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        private int GetEpochTime()
        {
            DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (int)(DateTime.UtcNow - epochStart).TotalSeconds;
        }
    }
}