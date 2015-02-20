/*
	HeD Schema Framework
	Copyright (c) 2012 - 2013 Office of the National Coordinator for Health Information Technology (ONC)
	This file is licensed under a modified BSD-license which can be found in the HED_License.txt file included with this distribution.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeD.Engine.Model;
using HeD.Engine.Translation;

using CREFModel = CREF.Model;
using AllscriptsModel = Allscripts.Model;

namespace CREF.Translation
{
	// Current CREF integration Model understands the following types
		//Patient - vMR.EvaluatedPerson
		//Person - vMR.EvaluatedPerson???
		//Encounter - vMR.Encounter
		//Allergy - vMR.Problem? vMR.Specimen? vMR.AdverseEvent?
		//Problem - vMR.Problem
		//Result - vMR.ObservationResult
		//Immunization - vMR.SubstanceAdministrationEvent?
		//Medication - vMR.SubstanceAdministrationProposal?
		//VitalSignObservation - vMR.ObservationResult
		//Procedure - vMR.Procedure
		//PlanOfCare - Goal?
		//FamilyHistory - Problem?
		//SocialHistory - Problem?

	public abstract class ModelTranslator
	{
		protected object GetPropertyExpression(TranslationContext context, ASTNode node, string path)
		{
			var result = new CREFModel.PropertyExpression();
			result.Path = path;

			if (node.Children.Count > 0)
			{
				result.Item = context.TranslateNode(node.Children[0]);
			}

			return result;
		}

		protected object GetCalculateAgeExpression(TranslationContext context, object result)
		{
			var calculateAge = new CREFModel.CalculateAge();

			calculateAge.Items.Add(result);

			var binaryExpression = new CREFModel.BinaryExpression();

			// Returns as years, so multiply by days/year to get consistent results with the physical quantity translator
			binaryExpression.Operator = CREFModel.BinaryOperator.opMultiply;
			binaryExpression.OperatorSpecified = true;
			binaryExpression.Items.Add(calculateAge);

			var multiplier = new CREFModel.ValueExpression();
			multiplier.Type = CREFModel.ValueType.Decimal;
			multiplier.TypeSpecified = true;
			multiplier.Value = Convert.ToString(365.25m);

			binaryExpression.Items.Add(multiplier);

			return binaryExpression;
		}
	}

	public abstract class ClinicalItemBaseTranslator : ModelTranslator
	{
		protected object GetFirstExpression(TranslationContext context, object source)
		{
			var first = new CREFModel.UnaryExpression();

			first.Item = source;
			first.Operator = CREFModel.UnaryOperator.opFirst;
			first.OperatorSpecified = true;

			return first;
		}

		protected object TranslateIdReference(TranslationContext context, ObjectType sourceType, ASTNode node, string path)
		{
			// Reference the IDs property with a First expression
			var result = GetPropertyExpression(context, node, "IDs");

			result = GetFirstExpression(context, result);

			// TODO: Issue a warning that an arbitrary ID is being selected

			return result;
		}

		protected object TranslateCodeReference(TranslationContext context, ObjectType sourceType, ASTNode node, string path)
		{
			// Reference the Codes property with a First expression
			var result = GetPropertyExpression(context, node, "Codes");

			result = GetFirstExpression(context, result);

			// TODO: Issue a warning that an arbitrary Code is being selected

			return result;
		}
	}

	// BodySite
	// DietQualifier
	// DoseRestriction
	// Documentation
	// AdministrableSubstanceSimple // Medication // TODO: Semantics will need to collapse the reference to this type
	// AdministrableSubstance // Medication // TODO: Semantics will need to collapse the reference to this type
	// EntitySimple
	// Entity

	// EvaluatedPerson // Patient // TODO: Assumes the EvaluatedPerson is the patient
	public class PatientTranslator : ModelTranslator, IModelTranslator
	{
		#region IModelTranslator Members

		public ObjectType TransformModel(TranslationContext context, ObjectType sourceType)
		{
 			return (ObjectType)CREFModel.DataTypes.ResolveType(typeof(AllscriptsModel.Patient));
		}

		protected object TranslateAgeReference(TranslationContext context, ASTNode node, string path)
		{
			var result = GetPropertyExpression(context, node, path);

			result = GetCalculateAgeExpression(context, result);

			return result;
		}

		public object TransformModelPath(TranslationContext context, ObjectType sourceType, ASTNode node, string path)
		{
			switch (path)
			{
				//demographics.birthTime : TS
				case "demographics.birthTime" : return GetPropertyExpression(context, node, "DateOfBirth");

				//demographics.age : PQ
				case "demographics.age" : return TranslateAgeReference(context, node, "DateOfBirth");

				//demographics.gender : CD
				case "demographics.gender" : return GetPropertyExpression(context, node, "Gender"); // TODO: Gender mapping

				//demographics.race : List<CD>
				//demographics.ethnicity : List<CD>
				
				//demographics.name : List<EN>
				case "demographics.name" : return GetPropertyExpression(context, node, "Name"); // TODO: Name mapping

				//demographics.address : List<AD>
                case "demographics.address" : return GetPropertyExpression(context, node, "Address");

				//demographics.telecom : List<TEL>
				//demographics.isDeceased : BL
				//demographics.ageAtDeath : PQ
				//demographics.preferredLanguage : CD
				//relatedEntity : List<RelatedEntity>
				//clinicalStatements : EvaluatedPersonClinicalStatements
				//clinicalStatementRelationships : List<ClinicalStatementRelationship>
				//clinicalStatementEntityInRoleRelationships : List<EntityRelationship>
				//entityRelationships : List<EntityRelationship>
				//entityLists : EvaluatedPersonEntityLists
				default: throw new NotSupportedException(String.Format("Referenced property ({0}.{1}) does not have an equivalent CREF representation.", sourceType.Name, path));
			}
		}

		#endregion
	}

    // AD - Address
    public class AddressTranslator : ModelTranslator, IModelTranslator
    {
        #region IModelTranslator Members

        public ObjectType TransformModel(TranslationContext context, ObjectType sourceType)
        {
 			return (ObjectType)CREFModel.DataTypes.ResolveType(typeof(AllscriptsModel.StructuredAddress));
        }

        public object TransformModelPath(TranslationContext context, ObjectType sourceType, ASTNode node, string path)
        {
            switch (path)
            {
                case "use" :
                {
                    // Translate access to use as a list, because it is a list in the vMR representation
                    var list = new CREFModel.ListExpression();
                    list.Items.Add(GetPropertyExpression(context, node, "AddressType"));
                    return list;
                }

                case "part" : 
                default : throw new NotImplementedException("Access to the part element is not yet implemented.");
            }
        }

        #endregion
    }

    // ADXP - Address Part
    public class AddressPartTranslator : ModelTranslator, IModelTranslator
    {
        #region IModelTranslator Members

        public ObjectType TransformModel(TranslationContext context, ObjectType sourceType)
        {
            // TODO: The result type is actually String, not an object type..., this should be okay because it's never going to be the result of a ClinicalRequest...
            throw new NotImplementedException();
        }

        public object TransformModelPath(TranslationContext context, ObjectType sourceType, ASTNode node, string path)
        {
            // This translation expects the ADXP (Address Part) to be accessed through an HeD expression of:
                // First(Filter(Property(, 'part'), Equal(Property(, 'type'), <vMR Address Type Code>)))
            // This would translate to direct property access of the relevant value in the StructuredAddress type of the Allscripts Model
            var firstNode = node.Children.FirstOrDefault(c => c.Name == "source" && c.NodeType.GetLocalName() == "First");
            if (firstNode != null)
            {
                var filterNode = firstNode.Children.FirstOrDefault(c => c.Name == "source" && c.NodeType.GetLocalName() == "Filter");
                if (filterNode != null)
                {
                    var propertyNode = filterNode.Children.FirstOrDefault(c => c.Name == "source" && c.NodeType.GetLocalName() == "Property" && c.GetAttribute<string>("path") == "part");
                    if (propertyNode != null)
                    {
                        var equalNode = filterNode.Children.FirstOrDefault(c => c.Name == "condition" && c.NodeType.GetLocalName() == "Equal");
                        if (equalNode != null)
                        {
                            var propertyTypeNode = equalNode.Children.FirstOrDefault(c => c.Name == "operand" && c.NodeType.GetLocalName() == "Property" && c.GetAttribute<string>("path") == "type");
                            if (propertyTypeNode != null)
                            {
                                var partTypeNode = equalNode.Children.FirstOrDefault(c => c.NodeType.GetLocalName() == "StringLiteral" || c.NodeType.GetLocalName() == "Literal");
                                if (partTypeNode != null)
                                {
                                    var partType = partTypeNode.GetAttribute<string>("value");
                                    switch (partType)
                                    {
                                        // AddressLine
                                        case "AL" : return GetPropertyExpression(context, propertyNode, "AddressLine1"); 

                                        // Additional Locator
                                        case "ADL" : break;
                                        
                                        // Unit Identifier
                                        case "UNID" : break; 
                                        
                                        // Unit Designator
                                        case "UNIT" : break;
                                        
                                        // Delivery Address Line
                                        case "DAL" : return GetPropertyExpression(context, propertyNode, "AddressLine1");

                                        // Delivery Installation Type
                                        case "DINST" : break;

                                        // Delivery Installation Area
                                        case "DINSTA" : break;

                                        // Delivery Installation Qualifier
                                        case "DINSTQ" : break;

                                        // Delivery Mode
                                        case "DMOD" : break;

                                        // Delivery Mode Identifier
                                        case "DMODID" : break;

                                        // Street Address Line
                                        case "SAL" : return GetPropertyExpression(context, propertyNode, "AddressLine1");

                                        // Building Number
                                        case "BNR" : break;

                                        // Building Number Suffix
                                        case "BNS" : break;
 
                                        // Street Name
                                        case "STR" : break;
 
                                        // Street Name Base
                                        case "STB" : break;
 
                                        // Street Type
                                        case "STTYP" : break;
 
                                        // Direction
                                        case "DIR" : break;
 
                                        // Intersection
                                        case "INT" : break;
 
                                        // Care of
                                        case "CAR" : break;
 
                                        // Census Tract
                                        case "CEN" : break;
 
                                        // Country
                                        case "CNT" : return GetPropertyExpression(context, propertyNode, "Country");

                                        // County or Parish
                                        case "CPA" : return GetPropertyExpression(context, propertyNode, "CountyCode");

                                        // Municipality or City
                                        case "CTY" : return GetPropertyExpression(context, propertyNode, "City");

                                        // Delimiter
                                        case "DEL" : break;

                                        // Post Office Box
                                        case "POB" : break;

                                        // Precinct
                                        case "PRE" : break;

                                        // State
                                        case "STA" : return GetPropertyExpression(context, propertyNode, "StateOrProvince");

                                        // Zip
                                        case "ZIP" : return GetPropertyExpression(context, propertyNode, "PostalCode");

                                        // Delivery Point Identifier
                                        case "DPID" : break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // If we make it here, translation failed because the pattern was not what was expected.
            throw new InvalidOperationException("Could not translated Adress Part reference to a representation within CREF.");
        }

        #endregion
    }

	// FacilitySimple
	// Facility
	// OrganizationSimple
	// Organization
	// PersonSimple
	// Person
	// SpecimenSimple
	// Specimen
	
    // RelatedClinicalStatement
    public class RelatedClinicalStatementTranslator : ModelTranslator, IModelTranslator
    {
        #region IModelTranslator Members

        public ObjectType TransformModel(TranslationContext context, ObjectType sourceType)
        {
            // There is no CREF equivalent for this class, should be okay though because it should never be the result type of a ClinicalRequest.
            throw new NotImplementedException();
        }

        public object TransformModelPath(TranslationContext context, ObjectType sourceType, ASTNode node, string path)
        {
            switch (path)
            {
                //case "adverseEvent" :
                //case "deniedAdverseEvent" :
                
                case "encounterEvent" : return GetRelatedClinicalStatement(context, node, "EncounterID", CREFModel.PatientQueryType.Encounter);

                //case "missedAppointement" :
                //case "scheduledAppointement" :
                //case "appointmentRequest" :
                //case "appointmentProprosal" :
                //case "goal" :
                //case "goalProposal" :
                //case "observationResult" :
                //case "unconductedObservation" :

                case "observationOrder" : return GetRelatedClinicalStatement(context, node, "LabOrderID", CREFModel.PatientQueryType.LabOrder);

                //case "observationProposal" :
                //case "problem" :
                //case "deniedProblem" :
                //case "procedureEvent" :
                //case "undeliveredProcedure" :
                //case "scheduledProcedure" :
                //case "procedureOrder" :
                //case "procedureProposal" :
                //case "substanceAdministrationEvent" :
                //case "undeliveredSubstanceAdministrationProposal" :
                //case "substanceAdministrationOrder" :
                //case "substanceAdministrationProposal" :
                //case "substanceDispensationEvent" :
                //case "supplyEvent" :
                //case "undeliveredSupply" :
                //case "supplyOrder" :
                //case "supplyProposal" :
                default : throw new NotImplementedException(String.Format("Unimplemented related clinical statement reference: {0}.", path));
            }
        }

        #endregion

        private ASTNode GetRelatedClinicalStatementPropertyNode(ASTNode node)
        {
            if (node.NodeType.GetLocalName() == "Property" && node.GetAttribute<string>("path") == "relatedClinicalStatement")
            {
                return node;
            }

            foreach (var child in node.Children)
            {
                return GetRelatedClinicalStatementPropertyNode(child);
            }

            return null;
        }

        private object GetRelatedClinicalStatement(TranslationContext context, ASTNode node, string idPropertyName, CREFModel.PatientQueryType type)
        {
            var propertyNode = GetRelatedClinicalStatementPropertyNode(node);
            if (propertyNode == null)
            {
                throw new InvalidOperationException("Could not resolve related clinical statement property access node.");
            }

            var id = new CREFModel.PropertyExpression();
            var source = propertyNode.Children.FirstOrDefault();
            if (source != null)
            {
                id.Item = context.TranslateNode(source);
            }
            id.Path = idPropertyName;

            var request = new CREFModel.RequestExpression();
            request.Cardinality = CREFModel.RequestCardinality.Multiple;
            request.CardinalitySpecified = true;

            request.Type = type;
            request.TypeSpecified = true;

            var filter = new CREFModel.FilterExpression();
            filter.Items.Add(request);
                    
            var condition = new CREFModel.BinaryExpression();
            condition.Operator = CREFModel.BinaryOperator.opEqual;
            condition.OperatorSpecified = true;

            var idReference = new CREFModel.PropertyExpression();
            idReference.Path = "ID";
            condition.Items.Add(idReference);
            condition.Items.Add(id);

            filter.Items.Add(condition);

            var first = new CREFModel.UnaryExpression();
            first.Item = filter;
            first.Operator = CREFModel.UnaryOperator.opFirst;
            first.OperatorSpecified = true;

            return first;
        }
    }

	// RelatedEntity
	// ClinicalStatementRelationship
	// EntityRelationship
	// VMR

	// AdverseEvent 
	// DeniedAdverseEvent
	public class AllergyTranslator : ClinicalItemBaseTranslator, IModelTranslator
	{
		#region IModelTranslator Members

		public ObjectType TransformModel(TranslationContext context, ObjectType sourceType)
		{
			return (ObjectType)CREFModel.DataTypes.ResolveType(typeof(AllscriptsModel.Allergy));
		}

		public object TransformModelPath(TranslationContext context, ObjectType sourceType, ASTNode node, string path)
		{
			switch (path)
			{
				// ClinicalStatement
				// templateId : List<CodedIdentifier>

				// id : II
				case "id" : return TranslateIdReference(context, sourceType, node, path);

				// dataSourceType : CD
				// evaluatedPersonId : II
				// extension : List<CodedNameValuePair>

				// AdverseEventBase
				// adverseEventCode : CD
				//case "adverseEventCode" : return TranslateCodeReference(context, sourceType, node, path); // TODO: Map to AdverseEventType?

				// NOTE: AdverseEventAgent is being used because it seems to map more closely to the expected Code for an Allergy clinical item in Allscripts MOM.
				// adverseEventAgent : CD
				case "adverseEventAgent" : return TranslateCodeReference(context, sourceType, node, path); // TODO: Map to AdverseEventType?

				// adverseEventTime : IVL_TS
				case "adverseEventTime" : return GetPropertyExpression(context, node, "EventDate"); // TODO: Interval?

				// documentationTime : IVL_TS
				// affectedBodySite : List<BodySite>

				// AdverseEvent
				// importance : CD
				// severity : CD
				case "severity" : return GetPropertyExpression(context, node, "SeverityCode"); // TODO: Map to ProblemSeverity?

				// adverseEventStatus : CD
				case "adverseEventStatus" : return GetPropertyExpression(context, node, "Status"); // TODO: Map adverse event status to ClinicalStatus

				// relatedEntity : List<RelatedEntity>
				// relatedClinicalStatement : List<RelatedClinicalStatement>

				// DeniedAdverseEvent
				// relatedEntity : List<RelatedEntity>
				// relatedClinicalStatement : List<RelatedClinicalStatement>

				default: throw new NotSupportedException(String.Format("Referenced property ({0}.{1}) does not have an equivalent CREF representation.", sourceType.Name, path));
			}
		}

		#endregion
	}

	// AppointmentProposal
	// AppointmentRequest

	// EncounterEvent // Encounter // With filter of Status = Complete
	public class EncounterTranslator : ClinicalItemBaseTranslator, IModelTranslator
	{
		#region IModelTranslator Members

		public ObjectType TransformModel(TranslationContext context, ObjectType sourceType)
		{
 			return (ObjectType)CREFModel.DataTypes.ResolveType(typeof(AllscriptsModel.Encounter));
		}

		public object TransformModelPath(TranslationContext context, ObjectType sourceType, ASTNode node, string path)
		{
			switch (path)
			{
				// ClinicalStatement
				// templateId : List<CodedIdentifier>

				// id : II
				case "id" : return TranslateIdReference(context, sourceType, node, path);

				// dataSourceType : CD
				// evaluatedPersonId : II
				// extension : List<CodedNameValuePair>

				// EncounterBase
				// encounterType : CD
				case "encounterType" : return TranslateCodeReference(context, sourceType, node, path);

				// EncounterEvent
				// encounterEventTime : IVL_TS
				case "encounterEventTime" : return "EncounterDate"; // with Interval selector?

				// relatedEntity : List<RelatedEntity>
				// relatedClinicalStatement : List<RelatedClinicalStatement>

				default: throw new NotSupportedException(String.Format("Referenced property ({0}.{1}) does not have an equivalent CREF representation.", sourceType.Name, path));
			}
		}

		#endregion
	}

	// MissedAppointment
	// ScheduledAppointment
	// Goal
	// GoalProposal

	// ObservationOrder // Result // With filter of Status = Complete?
	// ObservationProposal // Result // With filter of Status = Ordered?
	// ObservationResult // Result // With filter of Status = Active
	// UnconductedObservation // Result // With filter of Status = Denied?
	public class ResultTranslator : ClinicalItemBaseTranslator, IModelTranslator
	{
		#region IModelTranslator Members

		public ObjectType TransformModel(TranslationContext context, ObjectType sourceType)
		{
 			return (ObjectType)CREFModel.DataTypes.ResolveType(typeof(AllscriptsModel.Result));
		}

		public object TransformModelPath(TranslationContext context, ObjectType sourceType, ASTNode node, string path)
		{
			switch (path)
			{
				// ClinicalStatement
				// templateId : List<CodedIdentifier>

				// id : II
				case "id" : return TranslateIdReference(context, sourceType, node, path);

				// dataSourceType : CD
				// evaluatedPersonId : II
				// extension : List<CodedNameValuePair>

				// ObservationBase
				// observationFocus : CD
				case "observationFocus" : return TranslateCodeReference(context, sourceType, node, path);

				// observationMethod : CD
				// targetBodySite : BodySite

				// ObservationOrder
				// criticality : CD
				// orderEventTime : IVL_TS
				// observationTime : IVL_TS
				// repeatNumber : INT
				// relatedEntity : List<RelatedEntity>
				// relatedClinicalStatement : List<RelatedClinicalStatement>

				// ObservationProposal
				// criticality : CD
				// proposedObservationTime : IVL_TS
				// repeatNumber : INT
				// relatedEntity : List<RelatedEntity>
				// relatedClinicalStatement : List<RelatedClinicalStatement>

				// ObservationResult

				// observationEventTime : IVL_TS
				case "observationEventTime" : return GetPropertyExpression(context, node, "ResultDateTime"); // with Interval selector?

				// observationValue : ANY
				case "observationValue" : return GetPropertyExpression(context, node, "Value");

				// interpretation : List<CD>
                case "interpretation" :
                {
                    var list = new CREFModel.ListExpression();
                    list.Items.Add(GetPropertyExpression(context, node, "Interpretation")); // TODO: Interpretation mapping?
                    return list;
                }

				// relatedEntity : List<RelatedEntity>
				// relatedClinicalStatement : List<RelatedClinicalStatement>

				// UnconductedObservation
				// reason : CD
				// subjectEffectiveTime : IVL_TS
				// documentationTime : IVL_TS
				// relatedEntity : List<RelatedEntity>
				// relatedClinicalStatement : List<RelatedClinicalStatement>

				default: throw new NotSupportedException(String.Format("Referenced property ({0}.{1}) does not have an equivalent CREF representation.", sourceType.Name, path));
			}
		}

		#endregion
	}

	// DeniedProblem // Problem // With filter of Status = Denied
	// Problem // Problem // With filter of Status = Active
	public class ProblemTranslator : ClinicalItemBaseTranslator, IModelTranslator
	{
		#region IModelTranslator Members

		public ObjectType TransformModel(TranslationContext context, ObjectType sourceType)
		{
 			return (ObjectType)CREFModel.DataTypes.ResolveType(typeof(AllscriptsModel.Problem));
		}

		public object TransformModelPath(TranslationContext context, ObjectType sourceType, ASTNode node, string path)
		{
			switch (path)
			{
				// ClinicalStatement
				// templateId : List<CodedIdentifier>

				// id : II
				case "id" : return TranslateIdReference(context, sourceType, node, path);

				// dataSourceType : CD
				// evaluatedPersonId : II
				// extension : List<CodedNameValuePair>

				// ProblemBase
				// problemCode : CD
				case "problemCode" : return TranslateCodeReference(context, sourceType, node, path);

				// problemEffectiveTime : IVL_TS
				case "problemEffectiveTime" : return GetPropertyExpression(context, node, "ProblemDate"); // with Interval selector?

				// diagnosticEventTime : IVL_TS
				// affectedBodySite : List<BodySite>

				// Problem
				// importance : CD
				// severity : CD
				case "severity" : return GetPropertyExpression(context, node, "Severity"); // with conversion to string?

				// problemStatus : CD
				case "problemStatus" : return GetPropertyExpression(context, node, "Status"); // TODO: clinical status mapping

				// ageAtOnset : PQ

				// wasCauseOfDeath : BL
                case "wasCauseOfDeath" : return GetPropertyExpression(context, node, "WasOneCauseOfDeath"); // TODO: Is this correct? Seems okay, but....

				// relatedEntity : List<RelatedEntity>
				// relatedClinicalStatement : List<RelatedClinicalStatement>

				// DeniedProblem
				// relatedEntity : List<RelatedEntity>
				// relatedClinicalStatement : List<RelatedClinicalStatement>

				default: throw new NotSupportedException(String.Format("Referenced property ({0}.{1}) does not have an equivalent CREF representation.", sourceType.Name, path));
			}
		}

		#endregion
	}

	// DietProposal
	// ImagingProposal
	// LaboratoryProposal

	// ProcedureEvent // Procedure // With filter of Status = Active?
	// ProcedureOrder // Procedure // With filter of Status = Complete?
	// ProcedureProposal // Procedure // With filter of Status = Ordered
	public class ProcedureTranslator : ClinicalItemBaseTranslator, IModelTranslator
	{
		#region IModelTranslator Members

		public ObjectType TransformModel(TranslationContext context, ObjectType sourceType)
		{
 			return (ObjectType)CREFModel.DataTypes.ResolveType(typeof(AllscriptsModel.Procedure));
		}

		public object TransformModelPath(TranslationContext context, ObjectType sourceType, ASTNode node, string path)
		{
			switch (path)
			{
				// ClinicalStatement
				// templateId : List<CodedIdentifier>

				// id : II
				case "id" : return TranslateIdReference(context, sourceType, node, path);

				// dataSourceType : CD
				// evaluatedPersonId : II
				// extension : List<CodedNameValuePair>

				// ProcedureBase
				// procedureCode : CD
				case "procedureCode" : return TranslateCodeReference(context, sourceType, node, path);

				// procedureMethod : CD
				// approachBodySite : BodySite
				// targetBodySite : BodySite

				// ProcedureEvent
				// procedureTime : IVL_TS
				case "procedureTime" : return GetPropertyExpression(context, node, "procedureDate"); // with Interval selector?

				// relatedEntity : List<RelatedEntity>
				// relatedClinicalStatement : List<RelatedClinicalStatement>

				// ProcedureOrder
				// criticality : CD
				// orderEventTime : IVL_TS
				// procedureTime : IVL_TS
				// repeatNumber : INT
				// relatedEntity : List<RelatedEntity>
				// relatedClinicalStatement : List<RelatedClinicalStatement>

				// ProcedureProposal
				// criticality : CD
				// proposedProcedureTime : IVL_TS
				// repeatNumber : INT
				// originationMode : CD
				// comment : List<Documentation>
				// frequency : CD
				// timing : CD
				// prnReason : List<CD>
				// relatedEntity : List<RelatedEntity>
				// relatedClinicalStatement : List<RelatedClinicalStatement>

				default: throw new NotSupportedException(String.Format("Referenced property ({0}.{1}) does not have an equivalent CREF representation.", sourceType.Name, path));
			}
		}

		#endregion
	}

	// RespiratoryCareProposal
	// ScheduledProcedure
	// UndeliveredProcedure
	// ComplexIVProposal
	// PCAProposal

	// SubstanceAdministrationEvent // Medication // With filter of Status = Active
	// SubstanceAdministrationOrder // Medication // With filter of Status = Complete?
	// SubstanceAdministrationProposal // Medication // With filter of Status = Ordered
	// UndeliveredSubstanceAdministration // Medication // With filter of Status = Denied
	public class MedicationTranslator : ClinicalItemBaseTranslator, IModelTranslator
	{
		#region IModelTranslator Members

		public ObjectType TransformModel(TranslationContext context, ObjectType sourceType)
		{
 			return (ObjectType)CREFModel.DataTypes.ResolveType(typeof(AllscriptsModel.Medication));
		}

		public object TransformModelPath(TranslationContext context, ObjectType sourceType, ASTNode node, string path)
		{
			switch (path)
			{
				// ClinicalStatement
				// templateId : List<CodedIdentifier>

				// id : II
				case "id" : return TranslateIdReference(context, sourceType, node, path);

				// dataSourceType : CD
				// evaluatedPersonId : II
				// extension : List<CodedNameValuePair>

				// SubstanceAdministrationBase
				// substanceAdministrationGeneralPurpose : CD
				// doseType : CD
				// substance : AdministrableSubstance
				// substance.substanceCode : CD
				case "substance.substanceCode" : return TranslateCodeReference(context, sourceType, node, path);

				// substance.strength : RTO_PQ
				case "substance.strength" : return GetPropertyExpression(context, node, "ProductConcentration"); // TODO: Translate RTO_PQ

				// substance.form : CD
				case "substance.form" : return GetPropertyExpression(context, node, "ProductForm"); // TODO: translate form?

				// substance.substanceBrandCode : CD
				case "substance.substanceBrandCode" : return GetPropertyExpression(context, node, "BrandName"); // TODO: Translate brand code?

				// substance.substanceGenericCode : CD
				// substance.manufacturer: CD
				case "substance.manufacturer" : return GetPropertyExpression(context, node, "DrugManufacturer"); // TODO: Translate code

				// substance.lotNo : ST

				// doseQuantity : IVL_PQ
				case "doseQuantity" : return GetPropertyExpression(context, node, "Dose"); // TODO: Translate PQ with DoseUnits?

				// deliveryRoute : CD
				case "deliveryRoute" : return GetPropertyExpression(context, node, "Route"); // TODO: Route mapping?

				// deliveryRate : IVL_PQ // Frequency?
				// dosingPeriod : IVL_PQ // Interval?
				// dosingPeriodIntevrvalIsImportant : BL
				// deliveryMethod : CD
				// approachBodySite : BodySite
				// targetBodySite : BodySite

				// SubstanceAdministrationEvent
				// doseNumber : INT
				// administrationTimeInterval : IVL_TS
				case "administrationTimeInterval" : return GetPropertyExpression(context, node, "StartDateTime"); // TODO: StopDateTime? Duration?

				// documentationTime : IVL_TS
				// informationAttestationType : CD
				// isValid : BL
				// relatedEntity : List<RelatedEntity>
				// relatedClinicalStatement : List<RelatedClinicalStatement>

				// SubstanceAdministrationOrder
				// criticality : CD
				// doseRestriction : DoseRestriction
				// administrationTimeInterval : IVL_TS
				// dosingSig : List<CD>
				// numberFillsAllowed : INT
				// orderEventTime : IVL_TS
				// relatedEntity : List<RelatedEntity>
				// relatedClinicalStatement : List<RelatedClinicalStatement>

				// SubstanceAdministrationProposal
				// criticality : CD
				// doseRestriction : DoseRestriction
				// proposedAdministrationTimeInterval : IVL_TS
				// validAdministrationTime : IVL_TS
				// numberFillsAllowed : INT
				// originationMode : CD
				// comment : List<Documentation>
				// prnReason : List<object>
				// infuseOver : IVL_PQ
				// timing : CD
				// relatedEntity : List<RelatedEntity>
				// relatedClinicalStatement : List<RelatedClinicalStatement>

				// UndeliveredSubstanceAdministration
				// reason : CD
				// subjectEffectiveTime : IVL_TS
				// documentationTime : IVL_TS
				// relatedEntity : List<RelatedEntity>
				// relatedClinicalStatement : List<RelatedClinicalStatement>

				default: throw new NotSupportedException(String.Format("Referenced property ({0}.{1}) does not have an equivalent CREF representation.", sourceType.Name, path));
			}
		}

		#endregion
	}

	// SubstanceDispensationEvent
	// TubeFeedingProposal
	// SupplyEvent
	// SupplyOrder
	// SupplyProposal
	// UndeliveredSupply
}
