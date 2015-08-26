/*
	HeD Schema Framework
	Copyright (c) 2012 - 2014 Office of the National Coordinator for Health Information Technology (ONC)
	This file is licensed under a modified BSD-license which can be found in the HED_License.txt file included with this distribution.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeD.Engine.Model;
using HeD.Engine.Verification;
using HL7.FHIR.Model;

namespace FHIR.Model
{
    public class FHIRModuleRegistrar : IModuleRegistrar
    {
        public IEnumerable<Operator> Register()
        {
            return 
                new Operator[] 
                { 
                    // In(CodeableConcept, List<Code>)
                    new Operator("In", new Signature(new[] { DataTypes.ResolveType(typeof(CodeableConcept)), DataTypes.CodeList }), DataTypes.Boolean),

                    // Contains(List<Code>, CodeableConcept)
                    new Operator("Contains", new Signature(new[] { DataTypes.CodeList, DataTypes.ResolveType(typeof(CodeableConcept)) }), DataTypes.Boolean),

                    // InValueSet(CodeableConcept, List<Code>)
                    new Operator("InValueSet", new Signature(new[] { DataTypes.ResolveType(typeof(CodeableConcept)), DataTypes.CodeList }), DataTypes.Boolean),

                    //new Operator("CalculateAge", new Signature(new[] { DataTypes.ResolveType(typeof(dateTime)) }), DataTypes.Integer),
                    //new Operator("CalculateAgeAt", new Signature(new[] { DataTypes.ResolveType(typeof(dateTime)), DataTypes.DateTime }), DataTypes.Integer),
                };
        }
    }
}
