using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using XSDDiagramConsole;

namespace XSDDiagramsTests
{
    [TestClass]
    public class CLITests
    {
        [TestMethod]
        public void RandomArgsGivesUsage()
        {
            string[] args = { "a", "b", "c" };
            var options = new Options(args);
            Program.Execute(options);
        }

        [TestMethod]
        [DeploymentItem(@"Resources\COLLADASchema_141.xsd")]
        public void ExerciseCLI()
        {
            var schema = @"COLLADASchema_141.xsd";
            Assert.IsTrue(File.Exists(schema));

            string[] extensions = { "png", "jpg", "emf", "svg", "txt", "csv" };

            bool[] compacts = { true, false };

            string[] expansion_levels = { "2", "3" };

            foreach (var ext in extensions)
            {
                foreach(var cmp in compacts)
                {
                    foreach (var e in expansion_levels)
                    {
                        var outfile = $"col{(cmp ? "_c" : "")}_e{e}.{ext}";
                        Assert.IsFalse(File.Exists(outfile), "");

                        string[] args = { "dummy", "-o", outfile, "-r", "COLLADA", "-e", e, schema };

                        var options = new Options(args);

                        Assert.AreEqual(outfile, options.OutputFile);

                        Program.Execute(options);

                        Assert.IsTrue(File.Exists(outfile));
                    }
                }
            }
        }

        [TestMethod]
        [DeploymentItem(@"Resources\fmi3Annotation.xsd")]
        [DeploymentItem(@"Resources\fmi3AttributeGroups.xsd")]
        [DeploymentItem(@"Resources\fmi3BuildDescription.xsd")]
        [DeploymentItem(@"Resources\fmi3InterfaceType.xsd")]
        [DeploymentItem(@"Resources\fmi3ModelDescription.xsd")]
        [DeploymentItem(@"Resources\fmi3Terminal.xsd")]
        [DeploymentItem(@"Resources\fmi3TerminalsAndIcons.xsd")]
        [DeploymentItem(@"Resources\fmi3Type.xsd")]
        [DeploymentItem(@"Resources\fmi3Unit.xsd")]
        [DeploymentItem(@"Resources\fmi3Variable.xsd")]
        [DeploymentItem(@"Resources\fmi3VariableDependency.xsd")]
        public void GenerateFMIFigures()
        {
            (string,string)[] element_schema_pairs = {
                ("fmiModelDescription", "fmi3ModelDescription.xsd"),
                ("UnitDefinitions", "fmi3ModelDescription.xsd"),
                ("BaseUnit", "fmi3ModelDescription.xsd"),
                ("DisplayUnit", "fmi3ModelDescription.xsd"),
                ("TypeDefinitions", "fmi3ModelDescription.xsd"),
                ("Float64Type", "fmi3Type.xsd"),
                ("Int32Type", "fmi3Type.xsd"),
                ("BooleanType", "fmi3Type.xsd"),
                ("BinaryType", "fmi3Type.xsd"),
                ("EnumerationType", "fmi3Type.xsd"),
                ("ClockType", "fmi3Type.xsd"),
                ("LogCategories", "fmi3ModelDescription.xsd"),
                ("DefaultExperiment", "fmi3ModelDescription.xsd"),
                ("fmiTerminalsAndIcons", "fmi3TerminalsAndIcons.xsd"),
                ("Terminals", "fmi3TerminalsAndIcons.xsd"),
                ("TerminalMemberVariable", "fmi3TerminalsAndIcons.xsd"),
                ("TerminalStreamMemberVariable", "fmi3TerminalsAndIcons.xsd"),
                ("GraphicalRepresentation", "fmi3TerminalsAndIcons.xsd"),
                ("CoordinateSystem", "fmi3TerminalsAndIcons.xsd"),
                ("Icon", "fmi3TerminalsAndIcons.xsd"),
                ("TerminalGraphicalRepresentation", "fmi3Terminal.xsd"),
                ("ModelVariables", "fmi3ModelDescription.xsd"),
                ("fmi3AbstractVariable", "fmi3ModelDescription.xsd"),
                ("Float64", "fmi3Variable.xsd"),
                ("Int32", "fmi3Variable.xsd"),
                ("Boolean", "fmi3Variable.xsd"),
                ("Binary", "fmi3Variable.xsd"),
                ("Enumeration", "fmi3Variable.xsd"),
                ("Clock", "fmi3Variable.xsd"),
                ("Annotations", "fmi3ModelDescription.xsd"),
                ("ModelStructure", "fmi3ModelDescription.xsd"),
                ("ModelExchange", "fmi3ModelDescription.xsd"),
                ("CoSimulation", "fmi3ModelDescription.xsd"),
                ("ScheduledExecution", "fmi3ModelDescription.xsd")
            };

            string[] extensions = { "png", "jpg", "emf", "svg", "txt", "csv" };

            bool[] compacts = { true, false };

            string[] expansion_levels = { "1", "2", "3" };

            foreach (var element_schema in element_schema_pairs)
            {
                var (element, schema) = element_schema;
                foreach (var ext in extensions)
                {
                    foreach (var cmp in compacts)
                    {
                        foreach (var e in expansion_levels)
                        {
                            var outfile = $"fmi_{element}_{(cmp ? "_c" : "")}_e{e}.{ext}";
                            Assert.IsFalse(File.Exists(outfile), "");

                            string[] args = { "dummy", "-o", outfile, "-r", element, "-e", e, schema };

                            var options = new Options(args);

                            Assert.AreEqual(outfile, options.OutputFile);

                            Program.Execute(options);

                            Assert.IsTrue(File.Exists(outfile));
                        }
                    }
                }
            }
        }
    }
}
