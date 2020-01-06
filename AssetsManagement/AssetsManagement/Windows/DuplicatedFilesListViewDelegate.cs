using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace AssetsManagement.Editor.Windows
{
	public class FileItem : TreeViewItem {
		public string FileName { get; set; }
		
		public FileItem(int id) : base(id){ }
	}

	public class DuplicatedFilesListViewDelegate : IListViewDelegate<FileItem> {

		Dictionary<FileData, List<string>> duplicatedFiles = new Dictionary<FileData, List<string>>();

		public string IgnoredExtensions = "";

		public MultiColumnHeader Header => new MultiColumnHeader(new MultiColumnHeaderState(new[]{
			new MultiColumnHeaderState.Column{ headerContent = new GUIContent("Duplicated files") }
		}));


		public List<TreeViewItem> GetData()
		{
			return RawData.Cast<TreeViewItem>().ToList();
		}

		public List<TreeViewItem> GetSortedData(int columnIndex, bool isAscending)
			=> GetSortedData0(columnIndex, isAscending).Cast<TreeViewItem>().ToList();

		private IEnumerable<FileItem> GetSortedData0(int columnIndex, bool isAscending)
		{
			switch (columnIndex){
				case 0:
					return isAscending
						? RawData.OrderBy(item => item.FileName)
						: RawData.OrderByDescending(item => item.FileName);
				default:
					throw new ArgumentOutOfRangeException(nameof(columnIndex), columnIndex, null);
			}
		}

		List<FileItem> innerFileListItem = new List<FileItem>();
		private int? selectedId;

		private List<FileItem> RawData 
		{
			get
			{
				if (innerFileListItem.Count > 0)
					return innerFileListItem;

				// find duplicated files
				FindDuplicates(new DirectoryInfo(Application.dataPath));

				// select duplicates
				var duplicatedFilesCollections = duplicatedFiles.Values.Where(v => v.Count > 1);

				foreach (var duplicatedFileList in duplicatedFilesCollections)
				{
					innerFileListItem.Add(new FileItem(innerFileListItem.Count) { FileName = string.Join(" - ", duplicatedFileList) });
				}

				return innerFileListItem;
			}
		}
		
		public void Draw(Rect rect, int columnIndex, FileItem data, bool selected){
			var labelStyle = selected ? EditorStyles.whiteLabel : EditorStyles.label;
			labelStyle.alignment = TextAnchor.MiddleLeft;
			
			switch (columnIndex){
				case 0 : 
					EditorGUI.LabelField(rect, data.FileName.ToString(), labelStyle);
					break;
				case 1 : 
					//EditorGUI.LabelField(rect, $"{data.Firstname} selected {selected}", labelStyle);
					break;
				case 2 : 
					//EditorGUI.LabelField(rect, data.Lastname, labelStyle);
					break;
				case 3 : 
					//EditorGUI.LabelField(rect, data.Age.ToString(), labelStyle);
					break;
				default: 
					throw new ArgumentOutOfRangeException(nameof(columnIndex), columnIndex, null);
			}
		}

		public void DeleteItem()
		{
			if (!selectedId.HasValue)
				return;

			innerFileListItem.RemoveAt(selectedId.Value);
		}

		public void Clear()
		{
			duplicatedFiles.Clear();
			innerFileListItem.Clear();
		}

		public void OnItemClick(int id)
		{
			this.selectedId = id;
		}

		public void OnContextClick()
		{
			
		}


		private void FindDuplicates(DirectoryInfo folder)
		{
			foreach (var fileInfo in folder.GetFiles())
			{
				if (IgnoredExtensions.Contains(fileInfo.Extension))
					continue;

				var fileData = new FileData
				{
					Name = fileInfo.Name + "." + fileInfo.Extension,
					Length = fileInfo.Length
				};

				if (!duplicatedFiles.ContainsKey(fileData))
				{
					duplicatedFiles.Add(fileData, new List<string>());
				}

				duplicatedFiles[fileData].Add(fileInfo.FullName);
			}

			foreach (var folderInfo in folder.GetDirectories())
			{
				FindDuplicates(folderInfo);
			}
		}
	}
}