start "" /wait XSDDiagram.exe -r COLLADA -e 3 http://www.khronos.org/files/collada_schema_1_4_1.xsd
start "" /wait XSDDiagram.exe Tests\COLLADASchema_141.xsd
start "" /wait XSDDiagram.exe -r sphere -r COLLADA -e 3 Tests\COLLADASchema_141.xsd
start "" /wait XSDDiagramConsole.exe -o Tests\file.png -r COLLADA -e 3 -z 200 Tests\COLLADASchema_141.xsd
start "" /wait XSDDiagramConsole.exe -o Tests\file.jpg -r COLLADA -e 3 -z 200 Tests\COLLADASchema_141.xsd
start "" /wait XSDDiagramConsole.exe -o Tests\file.emf -r COLLADA -e 3 Tests\COLLADASchema_141.xsd
start "" /wait XSDDiagramConsole.exe -o Tests\file.svg -r COLLADA -e 3 Tests\COLLADASchema_141.xsd
XSDDiagramConsole.exe -os png -r COLLADA -e 3 -y Tests\COLLADASchema_141.xsd > Tests\stdout.png
XSDDiagramConsole.exe -os jpg -r COLLADA -e 3 -y Tests\COLLADASchema_141.xsd > Tests\stdout.jpg
XSDDiagramConsole.exe -os svg -r COLLADA -e 3 -y Tests\COLLADASchema_141.xsd > Tests\stdout.svg
XSDDiagramConsole.exe -os txt -r COLLADA -e 2 -f NAME,TYPE,COMMENT Tests\COLLADASchema_141.xsd
XSDDiagramConsole.exe -h
@pause
