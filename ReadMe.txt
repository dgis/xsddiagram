XSD Diagram is a free xml schema definition diagram viewer (http://regis.cosnier.free.fr).

Version 0.11 Copyright (c) 2006-2012 Regis Cosnier, All Rights Reserved.

This program is free software and may be distributed
according to the terms of the GNU General Public License (GPL).


FEATURES:

- GPL (Part of the source code are dual licensed with LGPL and MS-PL)
- Need of the Microsoft Framework.NET 2.0 (if not already install) or Mono
- Display the elements, the groups and the attributes
- Show the text/HTML documentation of element and attribute when available
- Print the diagram
- Export the diagram to SVG, PNG, JPG and EMF (EMF only with Windows)
- Zoom the diagram with the mouse wheel while holding the control key
- XML validation based on the loaded XSD file
- Registration in the Windows Explorer contextual menu
- Drag'n drop a file from explorer
- Command line image generation


QUICK START:

- Open an xsd file.
- The xsd file and all its dependencies files are loaded in tab pages.
- Either:
	- Select a toplevel element in the toolbar (The first one is already selected).
	- Push the add button to put the element on the diagram
- Or double click in the right panel list.
- Then, on the diagram element, click on the + box.


COMMAND LINE USAGE: 

> XSDDiagram.exe [-o output.svg] [-so EXTENSION] [-r RootElement]* [-e N] [-z N] [file.xsd]

or on Windows use 'XSDDiagramConsole.exe' instead of 'XSDDiagram.exe' if you need the console:

> XSDDiagramConsole.exe [-o output.svg] [-so EXTENSION] [-r RootElement]* [-e N] [-z N] [file.xsd]

Options:

-o FILE
	specifies the output image. Only '.svg' or '.png' are allowed.
	If not present, the GUI is shown.
-so EXTENSION
	specifies the output image is streamed through the standard
	output. EXTENSION can be: png, jpg or svg.
	If not present, the GUI is shown.
-r ELEMENT
	specifies the root element of the tree.
	You can put several -r options = several root elements in the tree.
-e N
	specifies the expand level (from 0 to what you want).
	Be carefull, the result image can be huge.
-z N
	specifies the zoom percentage from 10% to 1000% (only for .png image).
	Work only with the '-o', '-os png' or '-os jpg' option.
-y
	Force huge image generation without user prompt.

Example 1:
> XSDDiagramConsole.exe -o file.png -r TotoRoot -e 3 -z 200 ./folder1/toto.xsd
	will generate a PNG image from a diagram with a root element
	'TotoRoot' and expanding the tree from the root until the 3rd level.

Example 2:
> XSDDiagram.exe ./folder1/toto.xsd
	will load the xsd file in the GUI window.

Example 3:
> XSDDiagram.exe -r TotoRoot -e 2 ./folder1/toto.xsd
	will load the xsd file in the GUI window with a root element
	'TotoRoot' and expanding the tree from the root until the 2nd level.

Example 4:
> XSDDiagramConsole.exe -os svg -r TotoRoot -e 3 ./folder1/toto.xsd
	will write a SVG image in the standard output from a diagram with a root element
	'TotoRoot' and expanding the tree from the root until the 3rd level.



TODO LIST:

- Tooltips above the diagram element with a summary (xpath/attributes/doc) (display 200ms after the mouse move -> avoid 100 %CPU)
	o The optional display of attributes inside the diagram
- Columns in the element/attributes tabs for restrictions (length/pattern/enumerations) 
- Element selection in the diagram + move from one element to another with the arrow key
- Multi-selection (i.e.: to remove several element at once)
- Save the current UI state (open file/diagram/zoom/...)
- Download xsd by specifying an Url instead of loading it from the file system
- XML sample (skeleton) generation (the ability to generate random test XML files complying with the open schema)
- Download .dtd dependency file
- On Linux, the horizontal and vertical scrollbars don't appear correctly.


CHANGES:

version 0.12 (2012-09-19)
- Improve the error message in case the image is too big to be generated.
- Some element (complex type derived from a restriction) could cause an exception. These element are now display but can not be expanded.

version 0.11 (2012-07-22)
- Remove the "Order" attributes in the source file XmlSchema.cs which are imcompatible with mono > 2.6!
- Add the option "-y" to force huge image generation without user prompt.
- Fix some hashtable to dictionary issues due to the previous refactoring.

version 0.10 (2011-12-18)
- Refactor within a core library "XSDDiagrams.dll" under the LGPL/MS-PL license.
- Add the XSD Diagrams core library, thanks to Paul's refactoring !
- Add the XML validation operation using the currently loaded XSD schema in the Tools menu.
- When the WebBrowser is not available, use a TextBox instead (For Mono without WebBrowser support).
- Mono version 2.10 does not work anymore with XML deserialization :-(

version 0.9 (2011-05-17)
- Allow to expand restriction type (Thanks to Hermann).
- Fix an unicode issue with infinity character when building xsd diagram on linux.

version 0.8 (2010-10-31)
- Add support for JPG.
- Add command line options to generate PNG, JPG or SVG image without the GUI window.

version 0.7 (2010-07-14)
- Inversion of the mouse wheel direction to zoom
- Add the SVG diagram export
- Add the Tiago Daitx's code about the PNG diagram export
- Improve the diagram quality

version 0.6 (2010-06-27)
- Fix the print function.
- Add as much as possible the support for Mono 2.6.3 on Linux.
- Fix the import/include opening on Linux.
- Fix the print font clipping bug on Linux.
- Fix the tab page selection corruption on Linux.
- On Linux, the export to EMF does not work because it seems the libgdiplus does not support this feature. 

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

Copyright (c) 2006-2012 Regis COSNIER, All Rights Reserved.

This program is free software and may be distributed
according to the terms of the GNU General Public License (GPL).
