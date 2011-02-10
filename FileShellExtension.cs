//    XSDDiagram - A XML Schema Definition file viewer
//    Copyright (C) 2006-2011  Regis COSNIER
//
//    This program is free software; you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation; either version 2 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program; if not, write to the Free Software
//    Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System.Diagnostics;
using Microsoft.Win32;

namespace XSDDiagram
{
	static class FileShellExtension
	{
		public static void Register(string fileType,
			   string shellKeyName, string menuText, string menuCommand)
		{
			// create path to registry location
			string regPath = string.Format(@"{0}\shell\{1}",
										   fileType, shellKeyName);

			// add context menu to the registry
			using (RegistryKey key =
				   Registry.ClassesRoot.CreateSubKey(regPath))
			{
				key.SetValue(null, menuText);
			}

			// add command that is invoked to the registry
			using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(
				string.Format(@"{0}\command", regPath)))
			{
				key.SetValue(null, menuCommand);
			}
		}

		public static void Unregister(string fileType, string shellKeyName)
		{
			Debug.Assert(!string.IsNullOrEmpty(fileType) &&
				!string.IsNullOrEmpty(shellKeyName));

			// path to the registry location
			string regPath = string.Format(@"{0}\shell\{1}",
										   fileType, shellKeyName);

			// remove context menu from the registry
			Registry.ClassesRoot.DeleteSubKeyTree(regPath);
		}
	}
}
