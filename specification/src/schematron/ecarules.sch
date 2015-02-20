<?xml version="1.0" encoding="UTF-8"?>
<!--
Constraints on ECA Rules specified for Health eDecisions
Aziz Boxwala
8 March 2013

 -->
<sch:schema xmlns:sch="http://purl.oclc.org/dsdl/schematron"
	queryBinding="xslt">
	<sch:title>Constraints specific for Health eDecisions ECA Rules</sch:title>

	<sch:ns prefix="xs"  uri="http://www.w3.org/2001/XMLSchema" />
	<sch:ns prefix="fn"  uri="http://www.w3.org/2005/xpath-functions" />
	<sch:ns prefix="xsi" uri="http://www.w3.org/2001/XMLSchema-instance"/>
	<sch:ns prefix="hed" uri="uurn:hl7-org:v3:knowledgeartifact:r1"/>
	<sch:ns prefix="dt"  uri="urn:hl7-org:v3:cdsdt:r2"/>
	
	
	<sch:pattern name="ECA-1: Artifact type is Rule">
		<sch:rule context="/hed:knowledgeDocument/hed:metadata/hed:artifactType">
			<sch:assert test="./@value='Rule'">
				The value attribute of artifact type must be 'Rule'
			</sch:assert>
		</sch:rule>
	</sch:pattern>

	<sch:pattern name="ECA-2: One condition of type ApplicableScenario is included">
		<sch:rule context="/hed:knowledgeDocument">
			<sch:assert test="count(hed:conditions/hed:condition/hed:conditionRole[@value ='ApplicableScenario'])=1">
				Exactly one condition of type ApplicableScenario must be present in a rule
			</sch:assert>
		</sch:rule>
	</sch:pattern>
	

</sch:schema>
