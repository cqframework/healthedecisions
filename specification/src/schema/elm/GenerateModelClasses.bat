rem Generates C# model classes for the core data types used by the HeD Schema Framework.
"C:\Program Files (x86)\Xsd2Code\Xsd2Code.exe" library.xsd CQL.ELM.Model library.cs /eit+ /xa+
"C:\Program Files (x86)\Xsd2Code\Xsd2Code.exe" types.xsd CQL.ELM.Model.Types types.cs /eit+ /xa+
"C:\Program Files (x86)\Xsd2Code\Xsd2Code.exe" cqlannotations.xsd CQL.ELM.Model.Annotations annotations.cs /eit+ /xa+