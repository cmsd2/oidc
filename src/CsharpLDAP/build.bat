@echo off

if not exist lib   md lib

if not exist doc   md doc



if exist lib\Novell.Directory.Ldap.dll del lib\Novell.Directory.Ldap.dll



echo "Building resources..."

resgen Novell.Directory.Ldap\Properties\ResultCodeMessages.resx lib\ResultCodeMessages.resources



resgen Novell.Directory.Ldap\Properties\ExceptionMessages.resx lib\ExceptionMessages.resources



echo "Generating lib\Novell.Directory.Ldap.dll.."

csc /noconfig /w:1 /r:System.dll /target:library /resource:lib\ResultCodeMessages.resources /resource:lib\ExceptionMessages.resources /doc:doc\comments.xml /out:lib\Novell.Directory.Ldap.dll  /recurse:Novell.Directory.Ldap\*.cs



del lib\ResultCodeMessages.resources

del lib\ExceptionMessages.resources

