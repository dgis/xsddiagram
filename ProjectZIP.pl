use warnings;
use Archive::Zip;

chomp($projectName = `dir /b *.vcproj *.csproj`);
$projectName = substr($projectName, 0, -7);

($sec,$min,$hour,$mday,$mon,$year) = localtime(time);
$year += 1900;
#$filename = "$projectName-$year-$mon-$mday.zip";
$filename = sprintf("%s-%04d-%02d-%02d.zip", $projectName, $year, $mon + 1, $mday);

if(-e $filename)
{ 
	print "The archive $filename already exists\n";
	$filename = "$projectName-$year-$mon-$mday-$hour-$min-$sec.zip";
	print "-> the archive will be $filename\n";
}

$zip = Archive::Zip->new();
$zip->addTree('.', '');
$zip->writeToFileNamed("$filename");

if(-e $filename)
{ 
	print "The archive $filename has been successfully created.\n";
}
else
{
	print "Error will creating the archive $filename.\n";
}

print "Type any key to quit\n";
getc(STDIN);
