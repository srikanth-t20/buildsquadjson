//*************************************************************************************
//*Purpose:
//  This file has model classes used by console application that converts a given tab seperated file
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
using System.Threading.Tasks;

namespace BuildSQuADjson
{
    public class squad1dot1
    {
        public List<squad1dot1_data> data;
        public string version;
    }

    public class squad1dot1_data
    {
        public string title;
        public List<squad1dot1_paragraph> paragraphs;
    }

    public class squad1dot1_paragraph
    {
        public string context;
        public List<squad1dot1_qas> qas;
    }

    public class squad1dot1_qas
    {
        public string id;
        public string question;
        //public List<squad1dot1_answer> answers;
    }

    public class squad1dot1_answer
    {
        public string answer_start;
        public string text;
    }
}
