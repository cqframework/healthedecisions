rem Generates C# model classes for the core data types used by the HeD Schema Framework.
"C:\Program Files (x86)\Xsd2Code\Xsd2Code.exe" MomTypesSimple.xsd Allscripts.Model MomTypesSimple.cs /eit+ /xa+
"C:\Program Files (x86)\Xsd2Code\Xsd2Code.exe" MomTypesBase.xsd Allscripts.Model MomTypesBase.cs /eit+ /xa+
"C:\Program Files (x86)\Xsd2Code\Xsd2Code.exe" MomTypesClinicalItem.xsd Allscripts.Model MomTypesClinicalItem.cs /eit+ /xa+
"C:\Program Files (x86)\Xsd2Code\Xsd2Code.exe" CDSMessages.xsd CREF.Model CDSMessages.cs /eit+ /xa+
"C:\Program Files (x86)\Xsd2Code\Xsd2Code.exe" CREF.xsd CREF.Model CREF.cs /eit+ /xa+

