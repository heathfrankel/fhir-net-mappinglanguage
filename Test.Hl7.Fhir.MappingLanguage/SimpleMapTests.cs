using Hl7.Fhir.MappingLanguage;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.Specification.Source;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using static Hl7.Fhir.MappingLanguage.StructureMapUtilitiesAnalyze;

namespace Test.FhirMappingLanguage
{
    [TestClass]
    public class SimpleMapTests
    {
        public class TestWorker : IWorkerContext
        {
            IResourceResolver _source;
            public TestWorker(IResourceResolver source)
            {
                _source = source;
            }

            public ValueSet.ExpansionComponent expandVS(ValueSet vs, bool v1, bool v2)
            {
                throw new NotImplementedException();
            }

            public T fetchResource<T>(string url) where T : Resource
            {
                var result = _source.ResolveByCanonicalUri(url);
                if (result is T value)
                    return value;
                var result2 = _source.ResolveByUri(url);
                if (result2 is T value2)
                    return value2;
                return null;
            }

            public T fetchResourceWithException<T>(string url) where T : Resource
            {
                var result = _source.ResolveByCanonicalUri(url);
                if (result is T value)
                    return value;
                var result2 = _source.ResolveByUri(url);
                if (result2 is T value2)
                    return value2;
                throw new FHIRException();
            }

            public StructureDefinition fetchTypeDefinition(string code)
            {
                var uri = ModelInfo.CanonicalUriForFhirCoreType(code);
                var result = _source.ResolveByCanonicalUri(uri);
                if (result is StructureDefinition value)
                    return value;
                var result2 = _source.ResolveByUri(uri);
                if (result2 is StructureDefinition value2)
                    return value2;
                // return null;
                throw new NotImplementedException();
            }

            public string getOverrideVersionNs()
            {
                return null;
                // throw new NotImplementedException();
            }

            public ValidationResult validateCode(TerminologyServiceOptions terminologyServiceOptions, string system, string code, object value)
            {
                throw new NotImplementedException();
            }
        }

        [TestMethod]
        public void AnalyzeStructureMap()
        {
            var expression = System.IO.File.ReadAllText("C:\\Users\\brian\\Downloads\\structuremap-emcarea.registration.p.map");
            var source = new CachedResolver(new MultiResolver(
                new DirectorySource(@"c:\temp\analyzetests"),
                ZipSource.CreateValidationSource()
                ));
            var worker = new TestWorker(source);
            var parser = new StructureMapUtilitiesAnalyze(worker);
            var sm = parser.parse(expression, null);
            var analysisResult = parser.analyse(null, sm);
        }

        [TestMethod]
        public void AnalyzeStructureR3ToR4Map()
        {
            var expression = System.IO.File.ReadAllText("E:\\git\\HL7\\fhir-core-build-r5-PA\\implementations\\r3maps\\R3toR4\\StructureMap.map");
            var source = new CachedResolver(new MultiResolver(
                new DirectorySource(@"c:\temp\analyzetests"),
                ZipSource.CreateValidationSource()
                ));
            var worker = new TestWorker(source);
            var parser = new StructureMapUtilitiesAnalyze(worker);
            var sm = parser.parse(expression, null);
            var analysisResult = parser.analyse(null, sm);
        }

        [TestMethod]
        public void TransformStructureMap()
        {
            var expression = System.IO.File.ReadAllText("C:\\Users\\brian\\Downloads\\structuremap-emcarea.registration.p.map");
            var qr = new QuestionnaireResponse();
            // var parser = new StructureMapUtilitiesExecute(null);
            // var sm = parser.parse(expression, null);
            // object execResult = parser.transform(null, qr.ToTypedElement(), sm);
        }

        [TestMethod]
        public void RoundTripStructureMap()
        {
            var expression = System.IO.File.ReadAllText("C:\\Users\\brian\\Downloads\\structuremap-emcarea.registration.p.map");
            var parser = new StructureMapUtilitiesParse();
            var sm = parser.parse(expression, null);

            var xml = new FhirXmlSerializer(new SerializerSettings() { Pretty = true }).SerializeToString(sm);
            System.Diagnostics.Trace.WriteLine(xml);

            var canonicalFml = StructureMapUtilitiesParse.render(sm);
            System.Diagnostics.Trace.WriteLine(canonicalFml);

            var result2 = parser.parse(canonicalFml, null);
            var xml2 = new FhirXmlSerializer(new SerializerSettings() { Pretty = true }).SerializeToString(result2);

            System.IO.File.WriteAllText(@"c:\temp\sm1.xml", xml);
            System.IO.File.WriteAllText(@"c:\temp\sm2.xml", xml2);

            // Assert.AreEqual(xml, xml2);
            Assert.IsTrue(sm.IsExactly(result2));
        }

        [TestMethod]
        public void RoundTripStructureR3toR4Map()
        {
            var expression = System.IO.File.ReadAllText("E:\\git\\HL7\\fhir-core-build-r5-PA\\implementations\\r3maps\\R3toR4\\StructureMap.map");
            var parser = new StructureMapUtilitiesParse();
            var sm = parser.parse(expression, null);

            var xml = new FhirXmlSerializer(new SerializerSettings() { Pretty = true }).SerializeToString(sm);
            System.Diagnostics.Trace.WriteLine(xml);

            var canonicalFml = StructureMapUtilitiesParse.render(sm);
            System.Diagnostics.Trace.WriteLine(canonicalFml);

            var result2 = parser.parse(canonicalFml, null);
            var xml2 = new FhirXmlSerializer(new SerializerSettings() { Pretty = true }).SerializeToString(result2);

            System.IO.File.WriteAllText(@"c:\temp\sm1.xml", xml);
            System.IO.File.WriteAllText(@"c:\temp\sm2.xml", xml2);

            // Assert.AreEqual(xml, xml2);
            Assert.IsTrue(sm.IsExactly(result2));
        }

        [TestMethod]
        public void ParseAllR3toR4Maps()
        {
            foreach (var filename in System.IO.Directory.EnumerateFiles("E:\\git\\HL7\\fhir-core-build-r5-PA\\implementations\\r3maps", "*.map", System.IO.SearchOption.AllDirectories))
            {
                System.Diagnostics.Trace.WriteLine("-----------------------");
                System.Diagnostics.Trace.WriteLine(filename);
                var expression = System.IO.File.ReadAllText(filename);
                try
                {
                    var parser = new StructureMapUtilitiesParse();
                    var sm = parser.parse(expression, null);

                    var xml = new FhirXmlSerializer(new SerializerSettings() { Pretty = true }).SerializeToString(sm);
                    // System.Diagnostics.Trace.WriteLine(xml);

                    var canonicalFml = StructureMapUtilitiesParse.render(sm);
                    // System.Diagnostics.Trace.WriteLine(canonicalFml);

                    var result2 = parser.parse(canonicalFml, null);
                    var xml2 = new FhirXmlSerializer(new SerializerSettings() { Pretty = true }).SerializeToString(result2);

                    Assert.IsTrue(sm.IsExactly(result2));
                }
                catch(FHIRLexerException ex)
                {
                    System.Diagnostics.Trace.WriteLine(ex.Message);
                }
            }
        }
    }
}