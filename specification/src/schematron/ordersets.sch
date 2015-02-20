<?xml version="1.0" encoding="UTF-8"?>
<!--
Constraints on Order Sets specified for Health eDecisions
Aziz Boxwala
15 March 2013

 -->
<sch:schema xmlns:sch="http://purl.oclc.org/dsdl/schematron"
	queryBinding="xslt">
	<sch:title>Constraints specific for Health eDecisions Order Sets</sch:title>

	<sch:ns prefix="xs"  uri="http://www.w3.org/2001/XMLSchema" />
	<sch:ns prefix="fn"  uri="http://www.w3.org/2005/xpath-functions" />
	<sch:ns prefix="xsi" uri="http://www.w3.org/2001/XMLSchema-instance"/>
	<sch:ns prefix="hed" uri="urn:hl7-org:v3:knowledgeartifact:r1"/>
	<sch:ns prefix="dt"  uri="urn:hl7-org:v3:cdsdt:r2"/>
	
	<sch:pattern name="OS-1: Artifact type is Order Set">
		<sch:rule context="/hed:knowledgeDocument/hed:metadata/hed:artifactType">
			<sch:assert test="./@value='Order Set'">
				The value attribute of artifact type must be 'Order Set'
			</sch:assert>
		</sch:rule>
	</sch:pattern>
	
	<sch:pattern name="OS-2: Some action types are disallowed in order sets">
		<sch:rule context="//hed:simpleAction">
			<sch:assert test="not(@xsi:type ='FireAction' or 
									@xsi:type='UpdateAction' or 
									@xsi:type='RemoveAction')">
				An action of this type is not allowed in an order set 
			</sch:assert>
		</sch:rule>
	</sch:pattern>
</sch:schema>
