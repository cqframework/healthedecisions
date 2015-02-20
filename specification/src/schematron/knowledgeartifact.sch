<?xml version="1.0" encoding="UTF-8"?>
<!--
Constraints on all Health eDecisions knowledge artifacts
Aziz Boxwala
19 March 2013

 -->
 <sch:schema xmlns:sch="http://purl.oclc.org/dsdl/schematron" 
	queryBinding="xslt2">
	<sch:title>Constraints specific for Health eDecisions ECA Rules</sch:title>

	<sch:ns prefix="xs"  uri="http://www.w3.org/2001/XMLSchema" />
	<sch:ns prefix="fn"  uri="http://www.w3.org/2005/xpath-functions" />
	<sch:ns prefix="xsi" uri="http://www.w3.org/2001/XMLSchema-instance"/>
	<sch:ns prefix="hed" uri="urn:hl7-org:v3:knowledgeartifact:r1"/>
	<sch:ns prefix="dt"  uri="urn:hl7-org:v3:cdsdt:r2"/>
	
 
	<sch:pattern name="MET-1: Schema identifier">
		<sch:rule context="/hed:knowledgeDocument/hed:metadata/hed:schemaIdentifier">
			<sch:assert test="./@root='urn:hl7-org:v3:knowledgeartifact:r1'">
				The schemaIdentifier root value MUST be urn-colon-hl7-org-colon-v3:knowledgeartifact:r1
			</sch:assert>
		</sch:rule>
	</sch:pattern>
 
	<sch:pattern name="ACT-1a: Actions have at most one condition with ApplicableScenario role">
		<sch:rule context="//hed:simpleAction/hed:conditions">
			<sch:assert test="count(hed:condition/hed:conditionRole[@value ='ApplicableScenario'])&lt;=1">
				Exactly one condition of type ApplicableScenario MUST be present in an action
			</sch:assert>
		</sch:rule>
	</sch:pattern>

	<sch:pattern name="ACT-1b: Action groups have at most one condition with ApplicableScenario role">
		<sch:rule context="//hed:actionGroup/hed:conditions">
			<sch:assert test="count(hed:condition/hed:conditionRole[@value ='ApplicableScenario'])&lt;=1">
				Exactly one condition of type ApplicableScenario MUST be present in an action group
			</sch:assert>
		</sch:rule>
	</sch:pattern>


	<sch:pattern name="BHV-1: GroupSelectionBehavior can only be used with action groups">
		<sch:rule context="//hed:behavior[@xsi:type='GroupSelectionBehavior']">
			<sch:assert test="name(../..)='actionGroup'">
				GroupSelectionBehavior MUST be specified under action groups only
			</sch:assert>
		</sch:rule>
	</sch:pattern>
	
	<sch:pattern name="BHV-2: GroupOrganizationBehavior can only be used with action groups">
		<sch:rule context="//hed:behavior[@xsi:type='GroupOrganizationBehavior']">
			<sch:assert test="name(../..)='actionGroup'">
				GroupOrganizationBehavior MUST be specified under action groups only
			</sch:assert>
		</sch:rule>
	</sch:pattern>

	<sch:pattern name="BHV-3: PrecheckBehavior can only be used with actions">
		<sch:rule context="//hed:behavior[@xsi:type='PrecheckBehavior']">
			<sch:assert test="name(../..)='simpleAction'">
				PrecheckBehavior MUST be specified under actions only
			</sch:assert>
		</sch:rule>
	</sch:pattern>

	<sch:pattern name="BHV-4: An action group with GroupOrganizationBehavior of VisualGroup SHOULD have a non-empty title">
		<sch:rule context="//hed:behavior[@xsi:type='GroupOrganizationBehavior' and @value='VisualGroup']">
			<sch:report test="not(../../hed:title/@value)">
				An action group with GroupOrganizationBehavior of VisualGroup MUST have a non-empty title
			</sch:report>
		</sch:rule>
	</sch:pattern>

	
	<sch:pattern name="BHV-5: Sub-elements of an action group with Group Organization Behavior of SentenceGroup MUST NOT specify Required Behavior.">
		<sch:rule context="//hed:behavior[@xsi:type='GroupOrganizationBehavior' and @value='SentenceGroup']">
			<sch:assert test="count(../../hed:subElements/hed:simpleAction/hed:behaviors/hed:behavior[@xsi:type='RequiredBehavior'])=0">
				Sub-elements of an action group with Group Organization Behavior of SentenceGroup MUST NOT specify Required Behavior
			</sch:assert>
		</sch:rule>
	</sch:pattern>

	<sch:pattern name="BHV-6: An action group with Group Organization Behavior of SentenceGroup MUST specify a GroupSelectionBehavior of AtMostOne or ExactlyOne">
		<sch:rule context="//hed:actionGroup/hed:behaviors/hed:behavior[@xsi:type='GroupOrganizationBehavior' and @value='SentenceGroup']">
 			<sch:assert test="if (../hed:behavior[@xsi:type='GroupSelectionBehavior']) then ../hed:behavior[@value='AtMostOne' or @value='ExactlyOne'] else 'true'">
				An action group with Group Organization Behavior of SentenceGroup MUST specify a GroupSelectionBehavior of AtMostOne or ExactlyOne
			</sch:assert>
		</sch:rule>
	</sch:pattern>

	<sch:pattern name="BHV-7: Sub-elements of an action group with Group Organization Behavior of SentenceGroup MUST be simple actions only">
		<sch:rule context="//hed:actionGroup/hed:behaviors/hed:behavior[@xsi:type='GroupOrganizationBehavior' and @value='SentenceGroup']">
			<sch:assert test="count(../../hed:subElements/hed:simpleAction) = count(../../hed:subElements/*)">
				Sub-elements of an action group with Group Organization Behavior of SentenceGroup MUST be simple actions only
			</sch:assert>
		</sch:rule>
	</sch:pattern>

	<sch:pattern name="BHV-8: Group Selection Behavior and sub-elements whose Required Behavior is Must">
		<sch:rule context="//hed:behavior[@xsi:type='GroupSelectionBehavior' and (@value='AllOrNone' or @value='AtMostOne' or @value='ExactlyOne')]">
			<sch:assert test="count(../../hed:subElements/*/hed:behaviors/hed:behavior[@xsi:type='RequiredBehavior' and @value='Must'])=0">
				An action group with Group Selection Behavior of AllOrNone, ExactlyOne, AtMostOne MUST NOT have any sub-elements whose Required Behavior is Must
			</sch:assert>
		</sch:rule>
	</sch:pattern>


	<sch:pattern name="BHV-9: None of the HeD predefined behaviors are used at the knowledgeDocument level">
		<sch:rule context="/hed:knowledgeDocument/hed:behaviors/hed:behavior">
			<sch:assert test="not(@xsi:type ='GroupSelectionBehavior' or 
									@xsi:type='PrecheckBehavior' or 
									@xsi:type='RequiredBehavior' or
									@xsi:type='GroupOrganizationBehavior')">
				A behavior of this type is not allowed at the level of the knowledgeDocument 
			</sch:assert>
		</sch:rule>
	</sch:pattern>
	
<!-- 
An action group with Group Organization Behavior of SentenceGroup? SHOULD have at least one associated represented concept that unifies the actions in the group.
 -->	
</sch:schema>