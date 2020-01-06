using System;
using UnityEditor;
using UnityEngine;

namespace AssetsManagement.Editor.Windows
{
    class FileData : IEquatable<FileData>
    {
        public string Name { get; set; }
        public long Length { get; set; }

        public override bool Equals(object obj)
        {
            return obj is FileData && Equals((FileData)obj);
        }

        public bool Equals(FileData other)
        {
            return Name == other.Name && Length == other.Length;
        }

        public override int GetHashCode()
        {
            return Name.Length ^ Length.GetHashCode();
        }
    }

    public class DuplicatedAssets : EditorWindow
    {
        static ListView<FileItem> listView;
        private DuplicatedFilesListViewDelegate dataDelegate;
        static string ignoredExtensions = ".meta";

        [MenuItem("Window/Asset Management/Duplicated assets")]

        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(DuplicatedAssets));
        }

        void OnEnable()
        {
            dataDelegate = new DuplicatedFilesListViewDelegate();
            listView = new ListView<FileItem>(dataDelegate);
            listView.Refresh();
        }

        void OnGUI()
        {
            titleContent = new GUIContent("Duplicated assets");
            var w = this.position.width;

            EditorGUILayout.BeginVertical();

            dataDelegate.IgnoredExtensions = ignoredExtensions = EditorGUILayout.TextField("Ignore extensions:", ignoredExtensions);

            // buttons
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Refresh", GUILayout.Width(100)))
            {
                dataDelegate.Clear();
                listView.Refresh();
            }

            EditorGUILayout.EndHorizontal();

            DisplayListView();

            EditorGUILayout.EndVertical();
        }

        private void DeleteItems()
        {
            dataDelegate.DeleteItem();
        }

        private void DisplayListView()
        {
            var controlRect = EditorGUILayout.GetControlRect(
                                GUILayout.ExpandHeight(true),
                                GUILayout.ExpandWidth(true));

            listView?.OnGUI(controlRect);
        }
    }
}