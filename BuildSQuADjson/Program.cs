//*************************************************************************************
//*Purpose:
//  This is the main program in console application that converts a given tab seperated file
//  with two columns - question and context into SQuAD 1.1 json format.
//
//*Created By:
//  Srikanth Thirumalasetti
//
//*Current Version on LIVE:
//  v1.0
//
//*Product Roadmap Implementation:
//  Sl# Change Request                                                          Planned Version in the Roadmap
//
//
//*History:
//  Date        Changed By          Change Description
//  04/30/20    SK                  Initial creation.
//
//
//
//*************************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BuildSQuADjson
{
    class Program
    {
        static void Main(string[] args)
        {
            string SQUAD_VERSION = "1.1";
            try
            {
                if (args.Length != 1)
                {
                    Console.WriteLine("Input file in tab seperated format is required!");
                    Thread.Sleep(5);
                    return;
                }
                string fileModelTemplateQR = args[0]; // input file with full path
                if (!File.Exists(fileModelTemplateQR))
                {
                    Console.WriteLine("Input file '{0}' NOT found at the given location!", fileModelTemplateQR);
                    Thread.Sleep(5);
                    return;
                }

                // File exists. Get question and context pairs.
                Dictionary<string, List<string>> questns_contxts = GetQuestionContextPairs(fileModelTemplateQR);
                if (questns_contxts != null && questns_contxts.Count > 0)
                {
                    // Build JSON in SQuAD 1.1 format
                    squad1dot1 squadJson = new squad1dot1();
                    squadJson.version = SQUAD_VERSION;
                    squadJson.data = new List<squad1dot1_data>();

                    // We'll build only one data node as the question and context pairs belong to one category.
                    // If multiple categories, then, we can add many data items to the list.
                    // But, in this case, we populate only one list item with questions and contexts.
                    squadJson.data.Add(new squad1dot1_data() { title = "First Set of Paragraphs", paragraphs = new List<squad1dot1_paragraph>() } );

                    // We'll have as many paragraphs as there are # of answers (contexts)
                    int contextsCnt = 0; // running count of total contexts (answers) in the input file
                    foreach(KeyValuePair<string, List<string>> pair in questns_contxts)
                    {
                        if (pair.Value != null && pair.Value.Count > 0) // one or more questions to the context (answer) 
                        {
                            // Add context (answer) to paragraphs[]
                            squad1dot1_paragraph para = new squad1dot1_paragraph() { context = pair.Key, qas = new List<squad1dot1_qas>() };

                            // Add qas (questions) to paragraphs[]
                            for (int questionsCnt = 0; questionsCnt < pair.Value.Count; questionsCnt++)
                            {
                                para.qas.Add(new squad1dot1_qas() { id = Guid.NewGuid().ToString("N"), question = pair.Value[questionsCnt] });

                                // **********************************************
                                // In future if we want to measure predictions against the inference data, we can implement the below code.
                                //  - Uncomment the below lines of code when we are populating the actual answer with answer start index in the input file.
                                ////  - Add qas[answers] with answer-start and text for each question (that can have multiple answers in SQuAD 1.1.DEV.json)
                                //para.qas[questionsCnt].answers = new List<squad1dot1_answer>();
                                //List<squad1dot1_answer> actualAnswers = GetActualAnswersStartAndTextArray(fileModelTemplateQR);
                                //foreach (squad1dot1_answer ans in actualAnswers)
                                //{
                                //    para.qas[questionsCnt].answers.Add(new squad1dot1_answer() { answer_start = ans.answer_start, text = ans.text });
                                //}
                                // **********************************************

                            }
                            squadJson.data[0].paragraphs.Add(para);
                            contextsCnt++;
                        }
                    }

                    // Write the json string to file
                    using (TextWriter tw = File.CreateText(Path.Combine(Path.GetDirectoryName(fileModelTemplateQR), String.Concat(Path.GetFileNameWithoutExtension(fileModelTemplateQR), ".json"))))
                    {
                        tw.Write(JsonConvert.SerializeObject(squadJson));
                        tw.Flush();
                        tw.Close();
                    }
                }
                else
                {
                    Console.WriteLine("There are no questions and context paragraphs in the input file. Cannot build SQuAD 1.1 json file!");
                    Thread.Sleep(5);
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred. Error is: {0}", ex.Message);
            }
        }

        /// <summary>
        /// This method reads the input file in tab sep format with 2 cols - question and context.
        /// </summary>
        /// <param name="ModelTemplateQRfile"></param>
        /// <returns>Dictionary of answer (key) and questions (values).</returns>
        private static Dictionary<string, List<string>> GetQuestionContextPairs(string ModelTemplateQRfile)
        {
            int TOTAL_COLS = 2;
            Dictionary<string, List<string>> qrs = new Dictionary<string, List<string>>(); // one answer and multiple questions

            try
            {
                if (!File.Exists(ModelTemplateQRfile))
                {
                    throw (new Exception("Input file NOT found!"));
                }

                using (StreamReader sr = File.OpenText(ModelTemplateQRfile))
                {
                    string line = String.Empty;
                    int cnt = 0;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (!string.IsNullOrEmpty(line))
                        {
                            // Check # of columns in the file
                            if (!string.IsNullOrEmpty(line.Trim()))
                            {
                                // # of columns should be equal to TOTAL_COLS
                                string[] cols = line.Trim().Split('\t');
                                if(cols.Length != TOTAL_COLS)
                                {
                                    throw (new Exception(string.Format("Invalid file! Total columns in the uploaded training file '{0}' should be {1}!", ModelTemplateQRfile, TOTAL_COLS)));
                                }
                                string q = cols[0].Trim();
                                string a = cols[1].Trim();
                                bool keyAdded = false;
                                foreach(string contxt in qrs.Keys)
                                {
                                    if(!string.IsNullOrEmpty(contxt) && contxt.ToUpper() == a.ToUpper()) // ignore case
                                    {
                                        // Key is already added. Hence add only value.
                                        keyAdded = true;
                                        break;
                                    }
                                }
                                if (keyAdded) 
                                {
                                    // Add value
                                    qrs[a].Add(q);
                                }
                                else
                                {
                                    // Add value and key
                                    qrs.Add(a, new List<string>() { q });
                                }
                            }
                        }
                        cnt++;
                    }
                    sr.Close();
                }
                return qrs;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        /// <summary>
        /// ** future implementation ** This method reads the input file in tab sep format with additional cols for answer start and actual answer.
        /// </summary>
        /// <param name="ModelTemplateQRfile"></param>
        /// <returns>List of answer start and actual answer as model object</returns>
        private static List<squad1dot1_answer> GetActualAnswersStartAndTextArray(string ModelTemplateQRfile)
        {
            return null;
        }
    }
}
