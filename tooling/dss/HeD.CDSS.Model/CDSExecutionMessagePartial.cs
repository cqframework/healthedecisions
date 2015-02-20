/*
	HeD CDS Service Example Implementation
	Copyright (c) 2012 - 2013 Office of the National Coordinator for Health Information Technology (ONC)
	This file is licensed under a modified BSD-license which can be found in the HED_License.txt file included with this distribution.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeD.CDSS.Model
{
    public partial class CDSExecutionMessage
    {
        public bool IsExceptionLevel()
        {
            return level.valueSpecified && (level.value == CDSExecutionMessageLevel.Error || level.value == CDSExecutionMessageLevel.Fatal);
        }
    }
}
