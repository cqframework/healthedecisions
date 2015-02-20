/*
	HeD CDS Service Example Implementation
	Copyright (c) 2012 - 2013 Office of the National Coordinator for Health Information Technology (ONC)
	This file is licensed under a modified BSD-license which can be found in the HED_License.txt file included with this distribution.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Xml.Serialization;

namespace HeD.DSS.Models
{
    public static class DSSExceptionExtensions
    {
        public static HttpResponseMessage ToHttpResponseMessage(this DSSException e)
        {
            var rm = new HttpResponseMessage(GetHttpStatusCode(e));

            rm.Content = new StringContent(e.SerializeToString(), Encoding.Unicode, "application/xml");

            return rm;
        }

        public static string SerializeToString(this DSSException e)
        {
            var serializer = new XmlSerializer(e.GetType());

            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                serializer.Serialize(writer, e);
            }

            return sb.ToString();
        }

        private static HttpStatusCode GetHttpStatusCode(DSSException e)
        {
            switch (e.GetType().Name)
            {
                case "UnsupportedLanguageException" : return HttpStatusCode.BadRequest;
                case "UnrecognizedTraitCriterionException" : return HttpStatusCode.BadRequest;
                case "UnrecognizedScopingEntityException" : return HttpStatusCode.BadRequest;
                case "UnrecognizedScopedEntityException" : return HttpStatusCode.BadRequest;
                case "UnrecognizedLanguageException" : return HttpStatusCode.BadRequest;
                case "RequiredDataNotProvidedException" : return HttpStatusCode.BadRequest;
                case "InvalidTimeZoneOffsetException" : return HttpStatusCode.BadRequest;
                case "InvalidDataFormatException" : return HttpStatusCode.BadRequest;
                case "InvalidTraitCriterionDataFormatException" : return HttpStatusCode.BadRequest;
                case "InvalidDriDataFormatException" : return HttpStatusCode.BadRequest;
                case "EvaluationException" : return HttpStatusCode.InternalServerError;
                case "DSSRuntimeException" : return HttpStatusCode.InternalServerError;
                default : return HttpStatusCode.InternalServerError;
            }
        }
    }
}