using System;
using System.IO;
using Encog.App.Analyst.Script;
using Encog.App.Analyst.Script.Normalize;
using Encog.App.Analyst.Script.Prop;
using Encog.Util;
using FileUtil = Encog.Util.File.FileUtil;

namespace Encog.App.Analyst.Report
{
    /// <summary>
    /// Produce a simple report on the makeup of the script and data to be analyued.
    /// </summary>
    ///
    public class AnalystReport
    {
        /// <summary>
        /// Used as a col-span.
        /// </summary>
        ///
        public const int FIVE_SPAN = 5;

        /// <summary>
        /// Used as a col-span.
        /// </summary>
        ///
        public const int EIGHT_SPAN = 5;

        /// <summary>
        /// The analyst to use.
        /// </summary>
        ///
        private readonly EncogAnalyst analyst;

        /// <summary>
        /// Construct the report.
        /// </summary>
        ///
        /// <param name="theAnalyst">The analyst to use.</param>
        public AnalystReport(EncogAnalyst theAnalyst)
        {
            analyst = theAnalyst;
        }

        /// <summary>
        /// Produce the report.
        /// </summary>
        ///
        /// <returns>The report.</returns>
        public String ProduceReport()
        {
            var report = new HTMLReport();

            report.BeginHTML();
            report.Title("Encog Analyst Report");
            report.BeginBody();

            report.H1("Field Ranges");
            report.BeginTable();
            report.BeginRow();
            report.Header("Name");
            report.Header("Class?");
            report.Header("Complete?");
            report.Header("Int?");
            report.Header("Real?");
            report.Header("Max");
            report.Header("Min");
            report.Header("Mean");
            report.Header("Standard Deviation");
            report.EndRow();


            foreach (DataField df  in  analyst.Script.Fields)
            {
                report.BeginRow();
                report.Cell(df.Name);
                report.Cell(Format.FormatYesNo(df.Class));
                report.Cell(Format.FormatYesNo(df.Complete));
                report.Cell(Format.FormatYesNo(df.Integer));
                report.Cell(Format.FormatYesNo(df.Real));
                report.Cell(Format.FormatDouble(df.Max, FIVE_SPAN));
                report.Cell(Format.FormatDouble(df.Min, FIVE_SPAN));
                report.Cell(Format.FormatDouble(df.Mean, FIVE_SPAN));
                report.Cell(Format.FormatDouble(df.StandardDeviation,
                                                FIVE_SPAN));
                report.EndRow();

                if (df.ClassMembers.Count > 0)
                {
                    report.BeginRow();
                    report.Cell("&nbsp;");
                    report.BeginTableInCell(EIGHT_SPAN);
                    report.BeginRow();
                    report.Header("Code");
                    report.Header("Name");
                    report.Header("Count");
                    report.EndRow();

                    foreach (AnalystClassItem item  in  df.ClassMembers)
                    {
                        report.BeginRow();
                        report.Cell(item.Code);
                        report.Cell(item.Name);
                        report.Cell(Format.FormatInteger(item.Count));
                        report.EndRow();
                    }
                    report.EndTableInCell();
                    report.EndRow();
                }
            }

            report.EndTable();

            report.H1("Normalization");
            report.BeginTable();
            report.BeginRow();
            report.Header("Name");
            report.Header("Action");
            report.Header("High");
            report.Header("Low");
            report.EndRow();


            foreach (AnalystField item_0  in  analyst.Script.Normalize.NormalizedFields)
            {
                report.BeginRow();
                report.Cell(item_0.Name);
                report.Cell(item_0.Action.ToString());
                report.Cell(Format.FormatDouble(item_0.NormalizedHigh, FIVE_SPAN));
                report.Cell(Format.FormatDouble(item_0.NormalizedLow, FIVE_SPAN));
                report.EndRow();
            }

            report.EndTable();

            report.H1("Machine Learning");
            report.BeginTable();
            report.BeginRow();
            report.Header("Name");
            report.Header("Value");
            report.EndRow();

            String t = analyst.Script.Properties
                .GetPropertyString(ScriptProperties.ML_CONFIG_TYPE);
            String a = analyst.Script.Properties
                .GetPropertyString(ScriptProperties.ML_CONFIG_ARCHITECTURE);
            String rf = analyst.Script.Properties
                .GetPropertyString(
                    ScriptProperties.ML_CONFIG_MACHINE_LEARNING_FILE);

            report.TablePair("Type", t);
            report.TablePair("Architecture", a);
            report.TablePair("Machine Learning File", rf);
            report.EndTable();

            report.H1("Files");
            report.BeginTable();
            report.BeginRow();
            report.Header("Name");
            report.Header("Filename");
            report.EndRow();

            foreach (String key  in  analyst.Script.Properties.Filenames)
            {
                String value_ren = analyst.Script.Properties
                    .GetFilename(key);
                report.BeginRow();
                report.Cell(key);
                report.Cell(value_ren);
                report.EndRow();
            }
            report.EndTable();

            report.EndBody();
            report.EndHTML();

            return (report.ToString());
        }

        /// <summary>
        /// Produce a report for a filename.
        /// </summary>
        ///
        /// <param name="filename">The filename.</param>
        public void ProduceReport(FileInfo filename)
        {
            try
            {
                String str = ProduceReport();
                FileUtil.WriteFileAsString(filename, str);
            }
            catch (IOException ex)
            {
                throw new AnalystError(ex);
            }
        }
    }
}