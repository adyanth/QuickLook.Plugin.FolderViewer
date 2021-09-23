// Copyright © 2020 Paddy Xu, Frank Becker
// 
// This file is part of QuickLook program.
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;

namespace QuickLook.Plugin.FolderViewer
{
    public class FileEntry : IComparable<FileEntry>
    {
        private readonly FileEntry _parent;

        public FileEntry(string name, bool isFolder, FileEntry parent = null)
        {
            Name = name;
            IsFolder = isFolder;

            _parent = parent;
            _parent?.Children.Add(this, false);
        }

        public SortedList<FileEntry, bool> Children { get; set; } = new SortedList<FileEntry, bool>();

        public string Name { get; set; }
        public bool IsFolder { get; set; }
        public ulong Size { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string FullPath { get; set; }
        public int Level { get; set; }

        public int CompareTo(FileEntry other)
        {
            if (IsFolder == other.IsFolder)
                return string.Compare(Name, other.Name, StringComparison.CurrentCulture);

            if (IsFolder)
                return -1;

            return 1;
        }

        public override string ToString()
        {
            if (IsFolder)
                return $"{Name}";

            return $"{Name},{IsFolder},{Size},{ModifiedDate}";
        }
    }
}