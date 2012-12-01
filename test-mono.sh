echo XSDDiagram.exe -r COLLADA -e 3 http://www.khronos.org/files/collada_schema_1_4_1.xsd
mono XSDDiagram.exe -r COLLADA -e 3 http://www.khronos.org/files/collada_schema_1_4_1.xsd
echo XSDDiagram.exe Tests/COLLADASchema_141.xsd
mono XSDDiagram.exe Tests/COLLADASchema_141.xsd
echo XSDDiagram.exe -r sphere -r COLLADA -e 3 Tests/COLLADASchema_141.xsd
mono XSDDiagram.exe -r sphere -r COLLADA -e 3 Tests/COLLADASchema_141.xsd
echo XSDDiagram.exe -o Tests/file.png -r COLLADA -e 3 -z 200 Tests/COLLADASchema_141.xsd
mono XSDDiagram.exe -o Tests/file.png -r COLLADA -e 3 -z 200 Tests/COLLADASchema_141.xsd
echo XSDDiagram.exe -o Tests/file.jpg -r COLLADA -e 3 -z 200 Tests/COLLADASchema_141.xsd
mono XSDDiagram.exe -o Tests/file.jpg -r COLLADA -e 3 -z 200 Tests/COLLADASchema_141.xsd
echo XSDDiagram.exe -o Tests/file.svg -r COLLADA -e 3 Tests/COLLADASchema_141.xsd
mono XSDDiagram.exe -o Tests/file.svg -r COLLADA -e 3 Tests/COLLADASchema_141.xsd
echo XSDDiagram.exe -os png -r COLLADA -e 3 -y Tests/COLLADASchema_141.xsd > Tests/stdout.png
mono XSDDiagram.exe -os png -r COLLADA -e 3 -y Tests/COLLADASchema_141.xsd > Tests/stdout.png
echo XSDDiagram.exe -os jpg -r COLLADA -e 3 -y Tests/COLLADASchema_141.xsd > Tests/stdout.jpg
mono XSDDiagram.exe -os jpg -r COLLADA -e 3 -y Tests/COLLADASchema_141.xsd > Tests/stdout.jpg
echo XSDDiagram.exe -os svg -r COLLADA -e 3 -y Tests/COLLADASchema_141.xsd > Tests/stdout.svg
mono XSDDiagram.exe -os svg -r COLLADA -e 3 -y Tests/COLLADASchema_141.xsd > Tests/stdout.svg
