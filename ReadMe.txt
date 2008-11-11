XSD Diagram is a free xml schema definition diagram viewer (http://regis.cosnier.free.fr).

Version 0.5 Copyright © 2006-2008 Régis Cosnier, All Rights Reserved.

This program is free software and may be distributed
according to the terms of the GNU General Public License (GPL).


FEATURES:

- GPL
- Need of the Microsoft Framework.NET 2.0 (if not already install)
- Display the elements, the groups and the attributes
- Show the text/HTML documentation of element and attribute when available
- Print the diagram
- Export the diagram to emf
- Zoom the diagram
- Registration in the Windows Explorer contextual menu
- Drag'n drop a file from explorer


QUICK START:

- Open an xsd file.
- The xsd file and all its dependencies files are loaded in tab pages.
- Either:
	- Select a toplevel element in the toolbar (The first one is already selected).
	- Push the add button to put the element on the diagram
- Or double click in the right panel list.
- Then, on the diagram element, click on the + box.


COMMAND LINE USAGE: 

> XSDDiagram.exe [xsd file]
 or drag'n drop a file on XSDDiagram.exe


TODO LIST:

- Tooltips above the diagram element with a summary (xpath/attributes/doc) (display 200ms after the mouse move -> avoid 100 %CPU)
- Element selection in the diagram + move from one element to another with the arrow key
- Multi-selection (i.e.: to remove several element at once)
- Save the current UI state (open file/diagram/zoom/...)
- Download xsd by specifying an Url instead of loading it from the file system
- XML sample (skeleton) generation
- Download .dtd dependency file


CHANGES:

version 0.5 (2008-11-11)
- The element panel has been added again. This is not very user friendly because this should not be editable.
- The contextual menu in element list has an entry: "Add to diagramm" + drag'n drop on the diagram.

version 0.4 (2007-03-10)
- Add contextual menu in the panels to copy the list/selected line in the clipboard
- Displays enumerate type in a new panel
- The element panel has been removed
- The combobox must be wider or the same size as the widest element
- Fix an exception if no printer install when clicking on print setup/preview
- Fix an exception if selecting the attribute '*' in the XMLSchema.xsd schema file
- Fix a bug about bad simple content element displays
- Fix Ctrl+Tab that did not work in the browser view
- Fix some attributes not display
- Fix an exception on loading a dependent xml document
- Fix when selecting a browser view, the browser should have the focus
- Fix some zoom bound issues

version 0.3 (2006-11-20)
- Allow to edit the attributes label in order to copy a label in the clipboard
- Put *.xsd as default load extension
- Put xpath (/) instead of chevrons in the path
- Add the abstract element support
- Zoom accuracy
- Fix some bugs (sequence/choice/group not always display in complexType)

version 0.2 (2006-10-09)
- Automatic download of non local import
- Print per page
- Vast virtual scrolling diagram
- Top/Center/Bottom alignments
- Put chevrons in the path
- Fix some bugs (simple type and documentation space)

version 0.1 (2006-09-14)
- First version


LICENSE:

Copyright (c) 2006-2008 Regis COSNIER, All Rights Reserved.

This program is free software and may be distributed
according to the terms of the GNU General Public License (GPL).
