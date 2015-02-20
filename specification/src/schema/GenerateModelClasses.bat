rem Generates C# model classes for the core data types used by the HeD Schema Framework.
"C:\Program Files (x86)\Xsd2Code\Xsd2Code.exe" "common\datatypes.xsd" HeD.Model datatypes.cs /eit+
"C:\Program Files (x86)\Xsd2Code\Xsd2Code.exe" "knowledgeartifact\extdatatypes.xsd" HeD.Model extdatatypes.cs /eit+