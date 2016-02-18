XSD Diagram is a free xml schema definition diagram viewer (http://regis.cosnier.free.fr).

Version 0.18alpha Copyright (c) 2006-2016 Regis Cosnier, All Rights Reserved.

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
- Drag'n drop a xsd file or url on the main window header
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

> XSDDiagram.exe [-o output.svg] [-os EXTENSION] [-r RootElement]* [-e N] [-z N] [-y] [-u USERNAME] [-p PASSWORD] [file.xsd or URL]

or on Windows use 'XSDDiagramConsole.exe' instead of 'XSDDiagram.exe' if you need the console:

> XSDDiagramConsole.exe [-o output.svg] [-os EXTENSION] [-r RootElement]* [-e N] [-z N] [-f PATH,NAME,TYPE,NAMESPACE,COMMENT] [-y] [-u USERNAME] [-p PASSWORD] [file.xsd or URL]

Options:

-o FILE
	specifies the output image. '.png','.jpg', '.svg', '.txt', '.csv' ('.emf' on Windows) are allowed.
	If not present, the GUI is shown.
-os EXTENSION
	specifies the output image is streamed through the standard
	output. EXTENSION can be: png, jpg, svg, txt, csv.
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
-f PATH,NAME,TYPE,NAMESPACE,COMMENT
	specifies the fields you want to output when rendering to a txt or csf file.
-y
	force huge image generation without user prompt.
-u USERNAME
	specifies a username to authenticate when a xsd dependency
	(import or include) is a secured url.
-p PASSWORD
	specifies a password to authenticate when a xsd dependency
	(import or include) is a secured url.


Example 1:
> XSDDiagramConsole.exe -o file.png -r TotoRoot -r TotoComplexType -e 3 -z 200 ./folder1/toto.xsd
	will generate a PNG image from a diagram with a root elements
	'TotoRoot' and 'TotoComplexType', and expanding the tree from the root until the 3rd level.

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

Example 5:
> XSDDiagramConsole.exe -os txt -r TotoRoot -e 3 -f NAME,TYPE,COMMENT ./folder1/toto.xsd
	will write a textual representation in the standard output from a diagram with a root element
	'TotoRoot' and expanding the tree from the root until the 3rd level.


NOTES:

- With Mono on Linux, to prevent an exception with a missing assembly, please install the package "libmono-winforms2.0-cil"
(Prevent the error: Could not load file or assembly 'System.Windows.Forms').


TODO LIST:

- BUG: Cascading substitution groups may appear weird.
- Add include a possibility to show the length of an element (Jörg S.)
- Add the attributes to the element in the diagram (suggested by bob)
- Tooltips above the diagram element with a summary (xpath/attributes/doc) (display 200ms after the mouse move -> avoid 100 %CPU)
	o The optional display of attributes inside the diagram
- Columns in the element/attributes tabs for restrictions (length/pattern/enumerations) 
- Element selection in the diagram + move from one element to another with the arrow key
- Multi-selection (i.e.: to remove several element at once)
- Save the current UI state (open file/diagram/zoom/...)
- XML sample (skeleton) generation (the ability to generate random test XML files complying with the open schema)
- Download .dtd dependency file


CHANGES:

version 0.18 (Not released yet)
- Add the documentation in the diagram (experimental).
- Add a close entry in the File menu.
- Add a recently opened list.
- Show the Windows Explorer registration menu only if we have the adminitrative right.
- Disable the impossible actions in the menu and the toolbar when XSD file are not loaded.
- Increase the icon resolution.
- From AlexM: oh, and allow the specification of a complex type in the command line as a root (-r element1 -r complexType2 -r ...).
- Fixed min/maxOccurs for group references (Thanks Cleric-K).
- Fixed the node expansion group of type 'All' (Thanks Carsten).
- Fix a bug when printing with margin!
- On Linux with Mono, the horizontal and vertical scrollbars should now appear.
- Fix an UTF8 bug when downloading from a URL.
- Fix bug when zooming the fonts.

version 0.17 (2015-09-02)
- Add CSV and TXT output rendering following the Christian's idea.
- Log errors in the standard error console (when launched via command line).
- Set the culture to invariant (prevent comma in floating numbers).
- Fix a typo with "Attributes".

version 0.16 (2013-01-23)
- Fix an issue to prevent the cylcles in the imported files.
- Add a dialog to download a xsd file with its URL.

version 0.15 (2013-01-13)
- Show the enumerates documentation.
- Add Ctrl+0 to reset the zoom

version 0.14 (2012-12-01)
- Accept all kind of https certificates when the xsd dependencies (import or include) point to a TLS/SSL url.
- Prompt the user to authenticate when the xsd dependencies (import or include) point toward a secured url.
- Add the corresponding command line "-u USERNAME" and "-p PASSWORD" options to authenticate the url download.
- It is now possible to drag'n drop an url pointing to a xsd file on the main window.

version 0.13

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

Copyright (c) 2006-2016 Regis COSNIER, All Rights Reserved.

This program is free software and may be distributed
according to the terms of the GNU General Public License (GPL).


CONTRIBUTORS:

Regis Cosnier (Initial developer)
Mathieu Malaterre (Debian and Ubuntu package)
Paul Selormey (Refactoring)
Adrian Covrig
Hermann Swart
Arjan Kloosterboer
Christian Renninger
Peter Butkovic
