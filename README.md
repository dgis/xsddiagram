This is a fork of XSD Diagram, tailor made for the production of the [FMI Standard](https://fmi-standard.org/) documentation.

XSD Diagram is a free xml schema definition diagram viewer (http://regis.cosnier.free.fr).

Version 1.3alpha Copyright (c) 2006-2019 Regis Cosnier, All Rights Reserved.

This program is free software and may be distributed
according to the terms of the GNU General Public License (GPL).

## FEATURES:

- GPL (Part of the source code are dual licensed with LGPL and MS-PL)
- Export the diagram to TXT, SVG, PNG, JPG and EMF (EMF only with Windows)
- Zoom the diagram with the mouse wheel while holding the control key
- XML validation based on the loaded XSD file
- Command line image generation

## COMMAND LINE USAGE: 

> XSDDiagram.exe [-o output.svg] [-os EXTENSION] [-r RootElement[@namespace]]* [-e N] [-d] [-z N] [-y] [-u USERNAME] [-p PASSWORD] [file.xsd or URL]

or on Windows use 'XSDDiagramConsole.exe' instead of 'XSDDiagram.exe' if you need the console:

> XSDDiagramConsole.exe [-o output.svg] [-os EXTENSION] [-r RootElement[@namespace]]* [-e N] [-d] [-z N] [-f PATH,NAME,TYPE,NAMESPACE,COMMENT,SEQ,LASTCHILD,XSDTYPE] [-a] [-y] [-u USERNAME] [-p PASSWORD] [file.xsd or URL]

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
-d
	Display the documentation.
-c
	Draw a compact diagram.
-z N
	specifies the zoom percentage from 10% to 1000% (only for .png image).
	Work only with the '-o', '-os png' or '-os jpg' option.
-f PATH,NAME,TYPE,NAMESPACE,COMMENT,SEQ,LASTCHILD,XSDTYPE
	specifies the fields you want to output when rendering to a txt or csv file.
-a
	outputs the attributes in text mode only (.txt and .csv).
-y
	force huge image generation without user prompt.
-u USERNAME
	specifies a username to authenticate when a xsd dependency
	(import or include) is a secured url.
-p PASSWORD
	specifies a password to authenticate when a xsd dependency
	(import or include) is a secured url.


Example:
> XSDDiagramConsole.exe -o file.png -r TotoRoot -r TotoComplexType@http://mynamespace -e 3 -d -z 200 ./folder1/toto.xsd
	will generate a PNG image from a diagram with a root elements
	'TotoRoot' and 'TotoComplexType', and expanding the tree
	from the root until the 3rd level, with the documentation.

Example:
> XSDDiagramConsole.exe -os svg -r TotoRoot -e 3 ./folder1/toto.xsd
	will write a SVG image in the standard output from a diagram with a root element
	'TotoRoot' and expanding the tree from the root until the 3rd level.

Example:
> XSDDiagramConsole.exe -os txt -r TotoRoot -e 3 -f PATH,TYPE,COMMENT -a ./folder1/toto.xsd
	will write a textual representation in the standard output from a diagram with a root element
	'TotoRoot' and expanding the tree from the root until the 3rd level.


NOTES:
- With Mono on Linux, to prevent an exception with a missing assembly, please install the package "libmono-winforms2.0-cil"
(Prevent the error: Could not load file or assembly 'System.Windows.Forms').

LICENSE:

Copyright (c) 2006-2019 Regis COSNIER, All Rights Reserved.

This program is free software and may be distributed
according to the terms of the GNU General Public License (GPL).


CONTRIBUTORS:

Regis Cosnier (Initial developer)
Mathieu Malaterre (Debian and Ubuntu package)
Paul Selormey (Refactoring)
Edu Serna (searching feature)
TCH68k (for the text fileds: SEQ,LASTCHILD,XSDTYPE)
Cláudio Gomes (Tailoring for FMI Standard)
Adrian Covrig
Hermann Swart
Arjan Kloosterboer
Christian Renninger
Peter Butkovic
