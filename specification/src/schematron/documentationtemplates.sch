<?xml version="1.0" encoding="UTF-8"?>
<!--
Constraints on Documentation Templates specified for Health eDecisions
Aziz Boxwala
15 March 2013

 -->
<sch:schema xmlns:sch="http://purl.oclc.org/dsdl/schematron"
	queryBinding="xslt">
	<sch:title>Constraints specific for Health eDecisions Documentation Templates</sch:title>

	<sch:ns prefix="xs"  uri="http://www.w3.org/2001/XMLSchema" />
	<sch:ns prefix="fn"  uri="http://www.w3.org/2005/xpath-functions" />
	<sch:ns prefix="xsi" uri="http://www.w3.org/2001/XMLSchema-instance"/>
	<sch:ns prefix="hed" uri="urn:hl7-org:v3:knowledgeartifact:r1"/>
	<sch:ns prefix="dt"  uri="urn:hl7-org:v3:cdsdt:r2"/>
	
	<sch:pattern name="DOC-1: Artifact type is Documentation Template">
		<sch:rule context="/hed:knowledgeDocument/hed:metadata/hed:artifactType">
			<sch:assert test="./@value='Documentation Template'">
				The value attribute of artifact type must be 'Documentation Template'
			</sch:assert>
		</sch:rule>
	</sch:pattern>
	
	<sch:pattern name="DOC-2: Some action types are disallowed in documentation templates">
		<sch:rule context="//hed:simpleAction">
			<sch:assert test="not(@xsi:type='RemoveAction')">
				An action of this type is not allowed in a documentation template 
			</sch:assert>
		</sch:rule>
	</sch:pattern>
	
	<sch:pattern name="DOC-5: CollectInformationAction shall not incorporate precheck behavior">
		<sch:rule context="//hed:simpleAction[@xsi:type='CollectInformationAction']/hed:behaviors/hed:behavior">
			<sch:assert test="not(@xsi:type='PrecheckBehavior')">
				A CollectInformationAction in a documentation template may not include a precheck behavior 
			</sch:assert>
		</sch:rule>
	</sch:pattern>
		
	
</sch:schema>
